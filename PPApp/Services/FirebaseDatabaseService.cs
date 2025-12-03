using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using PPApp.Model;
using Microsoft.Maui.Storage;

namespace PPApp.Services
{
    public class FirebaseUserDatabaseService
    {
        private readonly FirebaseClient _client;

        // Ingredient metadata cache
        private Dictionary<string, IngredientMeta> _ingredientMeta = new(StringComparer.OrdinalIgnoreCase);
        private bool _ingredientMetaLoaded = false;

        // Recipe cache
        private List<Recipe> _cachedRecipes = new();
        private bool _recipesLoaded = false;

        public FirebaseUserDatabaseService()
        {
            _client = new FirebaseClient(
                "https://pantry-pal-23f98-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = async () => await SecureStorage.GetAsync("auth_token")
                }
            );
        }

        // ------------------------------
        // Internal: ensure all recipes are loaded and cached
        // ------------------------------
        private async Task EnsureRecipesLoadedAsync()
        {
            if (_recipesLoaded)
                return;

            var recipeList = await _client
                .Child("recipes")
                .OnceSingleAsync<List<Recipe>>();

            _cachedRecipes = recipeList ?? new List<Recipe>();
            _recipesLoaded = true;
        }

        // ------------------------------
        // Recipe methods
        // ------------------------------
        public async Task<List<Recipe>> GetAllRecipes()
        {
            await EnsureRecipesLoadedAsync();
            return _cachedRecipes;
        }

        public async Task SaveRecipeAsync(string uid, Recipe recipe)
        {
            await _client
                .Child("users")
                .Child(uid)
                .Child("SavedRecipes")
                .Child(recipe.RecipeID)
                .PutAsync(true);
        }

        public async Task RemoveRecipeAsync(string uid, Recipe recipe)
        {
            await _client
                .Child("users")
                .Child(uid)
                .Child("SavedRecipes")
                .Child(recipe.RecipeID)
                .DeleteAsync();
        }

        public async Task<List<Recipe>> GetSavedRecipesByIdsAsync(string uid)
        {
            try
            {
                var user = await _client
                    .Child("users")
                    .Child(uid)
                    .OnceSingleAsync<AppUser>();

                if (user?.SavedRecipes == null || user.SavedRecipes.Count == 0)
                    return new List<Recipe>();

                var savedIds = user.SavedRecipes
                    .Where(x => x.Value)
                    .Select(x => x.Key)
                    .ToList();

                if (!savedIds.Any())
                    return new List<Recipe>();

                await EnsureRecipesLoadedAsync();

                return _cachedRecipes
                    .Where(r => !string.IsNullOrEmpty(r.RecipeID) && savedIds.Contains(r.RecipeID))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching saved recipes: " + ex.Message);
                return new List<Recipe>();
            }
        }

        public async Task<List<Recipe>> SearchByIngredientsAsync(string query)
        {
            await EnsureRecipesLoadedAsync();

            if (string.IsNullOrWhiteSpace(query))
                return _cachedRecipes;

            var tokens = query
                .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            var scored = new List<(Recipe recipe, int score)>();

            foreach (var recipe in _cachedRecipes)
            {
                if (recipe?.Ingredients == null || !recipe.Ingredients.Any())
                    continue;

                int score = tokens.Count(token =>
                    recipe.Ingredients.Any(i =>
                        !string.IsNullOrWhiteSpace(i) &&
                        (i.ToLowerInvariant().Contains(token) || token.Contains(i.ToLowerInvariant()))
                    )
                );

                if (score > 0)
                    scored.Add((recipe, score));
            }

            return scored.Any()
                ? scored.OrderByDescending(s => s.score).Select(s => s.recipe).ToList()
                : _cachedRecipes;
        }

        // ------------------------------
        // Ingredient metadata
        // ------------------------------
        public async Task<Dictionary<string, IngredientMeta>> GetIngredientMetadataAsync()
        {
            if (_ingredientMetaLoaded)
                return _ingredientMeta;

            var nodes = await _client.Child("ingredients").OnceAsync<IngredientMeta>();

            _ingredientMeta = nodes
                .Where(n => n.Object != null && !string.IsNullOrWhiteSpace(n.Object.Ingredient))
                .ToDictionary(n => n.Object.Ingredient, n => n.Object, StringComparer.OrdinalIgnoreCase);

            _ingredientMetaLoaded = true;
            return _ingredientMeta;
        }

        public async Task<List<string>> GetAllIngredients()
        {
            try
            {
                var list = await _client.Child("ingredients").OnceSingleAsync<List<string>>();
                if (list == null) return new List<string>();

                return list
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => System.Globalization.CultureInfo.InvariantCulture.TextInfo
                        .ToTitleCase(s.Trim().ToLowerInvariant()))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading ingredients: " + ex.Message);
                return new List<string>();
            }
        }

        // ------------------------------
        // User profile
        // ------------------------------
        public async Task SaveUserToDatabaseAsync(AppUser user)
        {
            if (user == null || string.IsNullOrEmpty(user.Uid))
                throw new ArgumentException("User or UID is null");

            await _client.Child("users").Child(user.Uid).PutAsync(user);
        }

        public async Task<AppUser?> GetUserProfileAsync(string uid)
        {
            return await _client.Child("users").Child(uid).OnceSingleAsync<AppUser>();
        }

        public async Task UpdateDisplayNameAsync(string uid, string newName)
        {
            await _client.Child("users").Child(uid).Child("DisplayName").PutAsync(newName);
        }

        // ------------------------------
        // Ratings
        // ------------------------------
        public async Task SaveUserRatingToDatabaseAsync(string uid, Recipe recipe, RecipeRating rating)
        {
            if (string.IsNullOrWhiteSpace(uid) || recipe == null || rating == null)
                throw new ArgumentException("UID, recipe, or rating is null");

            string recipeKey = recipe.Name
                .Replace(".", "_").Replace("#", "_").Replace("$", "_")
                .Replace("[", "_").Replace("]", "_")
                .Replace("/", "_").Replace("&", "_").Replace("?", "_");

            await _client.Child("userReviews").Child(uid).Child(recipeKey).PutAsync(rating);
        }

        public async Task<List<RecipeRating>> GetAllPublicRatingsAsync()
        {
            try
            {
                var allUsers = await _client.Child("userReviews").OnceAsync<Dictionary<string, RecipeRating>>();
                var publicRatings = new List<RecipeRating>();

                foreach (var userRatings in allUsers)
                    foreach (var ratingEntry in userRatings.Object.Values)
                        if (ratingEntry.IsPublic)
                            publicRatings.Add(ratingEntry);

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
            var ratings = await _client.Child("userReviews").Child(uid).OnceAsync<RecipeRating>();
            return ratings.Select(r => r.Object).ToList();
        }

        public async Task<List<RecipeRating>> GetUserPublicRatingsAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid)) return new List<RecipeRating>();

            var ratings = await _client.Child("userReviews").Child(uid).OnceAsync<RecipeRating>();
            return ratings.Select(r => r.Object).Where(r => r.IsPublic).OrderByDescending(r => r.Date).ToList();
        }
    }
}
