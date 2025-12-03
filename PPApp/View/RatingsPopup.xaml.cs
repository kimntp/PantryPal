using PPApp.Model;
using PPApp.Services;
using Firebase.Auth;

namespace PPApp.View;

public partial class RatingsPopup : ContentPage
{
    private readonly Recipe _recipe;
    private readonly IFirebaseAuthService _auth;

    private int _rating = 0;
    private bool _isPublic = false;  // default: private

    public RatingsPopup(Recipe recipe, IFirebaseAuthService auth)
    {
        InitializeComponent();

        _recipe = recipe;
        _auth = auth;

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

        if (_isPublic)
        {
            toggleIcon.Source = "globe.png";
            toggleLabel.Text = "Public";
        }
        else
        {
            toggleIcon.Source = "lock.png";
            toggleLabel.Text = "Private";
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var reviewText = descriptionEditor.Text;

        var user = await _auth.GetCurrentUser();

        var rating = new RecipeRating
        {
            RecipeId = _recipe.RecipeID,
            RecipeName = _recipe.Name,

            Rating = _rating,
            Review = reviewText,
            Date = DateTime.Now,
            IsPublic = _isPublic,

            UserName = user.DisplayName  // if you have this set
        };

        AppData.AddRating(rating);

        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
