using System.Text.Json.Serialization;

namespace PPApp.Model
{
    // value for each ingredient key in the index
    public class IngredientIndexEntry
    {
        [JsonPropertyName("is_meat")]
        public required bool IsMeat { get; set; }

        [JsonPropertyName("has_gluten")]
        public required bool HasGluten { get; set; }
        [JsonPropertyName("recipes")]
        public List<Recipe> Recipes { get; set; } = new();
    }
}