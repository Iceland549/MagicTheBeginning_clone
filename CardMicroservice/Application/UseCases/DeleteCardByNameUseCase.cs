using CardMicroservice.Application.Interfaces;

namespace CardMicroservice.Application.UseCases
{
    public class DeleteCardByNameUseCase
    {
        private readonly ICardRepository _repo;

        public DeleteCardByNameUseCase(ICardRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> ExecuteAsync(string name)
        {
            return await _repo.DeleteByNameAsync(name);
        }
    }
}
