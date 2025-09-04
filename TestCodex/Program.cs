using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;
using TestCodex;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.AllowPasswordFlow();
        options.AcceptAnonymousClients();
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough();
        options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequest>(builder =>
            builder.UseInlineHandler(async context =>
            {
                if (context.Request.GrantType == GrantTypes.Password)
                {
                    if (context.Request.Username == "user" && context.Request.Password == "password")
                    {
                        var identity = new ClaimsIdentity(context.Scheme.Name,
                            Claims.Name, Claims.Role);

                        identity.AddClaim(Claims.Subject, Guid.NewGuid().ToString());
                        identity.AddClaim(Claims.Name, context.Request.Username!,
                            Destinations.AccessToken, Destinations.IdentityToken);

                        var principal = new ClaimsPrincipal(identity);
                        principal.SetScopes(Scopes.OpenId);

                        context.Principal = principal;
                    }
                    else
                    {
                        context.Reject(error: Errors.InvalidGrant, description: "Invalid credentials.");
                    }
                }
            }));
    })
    .AddValidation();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
