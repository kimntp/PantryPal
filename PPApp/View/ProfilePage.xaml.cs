using System.Collections.ObjectModel;
using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class ProfilePage : ContentPage
{
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userService;
     public ObservableCollection<Recipe> Recipes { get; set; } = new();
    public List<RecipeRating> PrivateRatings { get; set; } = new();

    public ProfilePage(IFirebaseAuthService auth, FirebaseUserDatabaseService userService)
    {
        InitializeComponent();
        _auth = auth;
        _userService = userService;

        // IMPORTANT: Bind recipes list ONCE
        listRecipes.ItemsSource = Recipes;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserProfileAsync();
        await LoadPrivateRatings();
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

    // ------------------------------
    // Toggle Buttons
    // ------------------------------
    private void OnShowRatingsClicked(object sender, EventArgs e)
    {
        listRatings.IsVisible = true;
        lblRatingsHeader.IsVisible = true;

        lblRecipesHeader.IsVisible = false;
        listRecipes.IsVisible = false;
    }

    private void OnShowRecipesClicked(object sender, EventArgs e)
    {
        listRatings.IsVisible = false;
        lblRatingsHeader.IsVisible = false;

        lblRecipesHeader.IsVisible = true;
        listRecipes.IsVisible = true;
    }

    // ------------------------------
    // Tapping Saved Recipes
    // ------------------------------
    private async void OnRecipeSelected(object sender, ItemTappedEventArgs e) 
    { 
        if (e.Item is Recipe recipe) 
        { 
            await Navigation.PushModalAsync(new RatingsPopup(recipe, _auth, _userService)); 
            listRecipes.SelectedItem = null; 
        }
}

    // ------------------------------
    // SIGN OUT (Stays on this page)
    // ------------------------------
    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        btnSignOut.IsEnabled = false;

        try
        {
            await _auth.SignOut();

            // Reset UI
            displayNameLabel.Text = "Not Signed In";
            emailLabel.Text = "-";
            btnSignOut.IsVisible = false;

            // CLEAR BOTH LISTS
            Recipes.Clear();
            listRatings.ItemsSource = new List<RecipeRating>();

            await DisplayAlert("Signed Out", "You have been signed out.", "OK");
        }
        finally
        {
            btnSignOut.IsEnabled = true;
        }
    }
}
