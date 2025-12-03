using PPApp.Model;
using PPApp.Service;

namespace PPApp.View;

public partial class AllRecipesPage : ContentPage
{
    private readonly FirebaseUserDatabaseService _indexService;
    private readonly IFirebaseAuthService _auth;
    private List<Recipe> _allRecipes = new List<Recipe>();

    public AllRecipesPage(IFirebaseAuthService auth, FirebaseUserDatabaseService indexService)
    {
        InitializeComponent();
        _auth = auth;
        _indexService = indexService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRecipesAsync();
    }

    private async Task LoadRecipesAsync()
    {
        try
        {
            var recipeList = await _indexService.GetAllRecipes();

            if (recipeList == null || recipeList.Count == 0)
            {
                await DisplayAlert("Info", "No recipes found.", "OK");
                return;
            }

            int index = 0;
            foreach (var item in recipeList)
            {
                if (string.IsNullOrEmpty(item.RecipeID))
                    item.RecipeID = index.ToString();
                index++;
            }

            _allRecipes = recipeList.ToList();
            listRecipes.ItemsSource = _allRecipes;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recipes: {ex.Message}", "OK");
        }
    }
    private async void OnRecipe_Tapped(object sender, ItemTappedEventArgs e) 
    { 
        if (e.Item is Recipe recipe) 
        { 
            await Navigation.PushModalAsync(new SaveRecipePopup(recipe, _auth)); 
            listRecipes.SelectedItem = null; 
        }
}
}
