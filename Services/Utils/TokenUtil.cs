using System.Text;
using System.Security.Claims;
using Chime_ASPNET.Models.Config;
using Chime_ASPNET.Models.Entities;
using Chime_ASPNET.Models.Dtos.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Chime_ASPNET.Services.Utils;

public static class TokenUtil
{
    public static string GenerateToken(User user, JWTSettings jwt, Token.TokenType type)
    {
        var now = DateTime.Now;
        var expires = type switch
        {
            Token.TokenType.Refresh => now.AddDays(jwt.RefreshTokenExpiry),
            Token.TokenType.Access => now.AddDays(jwt.AccessTokenExpiry),
            _ => now.AddMinutes(jwt.RefreshTokenExpiry)
        };

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (type != Token.TokenType.Refresh)
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(
                type == Token.TokenType.Access
                    ? new(ClaimTypes.NameIdentifier, user.Id.ToString())
                    : new("purpose", "reset-password")
            );
        }

        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        var creds = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static TokenDto GenerateTokens(User user, JWTSettings jwt, string? access = null, string? refresh = null)
    {
        return new TokenDto
        {
            Access = (access is null)
                ? GenerateToken(user, jwt, Token.TokenType.Access)
                : access,
            Refresh = (refresh is null)
                ? GenerateToken(user, jwt, Token.TokenType.Refresh)
                : refresh
        };
    }

    public static ClaimsPrincipal? ValidateToken(string token, JWTSettings jwt, IHostEnvironment environment)
    {
        var isDevelopment = environment.IsDevelopment();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Base64UrlEncoder.DecodeBytes(jwt.Key);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = !isDevelopment,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = !isDevelopment,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
