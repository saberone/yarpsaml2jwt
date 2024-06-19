using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Proxy.Configuration;

namespace Proxy.Infrastructure;

public class TokenIssuer
{
    private readonly IOptions<TokenIssuerConfig> _options;
    private readonly TimeSpan _expiration = TimeSpan.FromHours(1);
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    public static string Audience => "Super.API";
    public static string Issuer => "Proxy";
    public IList<SigningCredentials> SigningCredentials { get; } = new List<SigningCredentials>();

    public TokenIssuer(IOptions<TokenIssuerConfig> options)
    {
        _options = options;
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(_options.Value.TokenSigningKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        
        SigningCredentials.Add(credentials);
    }

    public string CreateToken(ClaimsIdentity identity, string? audience = null)
    {
        var nowUtc = DateTime.UtcNow;
        var issuedUtc = nowUtc.AddMinutes(-5); // Account for clock skew.

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = audience ?? Audience,
            Expires = nowUtc.Add(this._expiration),
            IssuedAt = issuedUtc,
            Issuer = Issuer,
            NotBefore = issuedUtc,
            SigningCredentials = this.SigningCredentials.First(),
            Subject = identity
        };

        var token = this._tokenHandler.CreateToken(tokenDescriptor);
        return this._tokenHandler.WriteToken(token);
    }
}