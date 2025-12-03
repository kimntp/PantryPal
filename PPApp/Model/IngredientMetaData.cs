using System.Text.Json.Serialization;

namespace PPApp.Model
{
    public class IngredientMeta
    {
        [JsonPropertyName("ingredient")]
        public required string Ingredient { get; set; }

        [JsonPropertyName("is_meat")]
        public bool IsMeat { get; set; }

        [JsonPropertyName("has_gluten")]
        public bool HasGluten { get; set; }
    }
}
