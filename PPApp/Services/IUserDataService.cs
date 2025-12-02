namespace PPApp.Service
{
    public interface IUserDataService
    {
        Task SaveRecipeAsync(string userId, Recipe recipe);
        Task<List<Recipe>> GetSavedRecipesAsync(string userId);
    }
}
