using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using PPApp.Model;
using Microsoft.Maui.Storage;
using System.Text.Json;

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
            // Firebase Auth config
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

            // Realtime Database client with Auth token
            _db = new FirebaseClient(
                "https://pantry-pal-23f98-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = async () => await SecureStorage.GetAsync("auth_token")
                });
        }

        // -----------------------------
        // LOGIN
        // -----------------------------
        public async Task<AppUser?> SignIn(string email, string password)
        {
            try
            {
                var result = await _client.SignInWithEmailAndPasswordAsync(email, password);
                var token = await result.User.GetIdTokenAsync();
                await SecureStorage.SetAsync("auth_token", token);

                var profile = await GetUserFromDatabase(result.User.Uid);

                // If user is not yet in database, create default
                if (profile == null)
                {
                    profile = new AppUser
                    {
                        Uid = result.User.Uid,
                        Email = result.User.Info.Email,
                        DisplayName = result.User.Info.DisplayName ?? "",
                        SavedRecipes = new Dictionary<string, bool>()
                    };
                    await SaveUserToDatabase(profile);
                }

                // cache the user locally for quick restore
                try
                {
                    await SecureStorage.SetAsync("user_json", JsonSerializer.Serialize(profile));
                }
                catch { }

                return profile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignIn failed: {ex}");
                return null;
            }
        }

        // -----------------------------
        // REGISTER
        // -----------------------------
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
                    SavedRecipes = new Dictionary<string, bool>()
                };

                await SaveUserToDatabase(appUser);

                try
                {
                    await SecureStorage.SetAsync("user_json", JsonSerializer.Serialize(appUser));
                }
                catch { }

                return appUser;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignUp failed: {ex}");
                return null;
            }
        }

        // -----------------------------
        // SIGN OUT
        // -----------------------------
        public Task SignOut()
        {
            _client.SignOut();
            SecureStorage.Remove("auth_token");
            try { SecureStorage.Remove("user_json"); } catch { }
            return Task.CompletedTask;
        }

        // -----------------------------
        // CHECK SIGNED-IN STATE
        // -----------------------------
        public bool IsSignedIn => _client.User != null;

        // -----------------------------
        // GET CURRENT USER
        // -----------------------------
        public async Task<AppUser?> GetCurrentUser()
        {
            // If client knows about the user, fetch latest profile from DB
            if (IsSignedIn)
            {
                var user = _client.User;
                if (user != null)
                    return await GetUserFromDatabase(user.Uid);
            }

            // fallback: try reading locally cached user JSON
            try
            {
                var json = await SecureStorage.GetAsync("user_json");
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<AppUser>(json);
                }
            }
            catch { }

            return null;
        }

        // -----------------------------
        // DATABASE HELPERS
        // -----------------------------
        private async Task SaveUserToDatabase(AppUser user)
        {
            await _db
                .Child("users")
                .Child(user.Uid)
                .PutAsync(user);
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
                System.Diagnostics.Debug.WriteLine($"GetUserFromDatabase failed: {ex}");
                return null;
            }
        }
    }
}
