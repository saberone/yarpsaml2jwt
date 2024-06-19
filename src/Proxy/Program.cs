using Mbb.Proxy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.Options;
using Proxy.Configuration;
using Proxy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Hello World from your BFF.");


app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();


app.Run();
return;


static void ConfigureServices(WebApplicationBuilder builder) {
    
    builder.Services.Configure<WsFederationConfig>(builder.Configuration.GetSection("WsFederation"));
    builder.Services.Configure<TokenIssuerConfig>(builder.Configuration.GetSection("TokenIssuer"));
    builder.Services.AddTransient<IPostConfigureOptions<WsFederationOptions>, ConfigureWsFed>();
    builder.Services.AddSingleton<TokenIssuer>();
    
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        .AddTransforms<JwtTransformProvider>();
    
    
    builder.Services.AddAuthentication(
            sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
            })
        .AddWsFederation()
        .AddCookie();
    
    builder.Services.AddAuthorization();
}
