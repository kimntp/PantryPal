namespace PPApp.model //name spaces are like a folder for your code
{
    public class Recipe
    {
        public string Name { get; set; }
        public string Url { get; set; } // Optional, if you have recipe links
        public List<string> Ingredients { get; set; } // Optional
    }
}
