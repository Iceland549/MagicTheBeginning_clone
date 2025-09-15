namespace AuthMicroservice.Infrastructure.Persistence.Entities
{
    public class ServiceClient
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ClientId { get; set; } = null!;
        public string ClientSecretHash { get; set; } = null!;
        public string AllowedScopes { get; set; } = "";
    }
}
