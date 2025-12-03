using System;
using PPApp.Model;
using PPApp.Services;
using Microsoft.Maui.Controls;

namespace PPApp.View
{
    public partial class SaveRecipePopup : ContentPage
    {
        private readonly Recipe _recipe;
        private readonly IFirebaseAuthService _authService;
        private readonly FirebaseUserDatabaseService _userDb;

        public SaveRecipePopup(Recipe recipe, IFirebaseAuthService auth)
        {
            InitializeComponent();
            _recipe = recipe;
            _authService = auth;
            _userDb = new FirebaseUserDatabaseService();

            LoadRecipe();
        }

        private void LoadRecipe()
        {
            recipeNameLabel.Text = _recipe.Name;
            ingredientsLabel.Text = string.Join(", ", _recipe.Ingredients ?? new List<string>());
            urlLabel.Text = _recipe.Url;
        }


        private async void OnSaveClicked(object sender, EventArgs e)
        {
            var user = await _authService.GetCurrentUser();

            if (user == null)
            {
                await DisplayAlert("Error", "You must be logged in to save recipes.", "OK");
                return;
            }

            await _userDb.SaveRecipeAsync(user.Uid, _recipe);

            await DisplayAlert("Saved", $"{_recipe.Name} has been saved!", "OK");
            await Navigation.PopModalAsync();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}
