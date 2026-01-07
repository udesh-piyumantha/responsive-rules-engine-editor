# Program.cs Configuration

Here's how to configure your `Program.cs` file to use the Rules Engine Editor:

## Complete Program.cs Example

```csharp
using RulesEngineEditor.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== ADD RULES ENGINE EDITOR SERVICES ==========
builder.Services.AddRulesEngineEditor(builder.Configuration);
// ====================================================

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ========== ADD CORS MIDDLEWARE ==========
app.UseCors("AllowAll");
// ========================================

// Serve static files (for index.html)
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// Optional: Map root to index.html
app.MapGet("/", () => Results.File("wwwroot/index.html", "text/html"));

app.Run();
```

## For .NET 6+ (Minimal APIs)

If you're using minimal APIs without a Startup class:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Rules Engine Editor services
builder.Services.AddRulesEngineEditor(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseStaticFiles();
app.MapControllers();

app.Run();
```

## For .NET Framework (Traditional Startup)

If you're using .NET Framework with a Startup class:

```csharp
public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRulesEngineEditor(Configuration);
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseStaticFiles();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Key Configuration Points

### 1. Add Rules Engine Services
```csharp
builder.Services.AddRulesEngineEditor(builder.Configuration);
```
This registers:
- `JsonFileStorageProvider`
- `S3StorageProvider`
- `AzureBlobStorageProvider`
- `StorageProviderFactory`
- And their dependencies

### 2. Configure CORS
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

**Production CORS (Restricted):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedCors", builder =>
    {
        builder.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
               .WithMethods("GET", "POST", "DELETE")
               .WithHeaders("Content-Type");
    });
});
```

### 3. Enable Static Files
```csharp
app.UseStaticFiles();
```
This serves `wwwroot/index.html`

### 4. Map Controllers
```csharp
app.MapControllers();
```
This maps the `RulesController` endpoints

---

## Verify Installation

After configuration, verify the setup:

1. **Check API Health:**
   ```bash
   curl http://localhost:5000/api/rules/providers
   ```
   Should return:
   ```json
   {
     "providers": [
       { "name": "jsonfile", "label": "JSON File (Local)", "configured": true },
       { "name": "s3", "label": "AWS S3", "configured": false },
       { "name": "azureblob", "label": "Azure Blob Storage", "configured": false }
     ]
   }
   ```

2. **Access UI:**
   Navigate to `http://localhost:5000/`
   You should see the Rules Engine Editor UI

3. **Check Workflows:**
   ```bash
   curl http://localhost:5000/api/rules/workflows?provider=jsonfile
   ```

---

## Troubleshooting

### Issue: "AddRulesEngineEditor not found"

**Solution:**
1. Verify you've created the `Configuration/ServiceConfiguration.cs` file
2. Verify the namespace matches your project
3. Add `using YourNamespace.Configuration;` at the top

### Issue: "StorageProviderFactory not found"

**Solution:**
1. Verify all storage provider files are copied to your project
2. Verify namespaces match
3. Rebuild solution

### Issue: CORS errors in browser console

**Solution:**
Add CORS middleware before routing:
```csharp
app.UseCors("AllowAll"); // Must be before MapControllers
app.MapControllers();
```

### Issue: 404 when accessing `/api/rules/workflows`

**Solution:**
1. Verify `RulesController.cs` is in your Controllers folder
2. Verify the namespace
3. Verify `app.MapControllers()` is called in Program.cs

---

## Environment-Specific Configuration

### Development (appsettings.Development.json)
```json
{
  "Storage": {
    "Type": "JsonFile",
    "JsonFilePath": "D:\\RulesStorage"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Production (appsettings.Production.json)
```json
{
  "Storage": {
    "Type": "AzureBlob",
    "Azure": {
      "ConnectionString": "[SECRET]",
      "ContainerName": "workflows"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## Next Steps

1. ✅ Update your Program.cs
2. ✅ Add NuGet packages
3. ✅ Copy backend files
4. ✅ Configure appsettings.json
5. ✅ Test the API
6. ✅ Access the UI

You're ready to start using the Rules Engine Editor!
