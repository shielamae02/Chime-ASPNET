using System.ComponentModel.DataAnnotations;

namespace Chime_ASPNET.Models.Dtos.Auth;

public class RegisterDto : BaseUserDto
{
    [Required(ErrorMessage = "Username is required.")]
    public new string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}
