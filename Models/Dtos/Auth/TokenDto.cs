namespace Chime_ASPNET.Models.Dtos.Auth;

public class TokenDto
{
    public string Access { get; init; } = string.Empty;
    public string Refresh { get; init; } = string.Empty;
}
