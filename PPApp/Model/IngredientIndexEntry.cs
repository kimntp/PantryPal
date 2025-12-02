namespace PPApp.Model
{
    // value for each ingredient key in the index
    public class IngredientIndexEntry
    {
        public required bool IsMeat { get; set; }
        public required bool HasGluten { get; set; }
        public required List<Recipe> Recipes { get; set; }
    }
}