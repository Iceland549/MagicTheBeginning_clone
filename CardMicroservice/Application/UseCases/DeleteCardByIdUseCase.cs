using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Application.UseCases
{
    public class DeleteCardByIdUseCase
    {
        private readonly ICardRepository _repo;

        public DeleteCardByIdUseCase(ICardRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ExecuteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return await _repo.DeleteByIdAsync(id);
        }
    }
}
