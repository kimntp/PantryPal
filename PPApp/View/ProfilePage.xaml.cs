using PPApp.Model;
using PPApp.Service;

namespace PPApp.View;

public partial class ProfilePage : ContentPage
{
    private readonly IFirebaseAuthService _auth;

    public ProfilePage(IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var user = await _auth.GetCurrentUser();
        if (user != null)
        {
            displayNameLabel.Text = $"Name: {user.DisplayName}";
            emailLabel.Text = $"Email: {user.Email}";
            btnSignOut.IsVisible = true;
        }
        else
        {
            displayNameLabel.Text = "Not Signed In";
        
            btnSignOut.IsVisible = false;
        }
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        await _auth.SignOut();
        // return to Login page
        await Shell.Current.GoToAsync(nameof(AllRecipesPage));
    }
}
