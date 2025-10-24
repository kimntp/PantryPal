

namespace PPApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		//must include pages here in in order to navigate to them
		Routing.RegisterRoute(nameof(View.AllRecipesPage), typeof(View.AllRecipesPage));
		Routing.RegisterRoute(nameof(View.SearchPage), typeof(View.SearchPage));
	}
}
