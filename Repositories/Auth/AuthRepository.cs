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

    }
}