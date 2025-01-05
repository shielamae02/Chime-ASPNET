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

    #region ForgotPassword
    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto request)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var user = await authRepository.GetUserByCredentialsAsync(request.Email);

            if (user is not null && user.Provider == User.AuthenticationType.Local)
            {
                var resetToken = TokenUtil.GenerateToken(user, jwt, Token.TokenType.Reset);
                var resetUrl = $"{app.URL}?token={resetToken}";

                await authRepository.SaveTokenAsync(
                    user,
                    resetToken,
                    DateTime.UtcNow.AddMinutes(jwt.ResetTokenExpiry),
                    Token.TokenType.Reset
                );

                emailQueue.QueueEmail(
                    [user.Email],
                    "Password Reset Request",
                    EmailTemplate.ForgotPasswordTemplate("Password Reset Request", resetUrl)
                );

                await transaction.CommitAsync();
            }

            return ApiResponse<object>.SuccessResponse(Success.PASSWORD_RESET_LINK_SENT, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred in the forgot password service.");

            await transaction.RollbackAsync();
            return ApiResponse<object>.ErrorResponse(
                Error.ServerError, Error.ErrorType.InternalServerError);
        }
    }
    #endregion

    #region ResetPassword
    public async Task<ApiResponse<object>> ResetPasswordAsync(string resetToken, ResetPasswordDto request)
    {
        var validationErrors = new Dictionary<string, string>();

        var principal = TokenUtil.ValidateToken(resetToken, jwt, env);
        if (principal is null)
        {
            validationErrors.Add("token", "Invalid reset token.");
            return ApiResponse<object>.ErrorResponse(
                Error.Unauthorized,
                Error.ErrorType.Unauthorized,
                validationErrors
            );
        }

        var purposeClaim = principal.Claims.FirstOrDefault(
            c => c.Type == "purpose" && c.Value == "reset-password")?.Value;

        var emailClaim = principal.Claims.FirstOrDefault(
            c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(purposeClaim) || string.IsNullOrEmpty(emailClaim))
        {
            validationErrors.Add("token", "Invalid reset token.");
            return ApiResponse<object>.ErrorResponse(
                Error.Unauthorized,
                Error.ErrorType.Unauthorized,
                validationErrors
            );
        }

        var user = await authRepository.GetUserByCredentialsAsync(emailClaim);
        if (user is null)
        {
            validationErrors.Add("user", "Invalid credentials.");
            return ApiResponse<object>.ErrorResponse(
               Error.Unauthorized,
               Error.ErrorType.Unauthorized,
               validationErrors
           );
        }

        var isTokenValid = user.Tokens.Any(t =>
            t.Value == resetToken &&
            !t.IsRevoked &&
            t.ExpiresAt > DateTime.UtcNow
        );

        if (!isTokenValid)
        {
            validationErrors.Add("token", "It looks like you clicked on an invalid password reset link. Please try again.");
            return ApiResponse<object>.ErrorResponse(
               Error.Unauthorized,
               Error.ErrorType.Unauthorized,
               validationErrors
           );
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var activeTokens = user.Tokens.Where(t => t.ExpiresAt > DateTime.Now && !t.IsRevoked);
            foreach (var activeToken in activeTokens)
            {
                activeToken.IsRevoked = true;
            }

            user.Password = PasswordUtil.HashPassword(request.Password);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ApiResponse<object>.SuccessResponse(
                Success.RESOURCE_UPDATED("Password"), null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred in the reset password service.");

            await transaction.RollbackAsync();
            return ApiResponse<object>.ErrorResponse(
                Error.ValidationError,
                Error.ErrorType.InternalServerError
            );
        }
    }
    #endregion

    #region LogoutUser
    public async Task<bool> LogoutUserAsync(RefreshTokenDto request)
    {
        var token = await authRepository.GetTokenByRefreshAsync(request.Refresh);

        if (token is null) return false;

        token.IsRevoked = true;
        await context.SaveChangesAsync();

        return true;
    }
    #endregion

    #region RegisterAsync
    private async Task<ApiResponse<AuthDto>> RegisterAsync<T>(
        T request, Action<User, T>? customizeUser = null) where T : BaseUserDto
    {
        var validationErrors = new Dictionary<string, string>();

        var isUserExist = await authRepository.IsUserExistByCredentialAsync(request.Email, request.Username);
        if (isUserExist)
        {
            validationErrors.Add("credentials", "Invalid credentials.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ValidationError,
                Error.ErrorType.ValidationError,
                validationErrors
            );
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var user = mapper.Map<User>(request);
            customizeUser?.Invoke(user, request);

            await authRepository.AddUserAsync(user);

            var tokens = await CreateAndSaveTokensAsync(user);

            var response = new AuthDto
            {
                Token = tokens,
                User = mapper.Map<UserDto>(user)
            };

            await transaction.CommitAsync();
            return ApiResponse<AuthDto>.SuccessResponse(Success.IS_AUTHENTICATED, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred in the register service.");

            await transaction.RollbackAsync();
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ServerError,
                Error.ErrorType.InternalServerError
            );
        }
    }
    #endregion

    #region LoginAsync
    private async Task<ApiResponse<AuthDto>> LoginAsync<T>(
       T request, Func<T, Task<User?>> getUserFunc, Func<T, User?, bool>? validateCredentials = null)
    {
        var validationErrors = new Dictionary<string, string>();

        var user = await getUserFunc(request);

        if (user is null || (validateCredentials != null && !validateCredentials(request, user)))
        {
            validationErrors.Add("user", "Invalid user credentials.");
            return ApiResponse<AuthDto>.ErrorResponse(
                Error.ValidationError,
                Error.ErrorType.ValidationError,
                validationErrors
            );
        }

        var tokens = await CreateAndSaveTokensAsync(user);

        var response = new AuthDto
        {
            Token = tokens,
            User = mapper.Map<UserDto>(user)
        };

        return ApiResponse<AuthDto>.SuccessResponse(Success.IS_AUTHENTICATED, response);
    }
    #endregion



}
