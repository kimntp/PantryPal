using PPApp.Model;
using PPApp.Service;

namespace PPApp.View;

public partial class SearchPage : ContentPage
{
    private readonly FirebaseUserDatabaseService _recipeService = new FirebaseUserDatabaseService();
    private readonly IFirebaseAuthService _auth;
    private List<Recipe> _allRecipes = new List<Recipe>();

    public SearchPage(IFirebaseAuthService auth)
    {
        InitializeComponent();
         _auth = auth;
        LoadRecipesAsync();
    }

    // Load all recipes from Firebase
    private async void LoadRecipesAsync()
    {
        try
        {
    
            var recipeList = await _recipeService.GetAllRecipes();

            // Assign recipeID from index
            int index = 0;
            foreach (var item in recipeList)
            {
                item.recipeID = index.ToString();
                index++;
            }

            // Now bind to UI
            listRecipes.ItemsSource = recipeList;


        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recipes: {ex.Message}", "OK");
        }
    }

    // Handle tapping a recipe
    private async void OnRecipe_Tapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is Recipe recipe)
        {
            await Navigation.PushModalAsync(new SaveRecipePopup(recipe, _auth));

        }
        // Deselect item
        listRecipes.SelectedItem = null;
    }


    // Optional: search filter
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue))
        {
            listRecipes.ItemsSource = _allRecipes;
        }
        else
        {
            listRecipes.ItemsSource = _allRecipes
                .Where(r => r.name.Contains(e.NewTextValue, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    // Optional: go to profile
    /*
    private async void BtnUser_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }
    */
}
