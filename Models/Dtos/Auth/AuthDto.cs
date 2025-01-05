using Chime_ASPNET.Models.Dtos.User;

namespace Chime_ASPNET.Models.Dtos.Auth;

public class AuthDto
{
    public TokenDto Token { get; init; } = null!;
    public UserDto User { get; init; } = null!;
}
