using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class AllRecipesPage : ContentPage
{
    private readonly FirebaseUserDatabaseService _indexService;
    private readonly IFirebaseAuthService _auth;

    public List<Recipe> Recipes { get; set; } = new(); // Initialize to avoid null binding

    public AllRecipesPage(IFirebaseAuthService auth, FirebaseUserDatabaseService indexService)
    {
        InitializeComponent();
        _auth = auth;
        _indexService = indexService;
        BindingContext = this; // Set BindingContext for data binding
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

            if (recipeList == null || !recipeList.Any())
            {
                await DisplayAlert("Info", "No recipes found.", "OK");
                return;
            }

            // Assign temporary IDs if missing
            for (int i = 0; i < recipeList.Count; i++)
            {
                if (string.IsNullOrEmpty(recipeList[i].RecipeID))
                    recipeList[i].RecipeID = i.ToString();
            }

            Recipes = recipeList.ToList();
            listRecipes.ItemsSource = Recipes; // Bind to CollectionView
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recipes: {ex.Message}", "OK");
        }
    }

    private async void OnRecipe_Selected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection is Recipe recipe)
        {
            await Navigation.PushModalAsync(new SaveRecipePopup(recipe, _auth));
            ((CollectionView)sender).SelectedItem = null; // Deselect
        }
    }
}
