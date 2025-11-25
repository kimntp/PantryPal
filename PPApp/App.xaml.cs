using PPApp.Service;

namespace PPApp;

public partial class App : Application
{
    private readonly IFirebaseAuthService _auth;

    public App(IFirebaseAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Pass the auth service to AppShell
        return new Window(new AppShell(_auth));
    }
}
