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

    #region LoginUser
    public async Task<ApiResponse<AuthDto>> LoginUserAsync(LoginDto request)
    {
        return await LoginAsync(
            request,
            req => authRepository.GetUserByCredentialsAsync(request.Email),
            (req, user) => PasswordUtil.VerifyPassword(user!.Password!, req.Password)
        );
    }
    #endregion

    #region RefreshUserToken
    public async Task<ApiResponse<TokenDto>> RefreshUserTokenAsync(RefreshTokenDto request)
    {
        var validationErrors = new Dictionary<string, string>();

        var principal = TokenUtil.ValidateToken(request.Refresh, jwt, env);
        if (principal is null)
        {
            validationErrors.Add("token", "Invalid refresh token.");
            return ApiResponse<TokenDto>.ErrorResponse(
                Error.ValidationError,
                Error.ErrorType.ValidationError,
                validationErrors
            );
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var token = await authRepository.GetTokenByRefreshAsync(request.Refresh);
            if (token is null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            {
                validationErrors.Add("token", "Refresh token is already expired or invalid.");
                return ApiResponse<TokenDto>.ErrorResponse(
                    Error.Unauthorized, Error.ErrorType.Unauthorized, validationErrors);
            }

            if (!TokenUtil.IsTokenNearExpiration(principal, bufferMinutes: 10))
            {
                var newAccessToken = new TokenDto
                {
                    Refresh = request.Refresh,
                    Access = TokenUtil.GenerateToken(
                        token.User, jwt, Token.TokenType.Access
                    )
                };

                return ApiResponse<TokenDto>.SuccessResponse(Success.IS_AUTHENTICATED, newAccessToken);
            }

            token.IsRevoked = true;
            await context.SaveChangesAsync();

            var tokens = await CreateAndSaveTokensAsync(token.User);
            await transaction.CommitAsync();

            return ApiResponse<TokenDto>.SuccessResponse(Success.IS_AUTHENTICATED, tokens);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred in the refresh token service.");

            await transaction.RollbackAsync();
            return ApiResponse<TokenDto>.ErrorResponse(
                Error.ServerError,
                Error.ErrorType.InternalServerError
            );
        }
    }
    #endregion




}
