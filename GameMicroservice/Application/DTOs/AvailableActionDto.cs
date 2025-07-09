namespace GameMicroservice.Application.DTOs
{
    public class AvailableActionDto
    {
        public string Label { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool Disabled { get; set; }
    }
}
