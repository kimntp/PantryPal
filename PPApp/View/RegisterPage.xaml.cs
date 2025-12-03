using PPApp.Services;
using PPApp.Model;
namespace PPApp.View;

public partial class RegisterPage : ContentPage
{
    private readonly IFirebaseAuthService _auth;

    public RegisterPage(IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        AppUser? user = await _auth.SignUp(displayNameEntry.Text,emailEntry.Text, passwordEntry.Text);

        if (user == null)
        {
            await DisplayAlert("Error", "Account could not be created. Please check your network connection and try again.", "OK");
            return;
        }

        await Shell.Current.GoToAsync(nameof(AllRecipesPage));
    }
}
