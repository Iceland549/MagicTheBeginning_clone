using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly RegisterUserUseCase _register;
        private readonly GetProfileUseCase _profile;

        public AccountController(RegisterUserUseCase register, GetProfileUseCase profile)
        {
            _register = register;
            _profile = profile;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // Création d'un nouvel utilisateur avec rôle Player
            await _register.ExecuteAsync(req.Email, req.Password);
            return Ok("Inscription réussie");
        }

        [HttpGet("me"), Authorize]
        public async Task<IActionResult> Profile()
        {
            // Extrait le profil via AutoMapper dans le UseCase
            var dto = await _profile.ExecuteAsync(User);
            return Ok(dto);
        }
    }
}