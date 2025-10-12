namespace PPApp.View
{
    public partial class AllRecipesPage : ContentPage
    {
        public AllRecipesPage()
        {
            InitializeComponent();
        }

        private void BtnSearch_Clicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync(nameof(SearchPage));
        }
    }
}