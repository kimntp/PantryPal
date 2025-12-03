using PPApp.Model;
public static class AppData
{
    public static List<RecipeRating> Ratings { get; } = new();

    public static void AddRating(RecipeRating rating)
    {
        Ratings.Add(rating);
    }

    public static List<RecipeRating> GetPublicRatings()
        => Ratings.Where(r => r.IsPublic).ToList();

    public static List<RecipeRating> GetPrivateRatings()
        => Ratings.Where(r => !r.IsPublic).ToList();
}
