# Project Structure

## Solution Layout

```
responsive-rules-engine-editor/
├── 📄 RulesEngineEditor.sln          # Visual Studio Solution file
├── 📄 RulesEngineEditor.csproj        # .NET 6 Project file
├── 📄 Program.cs                   # Application entry point
├── 📄 appsettings.json             # Base configuration
├── 📄 appsettings.Development.json  # Development environment config
├── 📄 appsettings.Production.json   # Production environment config
├── 📄 .gitignore                    # Git ignore patterns
├── 📄 README.md                     # Main project documentation
├── 📄 QUICK_REFERENCE.md           # Quick API reference
├── 📄 PROJECT_STRUCTURE.md         # This file
├── 📄 VISUAL_STUDIO_SETUP.md       # Visual Studio setup guide
├── 📁 Models/                        # Data models
│   └── WorkflowModels.cs             # Workflow and Rule models
├── 📁 Services/                      # Business logic layer
│   ├── 📁 Storage/
│   │   ├── IStorageProvider.cs         # Storage interface
│   │   ├── JsonFileStorageProvider.cs  # JSON File storage
│   │   ├── S3StorageProvider.cs        # AWS S3 storage
│   │   ├── AzureBlobStorageProvider.cs # Azure Blob storage
│   │   └── StorageProviderFactory.cs   # Factory pattern
│   └── WorkflowService.cs           # Business logic
├── 📁 Controllers/                  # API endpoints
│   └── RulesController.cs           # Rules API controller
├── 📁 Configuration/                # Dependency injection
│   └── ServiceConfiguration.cs      # Service registration
├── 📁 Properties/
│   └── launchSettings.json          # Development settings
├─┠ 📁 wwwroot/                       # Static files (UI)
│   └── index.html                   # React-based UI
├── 📁 bin/                          # Build output
└── 📁 obj/                          # Compilation cache
```

---

## File Descriptions

### Root Configuration Files

| File | Purpose | When to Edit |
|------|---------|---------------|
| `RulesEngineEditor.sln` | Visual Studio solution file | Never |
| `RulesEngineEditor.csproj` | Project file with NuGet dependencies | When adding packages |
| `Program.cs` | Application startup & middleware configuration | When adding services |
| `appsettings.json` | Base application configuration | Default storage setup |
| `appsettings.Development.json` | Development-only settings | Local testing |
| `appsettings.Production.json` | Production settings | Before deployment |
| `.gitignore` | Git exclude patterns | Never |

### Documentation Files

| File | Content |
|------|----------|
| `README.md` | Project overview, features, quick start |
| `QUICK_REFERENCE.md` | API endpoints, usage examples |
| `PROJECT_STRUCTURE.md` | This file - folder organization |
| `VISUAL_STUDIO_SETUP.md` | Step-by-step setup for VS |

### Directory Structure

#### `/Models`
**Purpose:** Data models and contracts

```csharp
// WorkflowModels.cs - Defines:
- Workflow { Name, Description, Rules }
- Rule { Name, Expression, SuccessEvent }
- WorkflowRequest { ... }
- WorkflowResponse { ... }
```

**When to Modify:** When adding new workflow properties or rule types

#### `/Services/Storage`
**Purpose:** Storage provider implementations (Strategy pattern)

**Files:**
- `IStorageProvider.cs` - Interface all providers implement
  ```csharp
  ListWorkflowsAsync()
  GetWorkflowAsync(name)
  SaveWorkflowAsync(workflow)
  DeleteWorkflowAsync(name)
  ```

- `JsonFileStorageProvider.cs` - Reads/writes workflows as JSON files
  ```
  D:\RulesStorage\Workflows\OrderValidation.json
  ```

- `S3StorageProvider.cs` - AWS S3 backend
  ```
  s3://prod-rules-bucket/workflows/OrderValidation.json
  ```

- `AzureBlobStorageProvider.cs` - Azure Blob Storage backend
  ```
  https://account.blob.core.windows.net/workflows-prod/OrderValidation.json
  ```

- `StorageProviderFactory.cs` - Creates appropriate provider based on config

**When to Modify:** When adding new storage backends (e.g., MongoDB, PostgreSQL)

#### `/Controllers`
**Purpose:** REST API endpoints

```csharp
// RulesController.cs
GET  /api/rules/providers              # List available storage providers
GET  /api/rules/workflows              # List workflows (queryable by provider)
GET  /api/rules/workflows/{name}       # Get specific workflow
POST /api/rules/workflows              # Create workflow
PUT  /api/rules/workflows/{name}       # Update workflow
DELETE /api/rules/workflows/{name}     # Delete workflow
```

**When to Modify:** When adding new API endpoints

#### `/Configuration`
**Purpose:** Dependency Injection setup

```csharp
// ServiceConfiguration.cs
public static void AddRulesEngineEditor(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Register all services
}
```

**Extension Method Pattern:**
```csharp
builder.Services.AddRulesEngineEditor(builder.Configuration);
```

**When to Modify:** When adding new services or dependencies

#### `/Properties`
**Purpose:** Launch settings and development profiles

```json
// launchSettings.json
{
  "profiles": {
    "IIS Express": { ... },
    "RulesEngineEditor": {
      "commandName": "Project",
      "applicationUrl": "http://localhost:5000",
      "launchBrowser": true
    }
  }
}
```

**When to Modify:** When changing development port or browser settings

#### `/wwwroot`
**Purpose:** Static files served by the app

```
wwwroot/
├── index.html          # Main UI entry point
├── css/
├── js/
└── images/
```

**When to Modify:** When updating the UI or adding static assets

#### `/bin` and `/obj`
**Purpose:** Build output (auto-generated)

**Never edit** - These are deleted on clean builds

---

## Configuration Hierarchy

### Default (Base) Configuration
**File:** `appsettings.json`
```json
{
  "Storage": {
    "Type": "JsonFile",
    "JsonFilePath": "D:\\RulesStorage"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Environment-Specific Overrides

**Development:** `appsettings.Development.json` (loaded when `ASPNETCORE_ENVIRONMENT=Development`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"  // Overrides base
    }
  }
}
```

**Production:** `appsettings.Production.json` (loaded when `ASPNETCORE_ENVIRONMENT=Production`)
```json
{
  "Storage": {
    "Type": "AzureBlob",  // Overrides base
    "Azure": {
      "ConnectionString": "${AZURE_CONNECTION_STRING}"
    }
  }
}
```

### Environment Variable Overrides

The highest priority:
```bash
ASPCORE_ENVIRONMENT=Production
Storage__Type=AzureBlob
Storage__Azure__ConnectionString=DefaultEndpointsProtocol=https;...
```

**Configuration Priority (lowest to highest):**
1. `appsettings.json` (base)
2. `appsettings.{Environment}.json`
3. Environment variables
4. Command-line arguments

---

## Storage Path Examples

### JSON File Storage
```
D:\RulesStorage\
└── Workflows\
    ├── OrderValidation.json
    ├── PaymentProcessing.json
    └── UserApproval.json
```

### AWS S3 Storage
```
s3://prod-rules-engine-bucket/
└── workflows/
    ├── OrderValidation.json
    ├── PaymentProcessing.json
    └── UserApproval.json
```

### Azure Blob Storage
```
https://rulesaccount.blob.core.windows.net/workflows-prod/
├── OrderValidation.json
├── PaymentProcessing.json
└── UserApproval.json
```

---

## Building the Solution

### Debug Build
```bash
dotnet build --configuration Debug
```

**Output:**
```
bin/Debug/net6.0/
├── RulesEngineEditor.exe          # Executable
├── RulesEngineEditor.pdb          # Debug symbols
└── appsettings*.json              # Configs
```

### Release Build
```bash
dotnet build --configuration Release
```

**Output:**
```
bin/Release/net6.0/
├── RulesEngineEditor.exe          # Optimized executable
└── appsettings*.json              # Configs
```

---

## Key Design Patterns

### 1. **Strategy Pattern** (Storage Providers)
```
IStorageProvider (interface)
  ├── JsonFileStorageProvider (strategy 1)
  ├── S3StorageProvider (strategy 2)
  └── AzureBlobStorageProvider (strategy 3)

StorageProviderFactory (context)
```

### 2. **Dependency Injection** (IoC Container)
```csharp
// Configuration
services.AddScoped<IStorageProvider>(sp => 
    storageFactory.CreateProvider(config));

// Usage
public RulesController(IStorageProvider provider) { }
```

### 3. **Extension Methods** (Service Registration)
```csharp
public static void AddRulesEngineEditor(
    this IServiceCollection services, 
    IConfiguration config) { }

// Called as:
builder.Services.AddRulesEngineEditor(builder.Configuration);
```

### 4. **Factory Pattern** (Storage Creation)
```csharp
public IStorageProvider CreateProvider(string type)
{
    return type switch {
        "JsonFile" => new JsonFileStorageProvider(),
        "S3" => new S3StorageProvider(),
        "AzureBlob" => new AzureBlobStorageProvider(),
        _ => throw new ArgumentException()
    };
}
```

---

## NuGet Dependencies

| Package | Version | Purpose |
|---------|---------|----------|
| `RulesEngine` | 6.0.0 | Core rules engine |
| `Swashbuckle.AspNetCore` | 6.5.0 | Swagger/OpenAPI |
| `Serilog` | 3.1.1 | Structured logging |
| `Azure.Storage.Blobs` | 12.18.0 | Azure integration |
| `AWSSDK.S3` | 3.7.300 | AWS integration |

**Check for Updates:**
```bash
dotnet outdated
```

**Update Package:**
```bash
dotnet add package [PackageName] --version [Version]
```

---

## Quick Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Clean
dotnet clean

# Publish
dotnet publish --configuration Release --output ./publish

# Watch mode (auto-rebuild on file changes)
dotnet watch run
```

---

## Next Steps

1. Read `README.md` for project overview
2. Read `VISUAL_STUDIO_SETUP.md` to open in VS
3. Read `QUICK_REFERENCE.md` for API usage
4. Start with `Program.cs` to understand startup
5. Explore `Controllers/RulesController.cs` for API logic
6. Examine `Services/Storage/` for storage implementations

---

**Project Complete! ✅**
