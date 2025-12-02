using System.Text.Json;
using PPApp.Model;

namespace PPApp.View;

public partial class SearchPage : ContentPage
{
    private List<Recipe> _allRecipes = new();

    public SearchPage()
    {
        InitializeComponent();
        LoadRecipesAsync();
    }

    // load recipes from bundled JSON file
    private async void LoadRecipesAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("recipes.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            _allRecipes = JsonSerializer.Deserialize<List<Recipe>>(json)
                          ?? new List<Recipe>();
                
            RecipesCollectionView.ItemsSource = ProjectForView(_allRecipes);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recipes: {ex.Message}", "OK");
        }
    }

    // Event handler for search bar text changes
    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(query))
        {
            // show all recipes when search is empty
            RecipesCollectionView.ItemsSource = ProjectForView(_allRecipes);
            return;
        }

        var tokens = query
            .Split(new[] { ',', ','}, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            RecipesCollectionView.ItemsSource = ProjectForView(_allRecipes);
            return;
        }

        var ranked = _allRecipes
            .Select(r => new
            {
                Recipe = r,
                Score = ComputeIngredientScore(r, tokens)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Recipe)
            .ToList();

        RecipesCollectionView.ItemsSource = ProjectForView(ranked);
    }
    

    // fuzzy-ish score: how many tokens match any ingredient string
    private int ComputeIngredientScore(Recipe recipe, List<string> tokens)
    {
        if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            return 0;

        var normalizedIngredients = recipe.Ingredients
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.ToLowerInvariant())
            .ToList();

        int score = 0;

        foreach (var token in tokens)
        {
            bool tokenMatches = normalizedIngredients.Any(ing =>
                ing.Contains(token) || token.Contains(ing));

            if (tokenMatches)
            {
                score++;
            }
        }
        return score;
    }

// Helper: project recipes into objects with an IngredientsString for display
private List<object> ProjectForView(List<Recipe> recipes)
{
    return recipes
        .Select(r => new
        {
            r.Name,
            r.Url,
            IngredientsString = r.Ingredients == null
                ? string.Empty
                : string.Join(", ", r.Ingredients)
        })
        .Cast<object>()
        .ToList();
    }
}
