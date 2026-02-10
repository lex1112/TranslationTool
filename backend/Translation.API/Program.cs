using Translation.Infrastructure;
using Translation.Infrastructure.Identity;
using translation_app.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Modular Configurations
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppIdentity();
builder.Services.AddAppOpenIddict(builder.Configuration);
builder.Services.AddAppInfrastructure();

var app = builder.Build();

// Pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseCors(); // Uses the default policy defined in extensions

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Automatic Migration & Seeding
await app.InitializeDatabase();

app.Run();
