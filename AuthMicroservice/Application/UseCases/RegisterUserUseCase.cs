using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Handles the registration of a new user.
    /// </summary>
    public class RegisterUserUseCase
    {
        private readonly IUserRepository _repo;

        public RegisterUserUseCase(IUserRepository repo) => _repo = repo;

        /// <summary>
        /// Registers a new user with the "Player" role after checking email uniqueness.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <param name="password">User's plain password.</param>
        public async Task ExecuteAsync(string email, string password)
        {
            var existing = await _repo.GetByEmailAsync(email);
            if (existing != null)
                throw new InvalidOperationException("Email already in use.");

            var user = new User
            {
                Email = email,
                EmailConfirmed = true
            };

            await _repo.CreateUserAsync(user, password);     // Hash & save user
            await _repo.AddRoleAsync(user.Id, "Player");     // Assign default role
        }
    }
}