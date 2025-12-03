namespace PPApp.Model
{
    public class AppUser
    {
        public string Uid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public Dictionary<string, bool>? SavedRecipes { get; set; } = new();

        // App-specific fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<string>? Following { get; set; } = new();
    }
}
