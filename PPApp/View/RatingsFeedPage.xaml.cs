using PPApp.Model;
using PPApp.Service;

namespace PPApp.View;

public partial class RatingsFeedPage : ContentPage
{
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userDb;

    public List<RecipeRating> PublicRatings { get; set; }

    public RatingsFeedPage(IFirebaseAuthService auth, FirebaseUserDatabaseService userDb)
    {
        InitializeComponent();
        _auth = auth;
        _userDb = userDb;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPublicRatings();
    }

    private void LoadPublicRatings()
    {
        PublicRatings = AppData.Ratings
            .Where(r => r.IsPublic)
            .OrderByDescending(r => r.Date)
            .ToList();

        ratingsFeedView.ItemsSource = PublicRatings;
    }

    private async void OnAddRatingClicked(object sender, EventArgs e)
    {
        var user = await _auth.GetCurrentUser();

        if (user == null)
        {
            await DisplayAlert("Not Signed In", "You must sign in to add a rating.", "OK");
            return;
        }

        // Get saved recipes
        var savedRecipes = await _userDb.GetSavedRecipesByIdsAsync(user.Uid);

        if (savedRecipes == null || savedRecipes.Count == 0)
        {
            await DisplayAlert("No Saved Recipes", "Save a recipe first to rate it.", "OK");
            return;
        }

        // Open the recipe picker popup
        // This popup will automatically open RatingsPopup when a recipe is selected
        await Navigation.PushModalAsync(new RecipePickerPopup(savedRecipes, _auth));
    }
}
