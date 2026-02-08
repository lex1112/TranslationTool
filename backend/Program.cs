

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using translation_app;
using translation_app.Middleware;
using translation_app.Model;
using translation_app.Provider;
using static OpenIddict.Abstractions.OpenIddictConstants;


var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5));
    options.UseOpenIddict();
});

// Identity Configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ITranslationProvider, DbTranslationProvider>();

// OpenIddict Configuration
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        // FIXED: Added leading slash "/"
        options.SetIssuer(new Uri("http://localhost:8080/"));

        options.SetAuthorizationEndpointUris("/connect/authorize")
               .SetTokenEndpointUris("/connect/token");

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

        options.AllowAuthorizationCodeFlow();


        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               // Note: Correct method name is Userinfo (not UserInfo)
               .EnableUserInfoEndpointPassthrough()
               .DisableTransportSecurityRequirement();
    })
     .AddValidation(options =>
     {
         // Указываем, что валидация происходит на этом же сервере
         options.UseLocalServer();

         // Подключаем интеграцию с ASP.NET Core
         options.UseAspNetCore();

         options.Configure(tokenOptions =>
         {
             tokenOptions.TokenValidationParameters.ValidateIssuer = false;
             tokenOptions.TokenValidationParameters.ValidateAudience = false;
             tokenOptions.TokenValidationParameters.ValidateLifetime = false;
             tokenOptions.TokenValidationParameters.RequireExpirationTime = false;
             tokenOptions.TokenValidationParameters.RequireSignedTokens = true; // Подпись оставить!
         });

         // Если PHP-фронт и .NET бэк на разных доменах, 
         // убедитесь, что Issuer совпадает с тем, что в токене
         // options.SetIssuer("http://localhost:5000/"); 
     });

// 4. CORS for PHP Frontend
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.SameSite = SameSiteMode.None; // Required for cross-site/port redirects
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Required if SameSite=None
});

// Also ensure CORS allows credentials
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:8000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // Mandatory for cookies
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});


builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// 5. Unified SEED Logic (Merged to avoid database conflicts)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var manager = services.GetRequiredService<IOpenIddictApplicationManager>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>(); ;

    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    // Seed PHP Client
    if (await manager.FindByClientIdAsync("php-client") == null)
    {
        await manager.CreateAsync(new OpenIddictApplicationDescriptor
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
    
                // Scopes must be added like this:
            Permissions.Prefixes.Scope + Scopes.OpenId,
            Permissions.Prefixes.Scope + Scopes.Email,
            Permissions.Prefixes.Scope + Scopes.Profile,
            Permissions.Prefixes.Scope + Scopes.Roles
}
        });
        Console.WriteLine("--- SEED: Client 'php-client' created. ---");
    }

    // Seed Admin User
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
        else
        {
            Console.WriteLine($"--- SEED ERROR: {string.Join(", ", result.Errors.Select(e => e.Description))} ---");
        }
    }

    if (!context.TextResources.Any())
    {
        var welcome = new TextResource { Sid = "WELCOME_MSG" };
        welcome.Translations.Add(new Translation { LangId = "en-US", Text = "Welcome (Default)" });
        welcome.Translations.Add(new Translation { LangId = "de-DE", Text = "Willkommen" });
        context.TextResources.Add(welcome);
        await context.SaveChangesAsync();
    }
}

app.Run();

