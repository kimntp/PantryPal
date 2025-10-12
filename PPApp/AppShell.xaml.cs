using PPApp.View;

namespace PPApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		//must include pages here in in order to navigate to them
		Routing.RegisterRoute(nameof(AllRecipesPage), typeof(AllRecipesPage));
		Routing.RegisterRoute(nameof(SearchPage), typeof(SearchPage));
	}
}
