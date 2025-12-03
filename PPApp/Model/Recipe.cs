using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace PPApp.Model
{
    public class Recipe
    {
        // JSON: "recipeID": 11 (number) → "11"
        [JsonPropertyName("recipeID")]
        public string RecipeID { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("ingredients")]
        public List<string> Ingredients { get; set; } = new();

        // Image mapping
        public string DisplayImage =>
            string.IsNullOrWhiteSpace(RecipeID)
                ? "recipe_placeholder.jpg"
                : $"recipe_{RecipeID}.jpg";

        // Title-cased name for UI
        private static readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;

        [JsonIgnore]
        public string DisplayName =>
            string.IsNullOrWhiteSpace(Name)
                ? Name
                : _textInfo.ToTitleCase(Name.ToLower());

        // --- Recipe-level dietary flags, derived from ingredient metadata ---

        [JsonIgnore]
        public bool ContainsMeat { get; set; }

        [JsonIgnore]
        public bool ContainsGluten { get; set; }

        // Vegetarian = no meat (gluten is allowed)
        [JsonIgnore]
        public bool IsVegetarian => !ContainsMeat;

        // Cleaned ingredients (same canonical strings used in the ComboBox)
        [JsonIgnore]
        public List<string> CleanIngredients { get; set; } = new();
    }
}
