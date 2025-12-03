using PPApp.Model;
using PPApp.Services;
using System.Collections.ObjectModel;

namespace PPApp.View;

public partial class AllRecipesPage : ContentPage
{
    private readonly FirebaseUserDatabaseService _indexService;
    private readonly IFirebaseAuthService _auth;

    // Use ObservableCollection for automatic UI updates
    public ObservableCollection<Recipe> Recipes { get; set; } = new();

    public AllRecipesPage(IFirebaseAuthService auth, FirebaseUserDatabaseService indexService)
    {
        InitializeComponent();
        _auth = auth;
        _indexService = indexService;

        // Bind the CollectionView to the ObservableCollection
        listRecipes.ItemsSource = Recipes;
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

            // Clear previous items
            Recipes.Clear();

            // Assign IDs if missing and add to ObservableCollection
            for (int i = 0; i < recipeList.Count; i++)
            {
                if (string.IsNullOrEmpty(recipeList[i].RecipeID))
                    recipeList[i].RecipeID = i.ToString();

                Recipes.Add(recipeList[i]);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recipes: {ex.Message}", "OK");
        }
    }
    private async void OnRecipe_Tapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is VisualElement ve && ve.BindingContext is Recipe recipe)
            {
                await Navigation.PushModalAsync(new SaveRecipePopup(recipe, _auth));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnRecipe_Tapped error: {ex}");
        }
    }
}
