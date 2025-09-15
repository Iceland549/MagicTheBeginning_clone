using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuthMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("api/service-auth")]
    public class ServiceAuthController : ControllerBase
    {
        private readonly GenerateServiceTokenUseCase _generateToken;

        public ServiceAuthController(GenerateServiceTokenUseCase generateToken)
        {
            _generateToken = generateToken;
        }

        [HttpPost("token")]
        public async Task<ActionResult<ServiceTokenResponse>> GetServiceToken([FromBody] ServiceTokenRequest req)
        {
            try
            {
                var result = await _generateToken.ExecuteAsync(req);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Erreur interne : " + ex.Message });
            }
        }
    }
}
