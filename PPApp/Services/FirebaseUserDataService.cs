using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using PPApp.Model;

namespace PPApp.Service;

public class FirebaseUserDataService : IUserDataService
{
    private readonly FirebaseClient _client;

    public FirebaseUserDataService()
    {
        _client = new FirebaseClient("https://pantry-pal-23f98-default-rtdb.firebaseio.com/");
    }

    // Write user profile to database
    public async Task SaveUserProfileAsync(AppUser user)
    {
        await _client
            .Child("users")
            .Child(user.Uid)
            .PutAsync(user);
    }

    // Get user profile
    public async Task<AppUser?> GetUserProfileAsync(string uid)
    {
        return await _client
            .Child("users")
            .Child(uid)
            .OnceSingleAsync<AppUser>();
    }

    // Update display name
    public async Task UpdateDisplayNameAsync(string uid, string newName)
    {
        await _client
            .Child("users")
            .Child(uid)
            .Child("DisplayName")
            .PutAsync(newName);
    }

    // Save Recipe
    public async Task SaveRecipeAsync(string uid, Recipe recipe)
    {
        await _client
            .Child("users")
            .Child(uid)
            .Child("SavedRecipes")
            .Child(recipe.RecipeID)
            .PutAsync(true);
    }

    // Remove saved recipe
    public async Task RemoveRecipeAsync(string uid, Recipe recipe)
    {
        await _client
            .Child("users")
            .Child(uid)
            .Child("SavedRecipes")
            .Child(recipe.RecipeID)
            .DeleteAsync();
    }
    
    public async Task<List<Recipe>> GetSavedRecipesAsync(string uid)
    {
        try
        {
            // get saved recipe IDs (stored as { recipeId: true })
            var savedNodes = await _client
                .Child("users")
                .Child(uid)
                .Child("SavedRecipes")
                .OnceAsync<bool>();

            var savedIds = savedNodes
                .Select(n => n.Key) // Firebase key for each child node
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (savedIds.Count == 0)
                return new List<Recipe>();

            // 2) Fetch all recipes and filter by savedIds
            var allRecipes = await _client
                .Child("recipes")
                .OnceSingleAsync<List<Recipe>>() ?? new List<Recipe>();

            var savedRecipes = allRecipes
                .Where(r => savedIds.Contains(r.RecipeID))
                .ToList();

            return savedRecipes;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR GETTING SAVED RECIPES: " + ex.Message);
            return new List<Recipe>();
        }
    }
} 