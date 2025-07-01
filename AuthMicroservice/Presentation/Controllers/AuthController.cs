using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUseCase _login;
        private readonly RefreshTokenUseCase _refresh;
        private readonly LogoutUseCase _logout;

        public AuthController(LoginUseCase login, RefreshTokenUseCase refresh, LogoutUseCase logout )
        {
            _login = login;
            _refresh = refresh;
            _logout = logout;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // Appel du UseCase pour se connecter
            var jwt = await _login.ExecuteAsync(req.Email, req.Password);
            if (jwt == null)
                return Unauthorized("Identifiants invalides");
            return Ok(jwt);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            // Appel du UseCase pour rafraîchir le token
            var jwt = await _refresh.ExecuteAsync(req.RefreshToken);
            if (jwt == null)
                return Unauthorized("Refresh token invalide");
            return Ok(jwt);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
        {
            await _logout.ExecuteAsync(req.RefreshToken);
            return NoContent();
        }
    }
}