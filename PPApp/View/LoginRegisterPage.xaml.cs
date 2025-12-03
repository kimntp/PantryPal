
using PPApp.Services;
namespace PPApp.View;

public partial class LoginRegisterPage : ContentPage
{

     private readonly IFirebaseAuthService _auth;
    public LoginRegisterPage(IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
{
    await Navigation.PushModalAsync(new LoginPage(_auth));
}

private async void OnRegisterButtonClicked(object sender, EventArgs e)
{
    await Navigation.PushModalAsync(new RegisterPage(_auth));
}

}