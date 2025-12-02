namespace PPApp.Model
{
    public class Ingredient
    {
        public required bool IsMeat { get; set; }
        public required bool HasGluten { get; set; }
        public required List<Recipe> Recipes { get; set; }
    }
}