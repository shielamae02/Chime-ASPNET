using Microsoft.EntityFrameworkCore;

namespace Chime_ASPNET.Models.Entities;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public sealed class User : BaseEntity
{
    public enum AuthenticationType
    {
        Local,
        Google,
        Github
    }

    public AuthenticationType Provider { get; set; } = AuthenticationType.Local;
    public string Email { get; set; } = null!;
    public string? Username { get; set; }
    public string? ProviderId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Password { get; set; }

    public ICollection<Token> Tokens { get; init; } = new List<Token>();
}
