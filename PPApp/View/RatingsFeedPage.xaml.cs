using PPApp.Model;
using PPApp.Services;
using System.Collections.ObjectModel;

namespace PPApp.View;

public partial class RatingsFeedPage : ContentPage
{
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userDb;

    public ObservableCollection<RecipeRating> PublicRatings { get; set; } = new();

    public RatingsFeedPage(IFirebaseAuthService auth, FirebaseUserDatabaseService userDb)
    {
        InitializeComponent();
        _auth = auth;
        _userDb = userDb;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRatingsFromDb();
    }

    private async Task LoadRatingsFromDb()
    {
        try
        {
            var ratings = await _userDb.GetAllPublicRatingsAsync();

            PublicRatings.Clear();
            foreach (var r in ratings)
                PublicRatings.Add(r);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to load ratings: " + ex.Message, "OK");
        }
    }

 
    private async void OnRecipe_Tapped(object sender, EventArgs e)
    {
        try
        {
            if (sender is VisualElement ve && ve.BindingContext is RecipeRating rating)
            {
                await Navigation.PushModalAsync(new OtherUserPopup(rating.UserId, _auth, _userDb));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnRecipe_Tapped error: {ex}");
        }
    }

}
