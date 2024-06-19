using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.Extensions.Options;

namespace Proxy.Configuration;

public class WsFederationConfig
{
      public required string MetadataAddress { get; set; }
      public required string Wtrealm { get; set; }
}

public class ConfigureWsFed 
      : IPostConfigureOptions<WsFederationOptions>
{     
      private readonly WsFederationConfig _wsFederationConfig;

      public ConfigureWsFed(IOptions<WsFederationConfig> options)
      {
            _wsFederationConfig = options.Value;
      }

      public void PostConfigure(string? name, WsFederationOptions options)
      {
            options.MetadataAddress = _wsFederationConfig.MetadataAddress;
            options.Wtrealm = _wsFederationConfig.Wtrealm;
            
            options.Events.OnSecurityTokenReceived = context =>
            {
                  var bla = context.ProtocolMessage.GetToken();
            
                  var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(bla);
                  var base64Encoded = System.Convert.ToBase64String(plainTextBytes);
            
                  Console.WriteLine(base64Encoded);
                  return Task.CompletedTask;
            };
      }
}