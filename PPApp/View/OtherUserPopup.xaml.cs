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
    private bool _isFollowing = false;

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

        // Get current user
        _currentUser = await _auth.GetCurrentUser();

        // Load other user's profile
        var otherUser = await _userDb.GetUserProfileAsync(_otherUserId);
        if (otherUser == null) return;

        displayNameLabel.Text = otherUser.DisplayName ?? "Anonymous";

        // Load public ratings
        PublicRatings = await _userDb.GetUserPublicRatingsAsync(_otherUserId);
        ratingsCollectionView.ItemsSource = PublicRatings;

        // Check if current user is following
        _isFollowing = _currentUser?.Following?.Contains(_otherUserId) ?? false;
        UpdateFollowButton();
    }

    private void UpdateFollowButton()
    {
        followButton.Text = _isFollowing ? "Unfollow" : "Follow";
    }

private async void OnFollowButtonClicked(object sender, EventArgs e)
{
    if (_currentUser == null || string.IsNullOrEmpty(_otherUserId))
        return;

    if (_currentUser.Following == null)
        _currentUser.Following = new List<string>();

    if (_isFollowing)
    {
        // Unfollow: remove the other user ID from the list
        _currentUser.Following.Remove(_otherUserId);
    }
    else
    {
        // Follow: add the other user ID to the list
        if (!_currentUser.Following.Contains(_otherUserId))
            _currentUser.Following.Add(_otherUserId);
    }

    // Toggle follow state
    _isFollowing = !_isFollowing;
    UpdateFollowButton(); // update UI

    try
    {
        // Save the updated user object to Firebase
        await _userDb.SaveUserToDatabaseAsync(_currentUser);
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", $"Failed to update following: {ex.Message}", "OK");
    }
}
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

}
