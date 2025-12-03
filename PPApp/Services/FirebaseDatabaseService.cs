using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using PPApp.Model;

namespace PPApp.Services;

public class FirebaseUserDatabaseService
public class FirebaseUserDatabaseService
{
    private readonly FirebaseClient _client;

    public FirebaseUserDatabaseService()
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
   public async Task<List<Recipe>> GetSavedRecipesByIdsAsync(string uid)
{
    try
    {
        // 1. Get the user profile
        var user = await _client
            .Child("users")
            .Child(uid)
            .OnceSingleAsync<AppUser>();

        if (user?.SavedRecipes == null || user.SavedRecipes.Count == 0)
            return new List<Recipe>();

        // Only recipe IDs marked as true
        var savedIds = user.SavedRecipes
            .Where(x => x.Value)
            .Select(x => x.Key)
            .ToList();

        if (savedIds.Count == 0)
            return new List<Recipe>();

        var fetchTasks = new List<Task<Recipe>>(); 

        foreach (var recipeId in savedIds)
        {
 
            fetchTasks.Add(_client.Child("recipes").Child(recipeId).OnceSingleAsync<Recipe>());
        }

        var fetchedRecipes = await Task.WhenAll(fetchTasks);


        var savedRecipes = fetchedRecipes
            .Where(recipe => recipe != null)
            .ToList(); 

        return savedRecipes;
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR fetching saved recipes: " + ex.Message);
        return new List<Recipe>();
    }
}
public async Task SaveRecipeRatingAsync(string uid, string recipeId, int rating, string? review)
{
    // The data to be saved for the specific review
    var newRatingData = new Dictionary<string, object?>
    {
        { "rating", rating },
        { "review", review }
        // You might want to add a timestamp here too, e.g.,
        // { "timestamp", Firebase.Database.ServerValue.Timestamp }
    };

    // Corrected way to use PushAsync:
    // Call PushAsync on the specific Child reference where you want the new, unique key to be created.
    await _client
        .Child("userReviews") // Top-level node for all user-submitted reviews
        .Child(uid)           // Specific user's ID
        .Child(recipeId)      // Specific recipe ID this user is reviewing
        // NOW, call PushAsync here. This will create a NEW, UNIQUE child node
        // (e.g., -M1aB2cD3eF4gH5iJ6kL) under "/userReviews/{uid}/{recipeId}"
        // and store the newRatingData within it.
        .PostAsync(newRatingData);
}





}