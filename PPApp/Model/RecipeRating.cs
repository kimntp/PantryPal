namespace PPApp.Model;

public class RecipeRating
{
    public string RecipeId { get; set; }
    public string RecipeName { get; set; }
    public int Rating { get; set; }
    public string Review { get; set; }
    public DateTime Date { get; set; }
    public bool IsPublic { get; set; }

        public string RatingStars { get; set; }

    public string UserName { get; set; } = "Anonymous"; 
}
