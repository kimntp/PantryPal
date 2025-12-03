namespace PPApp.Model;

public class RecipeRating
{
     public string UserId { get; set; } = "";
     public Recipe Recipe { get; set; }
    public string RecipeName { get; set; }
    public int Rating { get; set; }
    public string Review { get; set; }
    public DateTime Date { get; set; }
    public bool IsPublic { get; set; }
    public string RatingStars { get; set; } = string.Empty;

    public string StarsDisplay
    {
        get
        {
            var filled = Math.Max(0, Math.Min(5, Rating));
            var empty = 5 - filled;
            return new string('★', filled) + new string('☆', empty);
        }
    }

    public string UserName { get; set; } = "Anonymous";

    public RecipeRating()
    {
        UserId = string.Empty;
        Recipe = new Recipe();
        RecipeName = string.Empty;
        Review = string.Empty;
        Date = DateTime.Now;
        IsPublic = false;
    }
}
