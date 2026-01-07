using Serilog;
using RulesEngineEditor.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ========== LOGGING CONFIGURATION ==========
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.WithProperty("Application", "RulesEngineEditor")
);

// ========== ADD SERVICES TO CONTAINER ==========

// Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Add Rules Engine Editor Services
// This registers all storage providers and the factory
builder.Services.AddRulesEngineEditor(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Rules Engine Editor API",
        Version = "v1",
        Description = "REST API for managing Microsoft Rules Engine workflows with multi-storage support",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Support",
            Email = "support@example.com"
        }
    });
    
    // Include XML comments if available
    var xmlFile = System.IO.Path.Combine(System.AppContext.BaseDirectory, "RulesEngineEditor.xml");
    if (System.IO.File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// Add CORS - Configure for your deployment
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsPolicy =>
    {
        corsPolicy
            .AllowAnyOrigin()          // Change to specific origins in production
            .AllowAnyMethod()          // GET, POST, DELETE, etc.
            .AllowAnyHeader()          // Any headers
            .WithExposedHeaders("Content-Disposition"); // For file downloads
    });

    // Production CORS policy (example)
    options.AddPolicy("AllowProduction", corsPolicy =>
    {
        corsPolicy
            .WithOrigins(
                "https://yourdomain.com",
                "https://www.yourdomain.com"
            )
            .WithMethods("GET", "POST", "DELETE", "HEAD")
            .WithHeaders("Content-Type", "Authorization")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// ========== BUILD APPLICATION ==========
var app = builder.Build();

// ========== CONFIGURE MIDDLEWARE PIPELINE ==========

// Logging middleware
app.UseSerilogRequestLogging();

// Swagger/OpenAPI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rules Engine Editor API v1");
        options.RoutePrefix = "swagger";
    });
    app.UseDeveloperExceptionPage();
}

// HTTPS Redirect (production)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ========== CORS MIDDLEWARE (MUST BE BEFORE ROUTING) ==========
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "AllowProduction";
app.UseCors(corsPolicy);

// ========== STATIC FILES (FOR SERVING UI) ==========
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = false,
    DefaultContentType = "application/octet-stream"
});

// ========== ROUTING ==========
app.UseRouting();

// Authorization (add if you implement authentication)
// app.UseAuthorization();

// ========== ENDPOINT MAPPING ==========
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// API endpoint listing (using minimal API)
app.MapGet("/api", () => new
{
    version = "1.0",
    framework = ".NET 8",
    endpoints = new
    {
        workflows = "/api/rules/workflows",
        providers = "/api/rules/providers",
        swagger = "/swagger",
        health = "/health"
    }
})
.WithName("GetApiInfo")
.Produces<object>(StatusCodes.Status200OK)
.WithDescription("Get API information and available endpoints");

// Serve UI at root
//app.MapGet("/", () => Results.File("wwwroot/index.html", "text/html"))
//    .WithName("GetUI");

app.MapGet("/", () => Results.Redirect("/index.html"));

// ========== LOG STARTUP INFORMATION ==========
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("========================================");
logger.LogInformation("Rules Engine Editor API Starting");
logger.LogInformation(".NET Version: {DotnetVersion}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Storage Type: {StorageType}", builder.Configuration["Storage:Type"]);
logger.LogInformation("Swagger UI: http://localhost:5000/swagger");
logger.LogInformation("Rules Engine Editor: http://localhost:5000/");
logger.LogInformation("Health Check: http://localhost:5000/health");
logger.LogInformation("========================================");

// ========== RUN APPLICATION ==========
app.Run();
