using Chime_ASPNET.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Chime_ASPNET.Models.Dtos.Auth;

public class OAuthDto : BaseUserDto
{
    [Required(ErrorMessage = "Provider is required.")]
    public User.AuthenticationType Provider { get; init; }

    [Required(ErrorMessage = "Provider ID is required.")]
    public string ProviderId { get; init; } = string.Empty;

    public string? ProfilePicture { get; init; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be at least 2 characters.")]
    public string? FirstName { get; init; }


    [StringLength(100, MinimumLength = 2, ErrorMessage = "LastName must be at least 2 characters.")]
    public string? LastName { get; init; }
}
