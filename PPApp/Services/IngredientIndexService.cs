using System.Text.Json;
using PPApp.Model;

namespace PPApp.Services
{
    public class IngredientIndexService
    {
        private Dictionary<string, IngredientIndexEntry> _index =
            new(StringComparer.OrdinalIgnoreCase);

        private List<Recipe> _allRecipes = new();

        public async Task InitializeAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("ingredientIndex.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var dict = JsonSerializer.Deserialize<Dictionary<string, IngredientIndexEntry>>(json);

            _index = dict ?? new Dictionary<string, IngredientIndexEntry>(StringComparer.OrdinalIgnoreCase);

            BuildAllRecipes();
        }

        private void BuildAllRecipes()
        {
            _allRecipes = _index.Values
                .SelectMany(entry => entry.Recipes ?? new List<Recipe>())
                .GroupBy(r => r.RecipeID)
                .Select(g => g.First())
                .ToList();
        }

        public IReadOnlyDictionary<string, IngredientIndexEntry> GetIndex()
        {
            return _index;
        }

        public List<Recipe> GetAllRecipes()
        {
            return _allRecipes;
        }

        public List<string> GetAllIngredients()
        {
            if (_index == null || _index.Count == 0)
                return new List<string>();

            return _index.Keys
                .OrderBy(k => k)
                .ToList();
        }
 

        public List<Recipe> SearchByIngredients(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _allRecipes;

            var tokens = query
                .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .Where(t => t.Length > 0)
                .Distinct()
                .ToList();

            if (tokens.Count == 0)
                return _allRecipes;

            // Use the index to find candidate recipes quickly:
            var candidateRecipes = new Dictionary<string, (Recipe recipe, int score)>();

            foreach (var token in tokens)
            {
                // Fuzzy-ish ingredient key lookup
                var matchingKeys = _index.Keys
                    .Where(key => key.Contains(token, StringComparison.OrdinalIgnoreCase)
                               || token.Contains(key, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in matchingKeys)
                {
                    var entry = _index[key];
                    if (entry.Recipes == null) continue;

                    foreach (var recipe in entry.Recipes)
                    {
                        var keyId = recipe.Url ?? recipe.Name;

                        if (!candidateRecipes.TryGetValue(keyId, out var current))
                        {
                            candidateRecipes[keyId] = (recipe, 1);
                        }
                        else
                        {
                            candidateRecipes[keyId] = (recipe, current.score + 1);
                        }
                    }
                }
            }

            // Order by score (descending)
            return candidateRecipes
                .OrderByDescending(kvp => kvp.Value.score)
                .Select(kvp => kvp.Value.recipe)
                .ToList();
        }
    }
}
