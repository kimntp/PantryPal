using System;
using PPApp.Model;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace PPApp.View;

public partial class AllRecipesPage : ContentPage
{
    private AppUser? _user;

    public AllRecipesPage()
    {
        InitializeComponent();
        _user = null;

        // attempt to restore cached user
        try
        {
            var json = SecureStorage.GetAsync("user_json").GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(json))
            {
                _user = JsonSerializer.Deserialize<AppUser>(json);
            }
        }
        catch { }
    }

    public AllRecipesPage(AppUser user)
    {
        InitializeComponent();
        _user = user;
    }

    private void BtnSearch_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync(nameof(SearchPage));
    }

    private async void BtnRegister_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(new Services.FirebaseAuthService()));
    }

    private async void BtnLogin_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(new Services.FirebaseAuthService()));
    }
}
