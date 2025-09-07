using System.Threading.Tasks;
using Xunit;
using Moq;
using AuthMicroservice.Application.UseCases;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Infrastructure.Persistence.Entities;

namespace Test.AuthMicroservices.Tests
{
    public class SeedAdminUseCaseTests
    {
        [Fact]
        public async Task ExecuteAsync_CreatesAdmin_WhenNotExists()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();

            // Si on cherche l'admin par email on renvoie null => l'admin n'existe pas
            mockRepo
                .Setup(r => r.GetByEmailAsync("admin@mtb.com"))
                .ReturnsAsync((User?)null); // explicite nullable pour éviter warnings

            // Setup CreateUserAsync pour qu'elle retourne une tâche complétée
            mockRepo
                .Setup(r => r.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Setup GetRolesAsync pour renvoyer une liste vide (pas de rôles pour l'instant)
            mockRepo
                .Setup(r => r.GetRolesAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Collections.Generic.List<string>());

            var useCase = new SeedAdminUseCase(mockRepo.Object);

            // Act
            await useCase.ExecuteAsync("admin@mtb.com", "P@ssw0rd!"); // exécution avec email+password

            // Assert
            // Vérifie que CreateUserAsync a été appelé une fois avec un User ayant l'email attendu
            mockRepo.Verify(r =>
                r.CreateUserAsync(It.Is<User>(u => u.Email == "admin@mtb.com"), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotCreateAdmin_WhenAlreadyExists()
        {
            // Arrange
            var existing = new User { Id = "existing-id", Email = "admin@mtb.com", EmailConfirmed = true };
            var mockRepo = new Mock<IUserRepository>();

            // Simuler que l'admin existe déjà
            mockRepo
                .Setup(r => r.GetByEmailAsync("admin@mtb.com"))
                .ReturnsAsync(existing);

            // Si on demande les rôles, renvoyer une liste contenant "Admin" pour être complet
            mockRepo
                .Setup(r => r.GetRolesAsync(existing.Id))
                .ReturnsAsync(new System.Collections.Generic.List<string> { "Admin" });

            var useCase = new SeedAdminUseCase(mockRepo.Object);

            // Act
            await useCase.ExecuteAsync("admin@mtb.com", "any"); // les paramètres sont ignorés si admin déjà présent

            // Assert
            // Vérifie que CreateUserAsync N'EST PAS appelé si l'admin existe
            mockRepo.Verify(r => r.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }
    }
}
