using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using PPApp.Model;

namespace PPApp.Services
{
    public class FirebaseUserDatabaseService
    {
        private readonly FirebaseClient _client;

        private Dictionary<string, IngredientMeta> _ingredientMeta =
            new(StringComparer.OrdinalIgnoreCase);
        private bool _ingredientMetaLoaded = false;

        // Cached recipes for search
        private List<Recipe> _cachedRecipes = new();
        private bool _recipesLoaded = false;

        public FirebaseUserDatabaseService()
        {
            _client = new FirebaseClient("https://pantry-pal-23f98-default-rtdb.firebaseio.com/");
        }

        // ----------------------------------------------------
        // Internal helper: ensure recipes are loaded from Firebase
        // ----------------------------------------------------
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

        // ====================================================
        // USER PROFILE + SAVED RECIPES + RATINGS (Firebase)
        // ====================================================

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

        // Save Recipe (flag in user's SavedRecipes)
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

        public async Task<Dictionary<string, IngredientMeta>> GetIngredientMetadataAsync()
        {
            if (_ingredientMetaLoaded)
                return _ingredientMeta;

            var nodes = await _client
                .Child("ingredients")
                .OnceAsync<IngredientMeta>();

            _ingredientMeta = nodes
                .Where(n => n.Object != null && !string.IsNullOrWhiteSpace(n.Object.Ingredient))
                .ToDictionary(
                    n => n.Object.Ingredient,
                    n => n.Object,
                    StringComparer.OrdinalIgnoreCase);

            _ingredientMetaLoaded = true;
            return _ingredientMeta;
        }

        // Get ALL recipes from Firebase (raw list)
        public async Task<List<Recipe>> GetAllRecipes()
        {
            await EnsureRecipesLoadedAsync();
            return _cachedRecipes;
        }

        // Get saved recipes for a user based on IDs in their profile
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

                // Ensure we have the full recipes list cached
                await EnsureRecipesLoadedAsync();

                // Filter cached recipes by those IDs
                var savedRecipes = _cachedRecipes
                    .Where(r => !string.IsNullOrEmpty(r.RecipeID)
                             && savedIds.Contains(r.RecipeID))
                    .ToList();

                return savedRecipes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR fetching saved recipes: " + ex.Message);
                return new List<Recipe>();
            }
        }

        // Save a rating/review for a recipe by user
        public async Task SaveRecipeRatingAsync(string uid, string recipeId, int rating, string? review)
        {
            var newRatingData = new Dictionary<string, object?>
            {
                { "rating", rating },
                { "review", review }
                // Optionally add timestamp
            };

            await _client
                .Child("userReviews")
                .Child(uid)
                .Child(recipeId)
                .PostAsync(newRatingData);
        }

        // ====================================================
        // INGREDIENT + RECIPE SEARCH (using Firebase recipes)
        // ====================================================

        // Get list of all distinct ingredient names across all recipes
        public async Task<List<string>> GetAllIngredients()
        {
            try
            {
                var list = await _client
                    .Child("ingredients")
                    .OnceSingleAsync<List<string>>();

                if (list == null)
                    return new List<string>();

                return list
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    // normalize spacing/casing
                    .Select(s => System.Globalization.CultureInfo.InvariantCulture.TextInfo
                        .ToTitleCase(s.ToLowerInvariant()))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR loading ingredients from Firebase: " + ex);
                return new List<string>();
            }
        }
        

        public async Task<List<Recipe>> SearchByIngredientsAsync(string query)
        {
            // Ensure we’ve loaded all recipes from Firebase once
            await EnsureRecipesLoadedAsync();

            if (string.IsNullOrWhiteSpace(query))
                return _cachedRecipes;

            var tokens = query
                .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .Where(t => t.Length > 0)
                .Distinct()
                .ToList();

            if (tokens.Count == 0)
                return _cachedRecipes;

            var scored = new List<(Recipe recipe, int score)>();

            foreach (var recipe in _cachedRecipes)
            {
                if (recipe?.Ingredients == null || recipe.Ingredients.Count == 0)
                    continue;

                var normalizedIngredients = recipe.Ingredients
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .Select(i => i.ToLowerInvariant())
                    .ToList();

                if (normalizedIngredients.Count == 0)
                    continue;

                int score = 0;

                foreach (var token in tokens)
                {
                    bool tokenMatches = normalizedIngredients.Any(ing =>
                        ing.Contains(token) || token.Contains(ing));

                    if (tokenMatches)
                        score++;
                }

                if (score > 0)
                    scored.Add((recipe, score));
            }

            if (scored.Count == 0)
                return _cachedRecipes;

            return scored
                .OrderByDescending(s => s.score)
                .Select(s => s.recipe)
                .ToList();
        }

    }
}
