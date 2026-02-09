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
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var appManager = services.GetRequiredService<IOpenIddictApplicationManager>();

        // IMPORTANT: Use MigrateAsync instead of EnsureDeleted to keep your data
        await context.Database.MigrateAsync();

        // Seed PHP Client
        if (await appManager.FindByClientIdAsync("php-client") == null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "php-client",
                ClientSecret = "secret-123",
                DisplayName = "PHP Frontend",
                RedirectUris = { new Uri("http://localhost:8000/login/callback") },
                PostLogoutRedirectUris = { new Uri("http://localhost:8000") },
                Permissions = {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Prefixes.Scope + Scopes.OpenId,
                    Permissions.Prefixes.Scope + Scopes.Email,
                    Permissions.Prefixes.Scope + Scopes.Profile,
                    Permissions.Prefixes.Scope + Scopes.Roles
                }
            });
        }

        // Seed Admin User
        const string adminEmail = "admin@test.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(user, "Password123!");
        }

        // Seed Resources
        if (!await context.TextResources.AnyAsync())
        {
            // 1. Use the Constructor (it sets the Id and Sid internally)
            var welcome = new TextResourceEntity("WELCOME_MSG");

            // 2. Use the Domain Method (it handles the creation of TranslationEntity)
            welcome.AddOrUpdateTranslation("en-US", "Welcome");
            welcome.AddOrUpdateTranslation("de-DE", "Willkommen");

            // 3. Add to the DbContext
            context.TextResources.Add(welcome);

            await context.SaveChangesAsync();
            Console.WriteLine("--- SEED: Text resources created successfully. ---");
        }

    }
}
