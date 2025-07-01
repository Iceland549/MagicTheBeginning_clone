namespace AuthMicroservice.Application.DTOs
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = null!;
    }
}
