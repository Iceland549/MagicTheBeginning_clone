namespace DeckMicroservice.Infrastructure.Config
{
    /// <summary>
    /// Configuration settings for MongoDB connection.
    /// </summary>
    public class MongoDbConfig
    {
        public string ConnectionString { get; set; } = string.Empty; // MongoDB connection string
        public string Database { get; set; } = string.Empty;         // Database name
        public string CardCollection { get; set; } = string.Empty;   // Collection name for cards (adapt if needed)
    }
}