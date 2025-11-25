using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using PPApp.Model;

namespace PPApp.Service;

public class FirebaseUserDatabaseService
{
    private readonly FirebaseClient _client;

    public FirebaseUserDatabaseService()
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
            .Child(recipe.recipeID)
            .PutAsync(true);
    }

    // Remove saved recipe
    public async Task RemoveRecipeAsync(string uid, Recipe recipe)
    {
        await _client
            .Child("users")
            .Child(uid)
            .Child("SavedRecipes")
            .Child(recipe.recipeID)
            .DeleteAsync();
    }
           public async Task<List<Recipe>> GetAllRecipes()
{
    try
    {
        var recipeList = await _client
            .Child("recipes")
            .OnceSingleAsync<List<Recipe>>();

        return recipeList ?? new List<Recipe>();
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR GETTING RECIPES: " + ex.Message);
        return new List<Recipe>();
    }
}



}


