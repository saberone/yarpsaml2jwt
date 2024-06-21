using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Proxy.Configuration;

namespace Proxy.Infrastructure;

public class TokenIssuer : ITokenIssuer
{
    private readonly TimeSpan _expiration = TimeSpan.FromHours(1);
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private SigningCredentials _credentials;
    public static string Audience => "Super.API";
    public static string Issuer => "Proxy";

    public TokenIssuer(IOptions<TokenIssuerConfig> options)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(options.Value.TokenSigningKey));
        _credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
    }

    public string CreateToken(ClaimsIdentity identity, string? audience = null)
    {
        var nowUtc = DateTime.UtcNow;
        var issuedUtc = nowUtc.AddMinutes(-5); // Account for clock skew.

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = audience ?? Audience,
            Expires = nowUtc.Add(_expiration),
            IssuedAt = issuedUtc,
            Issuer = Issuer,
            NotBefore = issuedUtc,
            SigningCredentials = _credentials,
            Subject = identity
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }
}