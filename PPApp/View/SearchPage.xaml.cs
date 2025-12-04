
using System.Text.Json;

using PPApp.Model;
using PPApp.Services;

namespace PPApp.View
{
    public partial class SearchPage : ContentPage
    {
        private readonly FirebaseUserDatabaseService _dataService = new();
        private readonly IFirebaseAuthService _auth = new FirebaseAuthService();
        private List<Recipe> _allRecipes = new();

        private bool _useAndMatch = false;

        public SearchPage()
        {
            InitializeComponent();
            InitAsync();
        }

        private async void InitAsync()
        {
            try
            {
                // 1) Recipes from Firebase (or your backend)
                _allRecipes = await _dataService.GetAllRecipes();

                // Ensure every recipe has a RecipeID for image mapping.
                for (int i = 0; i < _allRecipes.Count; i++)
                {
                    var recipe = _allRecipes[i];
                    if (string.IsNullOrWhiteSpace(recipe.RecipeID))
                    {
                        recipe.RecipeID = i.ToString();
                    }
                }

                RecipesCollectionView.ItemsSource = _allRecipes;
                RecipesCollectionView.SelectionChanged += OnRecipeSelected;

                await LoadIngredientsFromJsonAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private async Task LoadIngredientsFromJsonAsync()
        {
            try
            {
                // ingredients.json must be in Resources/Raw with Build Action = MauiAsset
                using var stream = await FileSystem.OpenAppPackageFileAsync("ingredients.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                var ingredientMetas =
                    JsonSerializer.Deserialize<List<IngredientMeta>>(json)
                    ?? new List<IngredientMeta>();

                var grouped = ingredientMetas
                    .Where(m => !string.IsNullOrWhiteSpace(m.Ingredient))
                    .GroupBy(m => m.Ingredient, StringComparer.OrdinalIgnoreCase);

                var metaDict = grouped.ToDictionary(
                    g => g.Key,
                    g => new IngredientMeta
                    {
                        Ingredient = g.Key,
                        IsMeat = g.Any(x => x.IsMeat),
                        HasGluten = g.Any(x => x.HasGluten)
                    },
                    StringComparer.OrdinalIgnoreCase);

                var ingredientNames = metaDict.Keys
                    .OrderBy(name => name)
                    .ToList();

                IngredientCombo.ItemsSource = ingredientNames;
                IngredientCombo.SetIngredientMetadata(metaDict);

                foreach (var recipe in _allRecipes)
                {
                    var ingredients = recipe.Ingredients ?? new List<string>();

                    bool hasMeat = false;
                    bool hasGluten = false;
                    var cleanList = new List<string>();

                    foreach (var ing in ingredients)
                    {
                        if (metaDict.TryGetValue(ing, out var meta))
                        {
                            if (meta.IsMeat) hasMeat = true;
                            if (meta.HasGluten) hasGluten = true;
                            cleanList.Add(meta.Ingredient);
                        }
                        else
                        {
                            cleanList.Add(ing);
                        }
                    }

                    recipe.ContainsMeat = hasMeat;
                    recipe.ContainsGluten = hasGluten;
                    recipe.CleanIngredients = cleanList;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ingredient Error", ex.ToString(), "OK");
            }
        }

        private void IngredientCombo_SelectedItemChanged(object sender, EventArgs e)
        {
            ApplyIngredientFilter();
        }

        private void OnMatchModeToggled(object sender, ToggledEventArgs e)
        {
            _useAndMatch = e.Value; // false = OR, true = AND
            ApplyIngredientFilter();
        }

        private void ApplyIngredientFilter()
        {
            try
            {
                var selected = IngredientCombo.SelectedIngredients?.ToList()
                              ?? new List<string>();

                if (selected.Count == 0)
                {
                    RecipesCollectionView.ItemsSource = _allRecipes;
                    return;
                }

                var selectedSet = new HashSet<string>(selected, StringComparer.OrdinalIgnoreCase);

                IEnumerable<Recipe> filtered;

                if (_useAndMatch)
                {
                    filtered = _allRecipes.Where(r =>
                    {
                        var ingredients = r.Ingredients ?? new List<string>();
                        var recipeSet = new HashSet<string>(ingredients, StringComparer.OrdinalIgnoreCase);
                        return selectedSet.All(recipeSet.Contains);
                    });
                }
                else
                {
                    filtered = _allRecipes.Where(r =>
                        (r.Ingredients ?? new List<string>())
                            .Any(ing => selectedSet.Contains(ing)));
                }

                RecipesCollectionView.ItemsSource = filtered.ToList();
            }
            catch (Exception ex)
            {
                DisplayAlert("Filter Error", ex.Message, "OK");
                RecipesCollectionView.ItemsSource = _allRecipes;
            }
        }
        private async void OnSaveRecipeClicked(object sender, EventArgs e)
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

        private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection?.FirstOrDefault() is Recipe recipe)
                {
                    await Navigation.PushModalAsync(new RecipeDetailPage(recipe));
                }
            }
            finally
            {
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;
            }
        }
    }
}
