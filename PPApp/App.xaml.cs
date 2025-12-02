using PPApp.View;

namespace PPApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new SearchPage();
    }
}
