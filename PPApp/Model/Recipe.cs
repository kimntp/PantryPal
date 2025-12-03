namespace PPApp.Model //name spaces are like a folder for your code
{
    public class Recipe
    {    
        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> Ingredients { get; set; }
        public string RecipeID { get; set; }
    }
}
