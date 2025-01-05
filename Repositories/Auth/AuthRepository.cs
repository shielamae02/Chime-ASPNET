using Chime_ASPNET.Data;
using Chime_ASPNET.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chime_ASPNET.Repositories.Auth
{
    public class AuthRepository(DataContext context) : IAuthRepository
    {

    }
}