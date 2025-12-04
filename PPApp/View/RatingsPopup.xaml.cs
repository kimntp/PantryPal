using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class RatingsPopup : ContentPage
{
    private readonly Recipe _recipe;
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userDb;

    private int _rating = 0;
    private bool _isPublic = false;  // default: private

    public RatingsPopup(Recipe recipe, IFirebaseAuthService auth, FirebaseUserDatabaseService userDb)
    {
        InitializeComponent();
        _recipe = recipe;
        _auth = auth;
        _userDb = userDb;

        recipeNameLabel.Text = recipe.Name;
    }

    private void OnStarClicked(object sender, EventArgs e)
    {
        int starIndex = starLayout.Children.IndexOf((Button)sender) + 1;
        _rating = starIndex;

        for (int i = 0; i < 5; i++)
        {
            ((Button)starLayout.Children[i]).Text = (i < starIndex) ? "★" : "☆";
        }
    }

    private void OnToggleVisibilityClicked(object sender, EventArgs e)
    {
        _isPublic = !_isPublic;

        toggleIcon.Source = _isPublic ? "globe.png" : "lock.png";
        toggleLabel.Text = _isPublic ? "Public" : "Private";
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var reviewText = descriptionEditor.Text;

        // Get the signed-in user safely
        var user = await _auth.GetCurrentUser();
        if (user == null || string.IsNullOrEmpty(user.Uid))
        {
            await DisplayAlert("Error", "You must be signed in to submit a rating.", "OK");
            return;
        }

        // Ensure recipe ID is valid
        if (_recipe == null)
        {
            await DisplayAlert("Error", "Invalid recipe selected.", "OK");
            return;
        }

        var rating = new RecipeRating
        {
            RecipeName = _recipe.Name,
            Rating = _rating,
            Review = reviewText,
            Date = DateTime.Now,
            IsPublic = _isPublic,
            UserName = user.DisplayName ?? "Anonymous",
            UserId = user.Uid
        };

        // Save rating to database under /userReviews/{uid}/{recipeId}/
        try
        {
            await _userDb.SaveUserRatingToDatabaseAsync(user.Uid, _recipe, rating);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save rating to DB: {ex}");
            await DisplayAlert("Error", "Failed to save rating to the database.", "OK");
        }

        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
