using Chime_ASPNET.Models.Entities;

namespace Chime_ASPNET.Models.Dtos.Users;

public class UserDto
{
    public int Id { get; init; }
    public User.AuthenticationType Provider { get; init; }
    public string? ProviderId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Username { get; init; } = string.Empty;
    public string? FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; } = string.Empty;
    public string? ProfilePicture { get; init; } = string.Empty;
}
