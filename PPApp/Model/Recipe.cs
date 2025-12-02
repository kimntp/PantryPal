namespace PPApp.Model //name spaces are like a folder for your code
{
    public class Recipe
    {    
        public required string Name { get; set; }
        public required string Url { get; set; }
        public required List<string> Ingredients { get; set; }
        public required string RecipeID { get; set; }
        public required string ImageUrl { get; set; }
    }
}
