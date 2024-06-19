using System.Security.Claims;
using Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var tokenIssuerSection = builder.Configuration.GetSection("TokenIssuer");

// Parse it to my custom section's settings class
var tokenIssuerConfig = tokenIssuerSection.Get<TokenIssuerConfig>() 
                        ?? throw new ArgumentException("TokenIssuerConfig not configured");

var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(tokenIssuerConfig.TokenSigningKey));

// Register it for services who ask for an IOptions<FooSettings>
builder.Services.Configure<TokenIssuerConfig>(tokenIssuerSection);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateActor = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateTokenReplay = false,
            ValidIssuer = "Proxy",
            IssuerSigningKey = securityKey
        };
        opt.Audience = "Super.API";
        
        opt.Events = new JwtBearerEvents()
        {
            OnAuthenticationFailed = c =>
            {
                // do some logging or whatever..
                var ex = c.Exception;

                return Task.CompletedTask;
            }

        };

    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ClaimsPrincipal user, HttpContext context) =>
    {
        
        var claims = user.Claims.ToList();
        
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}