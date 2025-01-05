using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chime_ASPNET.Models.Entities;

[Index(nameof(Value), IsUnique = true)]
public sealed class Token : BaseEntity
{
    public enum TokenType
    {
        Access,
        Refresh,
        Reset
    }

    [ForeignKey(nameof(User))]
    public int UserId { get; init; }

    public string Value { get; set; } = null!;
    public bool IsRevoked { get; set; } = false;
    public DateTime ExpiresAt { get; init; }
    public TokenType Type { get; set; } = TokenType.Refresh;


    public User User { get; init; } = null!;
}
