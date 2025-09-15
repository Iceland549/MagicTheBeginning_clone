namespace AuthMicroservice.Application.DTOs
{
    public class ServiceTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
