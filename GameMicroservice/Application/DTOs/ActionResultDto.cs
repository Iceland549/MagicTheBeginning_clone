namespace GameMicroservice.Application.DTOs
{
    public class ActionResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public GameSessionDto? GameState { get; set; }
        public EndGameDto? EndGame { get; set; }
    }
}
