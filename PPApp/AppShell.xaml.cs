using PPApp.Service;

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

        // Check auth state and navigate to Login if needed
        Dispatcher.Dispatch(async () =>
        {
            try
            {
                var user = await _auth.GetCurrentUser();
                if (user == null)
                {
                    // push LoginPage so user can sign in
                    await Current.Navigation.PushAsync(new View.LoginPage(_auth));
                }
            }
            catch { }
        });

       // UpdateFlyoutItems();
    }

 /*    private async void UpdateFlyoutItems()
    {
        var user = await _auth.GetCurrentUser();

		if (user != null)
		{
			profileItem.IsVisible = true;
			loginItem.IsVisible = false;
			registerItem.IsVisible = false;
		}
		else
		{
			profileItem.IsVisible = false;
			loginItem.IsVisible = true;
			registerItem.IsVisible = true;
		}
        
    } */
}
