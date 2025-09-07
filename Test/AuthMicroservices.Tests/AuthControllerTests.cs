using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;

using AuthMicroservice.Presentation.Controllers;
using AuthMicroservice.Application.Interfaces;
using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.UseCases;

namespace Test.AuthMicroservices.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();

            // Le service retourne un JwtResponse valide
            var jwtResponse = new JwtResponse
            {
                AccessToken = "token",
                RefreshToken = "refresh",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                UserId = "user-id"
            };

            // Attention : LoginAsync signature = (string email, string password)
            mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(jwtResponse);

            // Construire les UseCases réels en leur passant le mock IAuthService
            var loginUseCase = new LoginUseCase(mockAuthService.Object);
            var refreshUseCase = new RefreshTokenUseCase(mockAuthService.Object);
            var logoutUseCase = new LogoutUseCase(mockAuthService.Object);

            // Injecte les UseCases dans le controller (comme fait dans l'app)
            var controller = new AuthController(loginUseCase, refreshUseCase, logoutUseCase);

            // Act
            var result = await controller.Login(new LoginRequest { Email = "test@example.com", Password = "pwd" });

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);                    // controller renvoie Ok(jwt)
            var returned = Assert.IsType<JwtResponse>(ok.Value);               // ok.Value doit être JwtResponse
            Assert.Equal("token", returned.AccessToken);                      // vérifie le contenu
            mockAuthService.Verify(s => s.LoginAsync("test@example.com", "pwd"), Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();

            // Simuler identifiants invalides -> LoginAsync renvoie null (AuthController teste 'if (jwt == null)')
            mockAuthService
                .Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((JwtResponse?)null);

            var loginUseCase = new LoginUseCase(mockAuthService.Object);
            var refreshUseCase = new RefreshTokenUseCase(mockAuthService.Object);
            var logoutUseCase = new LogoutUseCase(mockAuthService.Object);

            var controller = new AuthController(loginUseCase, refreshUseCase, logoutUseCase);

            // Act
            var result = await controller.Login(new LoginRequest { Email = "bad@e.mail", Password = "wrong" });

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result); // AuthController renvoie Unauthorized si jwt == null
            mockAuthService.Verify(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
