using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.Extensions.Logging;

using PPApp.Service;  
using PPApp.View;      

namespace PPApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}
