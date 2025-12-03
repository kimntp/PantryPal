using PPApp.Model;
using PPApp.Services;
namespace PPApp.View;

public partial class LoginPage : ContentPage
{
    private readonly IFirebaseAuthService _auth;

    public LoginPage(IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }


    private async void OnLoginClicked(object sender, EventArgs e)
    {
        AppUser? user = await _auth.SignIn(emailEntry.Text, passwordEntry.Text);

        if (user == null)
        {
            await DisplayAlert("Error", "Invalid login", "OK");
            return;
        }

        await Navigation.PopModalAsync();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new RegisterPage(_auth));
    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
