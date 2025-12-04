using PPApp.Services;

namespace PPApp;

public partial class AppShell : Shell
{
    private readonly IFirebaseAuthService _auth;

    public AppShell(IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;

        Routing.RegisterRoute(nameof(View.AllRecipesPage), typeof(View.AllRecipesPage));
        Routing.RegisterRoute(nameof(View.SearchPage), typeof(View.SearchPage));
        Routing.RegisterRoute(nameof(View.RegisterPage), typeof(View.RegisterPage));
        Routing.RegisterRoute(nameof(View.LoginPage), typeof(View.LoginPage));
        Routing.RegisterRoute(nameof(View.ProfilePage), typeof(View.ProfilePage));
        Routing.RegisterRoute(nameof(View.RatingsFeedPage), typeof(View.RatingsFeedPage));


    }


}