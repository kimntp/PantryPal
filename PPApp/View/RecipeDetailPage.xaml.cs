using PPApp.Services;
using PPApp.Model;

namespace PPApp.View
{
    public partial class RecipeDetailPage : ContentPage
    {
        public RecipeDetailPage(Recipe recipe)
        {
            InitializeComponent();
            BindingContext = recipe;

        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void OnUrlTapped(object sender, EventArgs e)
        {
            if (BindingContext is Recipe recipe &&
                !string.IsNullOrWhiteSpace(recipe.Url))
            {
                try
                {
                    await Launcher.OpenAsync(recipe.Url);
                }
                catch
                {
                    await DisplayAlert("Error", "Could not open recipe link.", "OK");
                }
            }
        }
        
    }
}
