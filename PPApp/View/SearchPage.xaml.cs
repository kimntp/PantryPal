using System.Text.Json;
using PPApp.Model;

namespace PPApp.View
{
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
            InitializeComponent();
            LoadRecipesAsync();
        }

        private async void LoadRecipesAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("dataset.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var recipes = JsonSerializer.Deserialize<List<Recipe>>(json);
                listRecipes.ItemsSource = recipes;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load recipes: {ex.Message}", "OK");
            }
        }
       private async void OnRecipe_Tapped(object sender, EventArgs e)
        {
            if (sender is TextCell cell && cell.BindingContext is Recipe recipe)
            {
                // Join ingredients into a single string
              string ingredientsList = string.Join(", ", recipe.ingredients);
                // Show an alert with the list of ingredients
                await DisplayAlert(recipe.name, $"Ingredients:\n{ingredientsList}", "OK");
            }
        }
    }
}   