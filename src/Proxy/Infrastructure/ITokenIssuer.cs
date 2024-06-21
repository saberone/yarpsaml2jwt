using System.Security.Claims;

namespace Proxy.Infrastructure;

public interface ITokenIssuer
{
    string CreateToken(ClaimsIdentity identity, string? audience = null);
}