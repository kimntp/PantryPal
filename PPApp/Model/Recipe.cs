namespace PPApp.Model //name spaces are like a folder for your code
{
    public class Recipe
    {
        public string name { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty; // Optional, if you have recipe links
        public List<string> ingredients { get; set; } = new List<string>(); // Optional
    }
}
