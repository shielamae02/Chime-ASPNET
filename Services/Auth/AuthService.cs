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
    #region OauthCallback
    public async Task<ApiResponse<AuthDto>> OAuthCallbackAsync(OAuthDto request)
    {
        var isUserExist = await authRepository.IsUserExistByCredentialAsync(
            request.Email,
            request.ProviderId,
            provider: request.Provider);

        if (isUserExist)
        {
            logger.LogInformation("Proceeding with login for email: {Email}, provider: {Provider}", request.Email, request.Provider);
            return await LoginAsync(
                request,
                req => authRepository.GetUserByCredentialsAsync(request.Email, request.ProviderId, request.Provider)
            );
        }

        logger.LogInformation("Proceeding with registration for email: {Email}, provider: {Provider}", request.Email, request.Provider);
        return await RegisterAsync(request, null);
    }
    #endregion
    #region RegisterUser
    public async Task<ApiResponse<AuthDto>> RegisterUserAsync(RegisterDto request)
    {
        return await RegisterAsync(request, (user, registerDto) =>
        {
            user.Password = PasswordUtil.HashPassword(registerDto.Password);
        });
    }
    #endregion


}
