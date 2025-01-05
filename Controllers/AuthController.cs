using Microsoft.AspNetCore.Mvc;
using Chime_ASPNET.Services.Auth;
using Chime_ASPNET.Models.Dtos.Auth;
using Chime_ASPNET.Controllers.Utils;
using System.ComponentModel.DataAnnotations;

namespace Chime_ASPNET.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController(
        IAuthService authService,
        ILogger<AuthController> logger
    ) : ControllerBase
    {
        [HttpPost("register")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ControllerUtil.GenerateValidationError(ModelState));

            try
            {
                var response = await authService.RegisterUserAsync(request);

                return response.Status.Equals("error")
                    ? ControllerUtil.GetResultFromError(response)
                    : StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while registering a new user.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> LoginUser([FromBody] LoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ControllerUtil.GenerateValidationError(ModelState));

            try
            {
                var response = await authService.LoginUserAsync(request);

                return response.Status.Equals("error")
                    ? ControllerUtil.GetResultFromError(response)
                    : Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while logging in the user.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("oauth-callback")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> OAuthCallback([FromBody] OAuthDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ControllerUtil.GenerateValidationError(ModelState));

            try
            {
                var response = await authService.OAuthCallbackAsync(request);

                return response.Status.Equals("error")
                    ? ControllerUtil.GetResultFromError(response)
                    : StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred during oauth callback.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("refresh")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> RefreshUserToken([FromBody] RefreshTokenDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ControllerUtil.GenerateValidationError(ModelState));

            try
            {
                var response = await authService.RefreshUserTokenAsync(request);

                return response.Status.Equals("error")
                    ? ControllerUtil.GetResultFromError(response)
                    : Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while refreshing the user token.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("logout")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> LogoutUser([FromBody] RefreshTokenDto request)
        {
            try
            {
                var response = await authService.LogoutUserAsync(request);

                return (!response)
                    ? BadRequest(new { Message = "Invalid refresh token." })
                    : NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while refreshing the user token.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("forgot-password")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ControllerUtil.GenerateValidationError(ModelState));

            try
            {
                var response = await authService.ForgotPasswordAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while refreshing the user token.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }

        [HttpPost("reset-password")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> ResetPassword(
           [Required][FromQuery] string resetToken,
           [FromBody] ResetPasswordDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ControllerUtil.GenerateValidationError(ModelState));

            try
            {
                var response = await authService.ResetPasswordAsync(resetToken, request);

                return response.Status.Equals("error")
                    ? ControllerUtil.GetResultFromError(response)
                    : Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred while trying to reset the user password.");
                return Problem("An error occurred while processing your request. Please try again later.");
            }
        }



    }
}