using Translation.Infrastructure;
using Translation.Infrastructure.Identity;
using translation_app.Middleware;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Modular Configurations
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppIdentity();
builder.Services.AddAppOpenIddict(builder.Configuration);
builder.Services.AddAppInfrastructure();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    // Integrate XML Comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        if (api.RelativePath.Contains("connect")) return new[] { "OpenID Connect" };
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });

    options.DocInclusionPredicate((name, api) => true);
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });
}

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
