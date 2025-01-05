using Chime_ASPNET.Data;
using Chime_ASPNET.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chime_ASPNET.Repositories.Auth
{
    public class AuthRepository(DataContext context) : IAuthRepository
    {
        public async Task<bool> IsUserExistByCredentialAsync(
                    string email,
                    string? username = null,
                    string? providerId = null,
                    User.AuthenticationType? provider = null
                )
        {
            return await context.Users
                 .Where(u => (u.Email == email || u.Username == username)
                    && (providerId == null || u.ProviderId == providerId)
                    && (provider == null || u.Provider == provider))
                .AnyAsync();
        }

        public async Task<User?> GetUserByCredentialsAsync(string email, string? providerId = null, User.AuthenticationType? provider = null)
        {
            return await context.Users
                .Include(u => u.Tokens)
                .Where(u => u.Email == email
                            && (providerId == null || u.ProviderId == providerId)
                            && (provider == null || u.Provider == provider))
                .FirstOrDefaultAsync();
        }

        public async Task<Token?> GetTokenByRefreshAsync(string refresh)
        {
            return await context.Tokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Value == refresh);
        }

        public async Task RemoveRevokedTokensAsync()
        {
            await context.Tokens
                .Where(t => t.IsRevoked || t.ExpiresAt < DateTime.UtcNow)
                .ExecuteDeleteAsync();
        }

        public async Task AddUserAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        public async Task SaveTokenAsync(User user, string refresh, DateTime exprires, Token.TokenType type)
        {
            var token = new Token
            {
                User = user,
                UserId = user.Id,
                Value = refresh,
                ExpiresAt = exprires,
                Type = type
            };

            await context.Tokens.AddAsync(token);
            await context.SaveChangesAsync();
        }
    }
}