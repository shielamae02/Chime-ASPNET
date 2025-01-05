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
        
    }
}