using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class RatingsFeedPage : ContentPage
{
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userDb;
    

    public List<RecipeRating> PublicRatings { get; set; } = new List<RecipeRating>();

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
        PublicRatings = await _userDb.GetAllPublicRatingsAsync();
        ratingsFeedView.ItemsSource = PublicRatings;
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", "Failed to load ratings: " + ex.Message, "OK");
    }
}
private async void OnRatingTapped(object sender, EventArgs e)
{
    if (sender is Button btn && btn.BindingContext is RecipeRating rating)
    {
        await Navigation.PushModalAsync(new OtherUserPopup(rating.UserId, _auth, _userDb));
    }
}
}
