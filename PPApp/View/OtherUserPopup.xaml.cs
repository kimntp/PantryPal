using PPApp.Model;
using PPApp.Services;

namespace PPApp.View;

public partial class OtherUserPopup : ContentPage
{
    private readonly string _otherUserId;
    private readonly IFirebaseAuthService _auth;
    private readonly FirebaseUserDatabaseService _userDb;

    public List<RecipeRating> PublicRatings { get; set; } = new List<RecipeRating>();
    private AppUser? _currentUser;
    public OtherUserPopup(string otherUserId, IFirebaseAuthService auth, FirebaseUserDatabaseService userDb)
    {
        InitializeComponent();
        _otherUserId = otherUserId;
        _auth = auth;
        _userDb = userDb;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

       
        _currentUser = await _auth.GetCurrentUser();

        var otherUser = await _userDb.GetUserProfileAsync(_otherUserId);
        if (otherUser == null) return;

        displayNameLabel.Text = otherUser.DisplayName ?? "Anonymous";

       
        PublicRatings = await _userDb.GetUserPublicRatingsAsync(_otherUserId);
        ratingsCollectionView.ItemsSource = PublicRatings;

    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

}
