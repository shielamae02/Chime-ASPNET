using Chime_ASPNET.Models.Response;
using Chime_ASPNET.Models.Dtos.Auth;

namespace Chime_ASPNET.Services.Auth;

public interface IAuthService
{
    Task<ApiResponse<AuthDto>> RegisterUserAsync(RegisterDto request);
    Task<ApiResponse<AuthDto>> LoginUserAsync(LoginDto request);
    Task<ApiResponse<AuthDto>> OAuthCallbackAsync(OAuthDto request);
    Task<ApiResponse<TokenDto>> RefreshUserTokenAsync(RefreshTokenDto request);
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto request);
    Task<ApiResponse<object>> ResetPasswordAsync(string resetToken, ResetPasswordDto request);
    Task<bool> LogoutUserAsync(RefreshTokenDto request);
    Task CleanUpTokensAsync();
}
