using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using Translation.Domain.Entities;
using Translation.Infrastructure.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Translation.Infrastructure;

public static class InfraBuilder
{
    public static async Task InitializeDatabase(this IHost app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var appManager = services.GetRequiredService<IOpenIddictApplicationManager>();

            // Database Lifecycle
            // EnsureDeleted/Created is perfect for Docker-only "Fast Dev" mode
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Seed OIDC Clients
            await SeedOidcClients(appManager);

            // Seed Identity Users
            await SeedIdentityUsers(userManager);

            // Seed Domain Resources
            await SeedTextResources(context);
        }
    }

    private static async Task SeedOidcClients(IOpenIddictApplicationManager appManager)
    {
        if (await appManager.FindByClientIdAsync("php-client") == null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "php-client",
                ClientSecret = "secret-123",
                DisplayName = "PHP Frontend",
                RedirectUris = { new Uri("http://localhost:8000/login/callback") },
                PostLogoutRedirectUris = { new Uri("http://localhost:8000") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + Scopes.OpenId,
                    Permissions.Prefixes.Scope + Scopes.Email,
                    Permissions.Prefixes.Scope + Scopes.Profile,
                    Permissions.Prefixes.Scope + Scopes.Roles,
                    Permissions.Prefixes.Scope + "api" // Added 'api' scope for the resource
                }
            });
            Console.WriteLine("--- SEED: OIDC Client 'php-client' created. ---");
        }
    }

    private static async Task SeedIdentityUsers(UserManager<IdentityUser> userManager)
    {
        const string adminEmail = "admin@test.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                Console.WriteLine($"--- SEED: Admin user '{adminEmail}' created. ---");
            }
        }
    }

    private static async Task SeedTextResources(ApplicationDbContext context)
    {
        if (!await context.TextResources.AnyAsync())
        {
            // Following your Domain/DDD pattern
            var welcome = new TextResourceEntity("WELCOME_MSG");
            welcome.AddOrUpdateTranslation("en-US", "Welcome");
            welcome.AddOrUpdateTranslation("de-DE", "Willkommen");

            context.TextResources.Add(welcome);
            await context.SaveChangesAsync();
            Console.WriteLine("--- SEED: Text resources initialized. ---");
        }
    }
}

