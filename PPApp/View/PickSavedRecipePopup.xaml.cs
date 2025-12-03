using PPApp.Model;
using PPApp.Service;

namespace PPApp.View;

public partial class RecipePickerPopup : ContentPage
{
    private readonly IFirebaseAuthService _auth;

    public RecipePickerPopup(List<Recipe> recipes, IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
        recipesList.ItemsSource = recipes;
        recipesList.SelectionChanged += OnSelected;
    }

    private async void OnSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedRecipe = e.CurrentSelection.FirstOrDefault() as Recipe;
        if (selectedRecipe == null)
            return;

        // Close this picker first
         
        
        await Navigation.PushModalAsync(new RatingsPopup(selectedRecipe, _auth));

        // Open the rating popup for the selected recipe
       
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
