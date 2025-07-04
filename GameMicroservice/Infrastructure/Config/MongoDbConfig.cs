namespace GameMicroservice.Infrastructure.Config
{
    public class MongoDbConfig
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CardCollection { get; set; } = null!;
        public string DeckCollection { get; set; } = null!;
        public string GameCollection { get; set; } = null!;
    }

}