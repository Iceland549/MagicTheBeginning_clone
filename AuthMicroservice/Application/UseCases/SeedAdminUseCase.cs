using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.UseCases
{
    public class SeedAdminUseCase
    {
        private readonly IUserRepository _repo;

        public SeedAdminUseCase(IUserRepository repo) => _repo = repo;

        public async Task ExecuteAsync(string email, string password)
        {
            var admin = await _repo.GetByEmailAsync(email);
            if (admin == null)
            {
                admin = new User { Email = email, EmailConfirmed = true };
                await _repo.CreateUserAsync(admin, password);
            }

            var roles = await _repo.GetRolesAsync(admin.Id);
            if (!roles.Contains("Admin"))
                await _repo.AddRoleAsync(admin.Id, "Admin");
        }
    }
}