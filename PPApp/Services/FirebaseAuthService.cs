using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using PPApp.Model;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Diagnostics;

namespace PPApp.Services
{
    public interface IFirebaseAuthService
    {
        Task<AppUser?> SignIn(string email, string password);
        Task<AppUser?> SignUp(string displayName, string email, string password);
        Task SignOut();
        bool IsSignedIn { get; }
        Task<AppUser?> GetCurrentUser();
    }
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthClient _client;
        private readonly FirebaseClient _db;

    

        public FirebaseAuthService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyDkGQiGvYKvInQUR5jxysX2cyOAji6TNeI",
                AuthDomain = "pantry-pal-23f98.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                }
            };

            _client = new FirebaseAuthClient(config);

            _db = new FirebaseClient(
                "https://pantry-pal-23f98-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    // always return a fresh token
                    AuthTokenAsyncFactory = async () =>
                    {
                        if (_client.User != null)
                            return await _client.User.GetIdTokenAsync();
                        return await SecureStorage.GetAsync("auth_token");
                    }
                });
        }

        public async Task<AppUser?> SignIn(string email, string password)
        {
            try
            {
                var result = await _client.SignInWithEmailAndPasswordAsync(email, password);

                var token = await result.User.GetIdTokenAsync();
                await SecureStorage.SetAsync("auth_token", token);

                var profile = await GetUserFromDatabase(result.User.Uid);

                if (profile == null)
                {
                    profile = new AppUser
                    {
                        Uid = result.User.Uid,
                        Email = result.User.Info.Email,
                        DisplayName = result.User.Info.DisplayName ?? "",
                        SavedRecipes = new Dictionary<string, bool>(),
                        Following = new List<string>()
                    };

                    await SaveUserToDatabase(profile);
                }

                await SecureStorage.SetAsync("user_json", JsonSerializer.Serialize(profile));

                return profile;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignIn failed: {ex}");
                return null;
            }
        }

        public async Task<AppUser?> SignUp(string displayName, string email, string password)
{
    try
    {
        var result = await _client.CreateUserWithEmailAndPasswordAsync(email, password);

        var token = await result.User.GetIdTokenAsync();
        await SecureStorage.SetAsync("auth_token", token);

        var appUser = new AppUser
        {
            Uid = result.User.Uid,
            Email = email,
            DisplayName = displayName,
            SavedRecipes = new Dictionary<string, bool>(),
            Following = new List<string>()
        };

        await SaveUserToDatabase(appUser);

        await SecureStorage.SetAsync("user_json", JsonSerializer.Serialize(appUser));

        return appUser;
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"SignUp failed: {ex}");
        return null;
    }
}


        public Task SignOut()
        {
            _client?.SignOut();
            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("user_json");
            return Task.CompletedTask;
        }

        public bool IsSignedIn => _client.User != null;

        public async Task<AppUser?> GetCurrentUser()
        {
            if (IsSignedIn)
                return await GetUserFromDatabase(_client.User.Uid);

            try
            {
                var json = await SecureStorage.GetAsync("user_json");
                if (!string.IsNullOrEmpty(json))
                    return JsonSerializer.Deserialize<AppUser>(json);
            }
            catch { }

            return null;
        }

        private async Task SaveUserToDatabase(AppUser user)
        {
            try
            {
                await _db.Child("users")
                         .Child(user.Uid)
                         .PutAsync(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveUserToDatabase failed: {ex}");
                throw;
            }
        }

        private async Task<AppUser?> GetUserFromDatabase(string uid)
        {
            try
            {
                return await _db
                    .Child("users")
                    .Child(uid)
                    .OnceSingleAsync<AppUser>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetUserFromDatabase failed: {ex}");
                return null;
            }
        }
    }
}
