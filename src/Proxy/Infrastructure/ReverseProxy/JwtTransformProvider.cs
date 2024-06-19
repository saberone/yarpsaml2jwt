using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using Proxy.Infrastructure;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Mbb.Proxy;

public class JwtTransformProvider : ITransformProvider
{
    private readonly TokenIssuer _tokenIssuer;
    // private readonly ApiTokenCacheClient _apiTokenClient;
 
    public JwtTransformProvider(TokenIssuer tokenIssuer)
    {
        _tokenIssuer = tokenIssuer;
    }
 
    public void Apply(TransformBuilderContext context)
    {
        if (context.Route.RouteId == "apiRoute")
        {
            context.AddRequestTransform(transformContext =>
            {
                var user = transformContext.HttpContext.User;

                if (user.Identity?.IsAuthenticated ?? false)
                {
                    var userId = user.Identity.Name;
                    // claims principal can have multiple identities
                    // TODO: logic to select the relevant identity
                    var identities = user.Identities;

                    var backendAppToken = _tokenIssuer.CreateToken(user.Identities.FirstOrDefault());
                    transformContext.ProxyRequest.Headers.Authorization
                          = new AuthenticationHeaderValue("Bearer", backendAppToken);
                }

                return ValueTask.CompletedTask;
            });
            
            context.RequestTransforms.Add(new RequestHeaderRemoveTransform("Cookie"));
        }
    }
 
    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }
 
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }
}