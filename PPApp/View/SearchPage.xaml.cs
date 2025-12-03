using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class SearchPage : ContentPage
{
    private readonly IngredientIndexService _indexService = new IngredientIndexService();
    private List<Recipe> _allRecipes = new List<Recipe>();

    public SearchPage()
    {
        InitializeComponent();
        InitAsync();
    }

private async void InitAsync()
    {
        try
        {
            await _indexService.InitializeAsync();
            _allRecipes = _indexService.GetAllRecipes();

            // Show all recipes initially
            RecipesCollectionView.ItemsSource = _allRecipes;
            RecipesCollectionView.SelectionChanged += OnRecipeSelected;

            // Populate ComboBox with ingredient names from the index
            var allIngredients = _indexService.GetAllIngredients();
            IngredientCombo.ItemsSource = allIngredients;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load ingredient index: {ex.Message}", "OK");
        }
    }


private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
{
    // when nothing is selected
    if (e?.CurrentSelection == null || e.CurrentSelection.Count == 0)
        return;

    if (e.CurrentSelection.FirstOrDefault() is Recipe recipe)
    {
        // visual confirmation
        await DisplayAlert("Recipe Selected", recipe.Name, "OK");

        // Later: open SaveRecipePopup or navigate to a detail page
        // await Navigation.PushModalAsync(new SaveRecipePopup(recipe, _userDataService, _userId));
    }

    ((CollectionView)sender).SelectedItem = null;
}


private async void IngredientCombo_SelectedItemChanged(object sender, EventArgs e)
{
    try
    {
        // Make sure we actually have a selection
        if (IngredientCombo.SelectedItem is not string ingredient ||
            string.IsNullOrWhiteSpace(ingredient))
        {
            return;
        }

        // Make sure recipes are loaded
        if (_allRecipes == null || _allRecipes.Count == 0)
        {
            RecipesCollectionView.ItemsSource = _allRecipes;
            return;
        }

        // Run the search using the ingredient text
        var results = _indexService.SearchByIngredients(ingredient);

        // If search returns nothing or null, fall back to full list
        if (results == null || results.Count == 0)
        {
            RecipesCollectionView.ItemsSource = _allRecipes;
        }
        else
        {
            RecipesCollectionView.ItemsSource = results;
        }
    }
    catch (Exception ex)
    {
        // Instead of crashing the whole app, show the real error
        await DisplayAlert("Ingredient filter error", ex.Message, "OK");
        RecipesCollectionView.ItemsSource = _allRecipes;
    }
}

}

