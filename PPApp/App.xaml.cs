using PPApp.View;

namespace PPApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        Windows[0].Page = new SearchPage();
    }
}
