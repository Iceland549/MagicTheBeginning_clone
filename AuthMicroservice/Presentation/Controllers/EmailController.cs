using AuthMicroservice.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.Presentation.Controllers
{
    [ApiController]
    [Route("account")]
    public class EmailController : ControllerBase
    {
        private readonly GenerateEmailConfirmationUseCase _genConfirm;
        private readonly ConfirmEmailUseCase _confirm;
        private readonly GenerateResetPasswordUseCase _genReset;
        private readonly ResetPasswordUseCase _reset;

        public EmailController(
            GenerateEmailConfirmationUseCase genConfirm,
            ConfirmEmailUseCase confirm,
            GenerateResetPasswordUseCase genReset,
            ResetPasswordUseCase reset)
        {
            _genConfirm = genConfirm;
            _confirm = confirm;
            _genReset = genReset;
            _reset = reset;
        }

        [HttpPost("send-confirmation")]
        public async Task<IActionResult> SendConfirmation([FromBody] string email)
        {
            await _genConfirm.ExecuteAsync(email);
            return Ok("Email de confirmation envoyé");
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] string token)
        {
            await _confirm.ExecuteAsync(token);
            return Ok("Email confirmé");
        }

        [HttpPost("send-reset")]
        public async Task<IActionResult> SendReset([FromBody] string email)
        {
            await _genReset.ExecuteAsync(email);
            return Ok("Lien de réinitialisation envoyé");
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset([FromQuery] string token, [FromBody] string newPassword)
        {
            await _reset.ExecuteAsync(token, newPassword);
            return Ok("Mot de passe mis à jour");
        }
    }
}