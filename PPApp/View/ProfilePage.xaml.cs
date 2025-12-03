using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class ProfilePage : ContentPage
{
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userService;

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

    private async void OnRecipe_Tapped(object sender, ItemTappedEventArgs e) 
    { 
        if (e.Item is Recipe recipe) 
        { 
            await Navigation.PushModalAsync(new RatingsPopup(recipe, _auth)); 
            listRecipes.SelectedItem = null; 
        }
}

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        await _auth.SignOut();
        await Shell.Current.GoToAsync(nameof(AllRecipesPage));
    }
}
