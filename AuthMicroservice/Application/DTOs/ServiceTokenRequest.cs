namespace AuthMicroservice.Application.DTOs
{
    public class ServiceTokenRequest
    {
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
    }
}
