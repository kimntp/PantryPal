using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using PPApp.Model;

namespace PPApp.Services;


public class FirebaseUserDatabaseService
{
    private readonly FirebaseClient _client;

    public FirebaseUserDatabaseService()
    {
        // Use SecureStorage-stored auth token when available so database writes respect rules
        _client = new FirebaseClient(
            "https://pantry-pal-23f98-default-rtdb.firebaseio.com/",
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = async () => await Microsoft.Maui.Storage.SecureStorage.GetAsync("auth_token")
            }
        );
    }


    // Get user profile
    public async Task<AppUser?> GetUserProfileAsync(string uid)
    {
        return await _client
            .Child("users")
            .Child(uid)
            .OnceSingleAsync<AppUser>();
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
public async Task SaveUserRatingToDatabaseAsync(string uid, Recipe recipe, RecipeRating rating)
{
    if (string.IsNullOrWhiteSpace(uid))
        throw new ArgumentException("uid cannot be null or empty", nameof(uid));
    if (recipe == null)
        throw new ArgumentNullException(nameof(recipe));
    if (rating == null)
        throw new ArgumentNullException(nameof(rating));

    // Sanitize recipe name to be a valid Firebase key
    string recipeKey = recipe.Name
        .Replace(".", "_")
        .Replace("#", "_")
        .Replace("$", "_")
        .Replace("[", "_")
        .Replace("]", "_")
        .Replace("/", "_")
        .Replace("&", "_")
        .Replace("?", "_");

    await _client
        .Child("userReviews")
        .Child(uid)
        .Child(recipeKey) // single valid key
        .PutAsync(rating);
}
public async Task<List<RecipeRating>> GetAllPublicRatingsAsync()
{
    try
    {
        var allUsers = await _client
            .Child("userReviews")
            .OnceAsync<Dictionary<string, RecipeRating>>();

        var publicRatings = new List<RecipeRating>();

        foreach (var userRatings in allUsers)
        {
            foreach (var ratingEntry in userRatings.Object.Values)
            {
                if (ratingEntry.IsPublic)
                    publicRatings.Add(ratingEntry);
            }
        }

        return publicRatings.OrderByDescending(r => r.Date).ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error fetching ratings: " + ex.Message);
        return new List<RecipeRating>();
    }
}
public async Task<List<RecipeRating>> GetUserRatingsAsync(string uid)
{
    var ratings = await _client
        .Child("userReviews")
        .Child(uid)
        .OnceAsync<RecipeRating>();

    return ratings.Select(r => r.Object).ToList();
}
public async Task<List<RecipeRating>> GetUserPublicRatingsAsync(string uid)
{
    if (string.IsNullOrWhiteSpace(uid))
        return new List<RecipeRating>();

    // Fetch all ratings for the user
    var ratings = await _client
        .Child("userReviews")
        .Child(uid)
        .OnceAsync<RecipeRating>();

    // Filter only public ratings
    return ratings
        .Select(r => r.Object)
        .Where(r => r.IsPublic)   // <-- only keep public ones
        .OrderByDescending(r => r.Date) // optional: sort by date
        .ToList();
}
public async Task SaveUserToDatabaseAsync(AppUser user)
{
    if (user == null || string.IsNullOrEmpty(user.Uid))
        throw new ArgumentException("User or User ID is null");

    await _client
        .Child("users")
        .Child(user.Uid)
        .PutAsync(user);
}




}