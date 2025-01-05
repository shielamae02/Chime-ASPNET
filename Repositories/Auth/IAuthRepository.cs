using Chime_ASPNET.Models.Entities;

namespace Chime_ASPNET.Repositories.Auth;

public interface IAuthRepository
{
    Task<bool> IsUserExistByCredentialAsync(
        string email,
        string? username = null,
        string? providerId = null,
        User.AuthenticationType? provider = null
    );

    Task<User?> GetUserByCredentialsAsync(
        string email,
        string? providerId = null,
        User.AuthenticationType? provider = null);

    Task<Token?> GetTokenByRefreshAsync(string refresh);
    Task RemoveRevokedTokensAsync();
    Task AddUserAsync(User user);
    Task SaveTokenAsync(User user, string refresh, DateTime expires, Token.TokenType type);
}
