using AutoMapper;
using Chime_ASPNET.Data;
using System.Security.Claims;
using Chime_ASPNET.Models.Utils;
using Chime_ASPNET.Models.Config;
using Chime_ASPNET.Services.Email;
using Chime_ASPNET.Services.Utils;
using Chime_ASPNET.Models.Response;
using Chime_ASPNET.Models.Entities;
using Chime_ASPNET.Models.Dtos.Auth;
using Chime_ASPNET.Models.Dtos.Users;
using Chime_ASPNET.Repositories.Auth;

namespace Chime_ASPNET.Services.Auth;

public class AuthService(
    DataContext context,
    IMapper mapper,
    ILogger<AuthService> logger,
    IAuthRepository authRepository,
    JWTSettings jwt,
    AppSettings app,
    IHostEnvironment env,
    EmailQueue emailQueue
) : IAuthService
{

}
