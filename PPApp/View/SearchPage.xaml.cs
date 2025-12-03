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

    private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            // If recipes aren't loaded yet, don't try to search
            if (_allRecipes == null || _allRecipes.Count == 0)
            {
                // You can decide whether to show an alert here or just ignore
                return;
            }

            var query = e.NewTextValue ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                // Reset to full list
                RecipesCollectionView.ItemsSource = _allRecipes;
                return;
            }

            var results = _indexService.SearchByIngredients(query);

            // Fallback in case service returns null
            RecipesCollectionView.ItemsSource = (results == null || results.Count == 0)
                ? _allRecipes
                : results;
        }
        catch (Exception ex)
        {
            // Instead of crashing the whole app, show what went wrong
            await DisplayAlert("Search error", ex.Message, "OK");

            // As a fallback, reset the list so the UI doesn't get stuck
            RecipesCollectionView.ItemsSource = _allRecipes;
        }
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
