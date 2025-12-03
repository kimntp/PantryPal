using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class ProfilePage : ContentPage
{
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userService;
    public List<RecipeRating> PrivateRatings { get; set; } = new List<RecipeRating>();


    public ProfilePage(IFirebaseAuthService auth, FirebaseUserDatabaseService userService)
    {
        InitializeComponent();
        _auth = auth;
        _userService = userService;

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserProfileAsync();
        await LoadPrivateRatings();
    }

    private async Task LoadUserProfileAsync()
    {
        try
        {
            var user = await _auth.GetCurrentUser();

            if (user == null)
            {
                displayNameLabel.Text = "Not Signed In";
                emailLabel.Text = "-";
                btnSignOut.IsVisible = false;
                listRecipes.ItemsSource = new List<Recipe>();
                return;
            }

            displayNameLabel.Text = $"Name: {user.DisplayName}";
            emailLabel.Text = $"Email: {user.Email}";
            btnSignOut.IsVisible = true;

            // Fetch full Recipe objects for saved recipe IDs
           var savedRecipes = await _userService.GetSavedRecipesByIdsAsync(user.Uid);

            if (savedRecipes.Count > 0)
            {
                listRecipes.ItemsSource = savedRecipes;
            }
            else
            {
                listRecipes.ItemsSource = new List<Recipe>();
                Console.WriteLine("User has no saved recipes.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProfilePage error: {ex}");
            await DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
        }
    }
      private async Task LoadPrivateRatings()
    {
        var user = await _auth.GetCurrentUser();
        if (user == null) return;

        try
        {
            // Get all ratings for this user
            var allRatings = await _userService.GetUserRatingsAsync(user.Uid);

            // Filter non-public ratings
            PrivateRatings = allRatings
                .Where(r => r.IsPublic == false)
                .OrderByDescending(r => r.Date)
                .ToList();

            listRatings.ItemsSource = PrivateRatings;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load private ratings: " + ex.Message, "OK");
        }
    }

    private async void OnRecipe_Tapped(object sender, ItemTappedEventArgs e) 
    { 
        if (e.Item is Recipe recipe) 
        { 
            await Navigation.PushModalAsync(new RatingsPopup(recipe, _auth, _userService)); 
            listRecipes.SelectedItem = null; 
        }
}
private void OnShowRatingsClicked(object sender, EventArgs e)
{
    listRatings.IsVisible = true;
    lblRatingsHeader.IsVisible = true;

    listRecipes.IsVisible = false;
    lblRecipesHeader.IsVisible = false;
}

private void OnShowRecipesClicked(object sender, EventArgs e)
{
    listRatings.IsVisible = false;
    lblRatingsHeader.IsVisible = false;

    listRecipes.IsVisible = true;
    lblRecipesHeader.IsVisible = true;
}

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
            try
            {
                await _auth.SignOut();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignOut failed: {ex}");
            }

            // Navigate to the Login page without killing the app. Push the LoginPage onto the
            // navigation stack so the user can sign in again. This avoids abrupt app exit.
            try
            {
                await Navigation.PushAsync(new LoginPage(_auth));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to LoginPage failed: {ex}");
                try { await Shell.Current.GoToAsync("//AllRecipesPage"); } catch { }
            }
    }
}
