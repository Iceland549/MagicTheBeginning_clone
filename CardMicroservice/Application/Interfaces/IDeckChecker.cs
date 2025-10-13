namespace CardMicroservice.Application.Interfaces
{
    public interface IDeckChecker
    {
        Task<bool> IsCardUsedInDeckAsync(string cardId);

    }
}
