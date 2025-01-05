
using System.ComponentModel.DataAnnotations;

namespace Chime_ASPNET.Models.Dtos.Auth;

public class BaseUserDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Username must be at least 2 characters.")]
    public string? Username { get; init; }
}
