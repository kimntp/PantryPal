using PPApp.Model;
using PPApp.Service;

namespace PPApp.View;

public partial class SearchPage : ContentPage
{
    private readonly IngredientIndexService _indexService = new();
    private List<Recipe> _allRecipes = new();

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
            RecipesCollectionView.ItemsSource = _allRecipes;
            RecipesCollectionView.SelectionChanged += OnRecipeSelected;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load ingredient index: {ex.Message}", "OK");
        }
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue;
        var results = _indexService.SearchByIngredients(query);
        RecipesCollectionView.ItemsSource = results;
    }

    private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is Recipe recipe)
        {
            await DisplayAlert("Recipe Selected", recipe.Name, "OK");

            //await Navigation.PushModalAsync(new SaveRecipePopup(recipe, _userDataService, _userId));
        }

        ((CollectionView)sender).SelectedItem = null;
    }
}
