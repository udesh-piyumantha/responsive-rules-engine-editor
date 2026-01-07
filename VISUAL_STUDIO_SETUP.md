# Visual Studio Setup Guide

## Opening the Project in Visual Studio

### Prerequisites
- **Visual Studio 2022** (version 17.4 or later) or **Visual Studio Code**
- **.NET 8 SDK** installed (download from [dotnet.microsoft.com](https://dotnet.microsoft.com))
- Git (for cloning the repository)

### Step 1: Clone the Repository

```bash
git clone https://github.com/udesh-piyumantha/responsive-rules-engine-editor.git
cd responsive-rules-engine-editor
```

### Step 2: Verify .NET 8 Installation

```bash
dotnet --version
# Should output: 8.x.x

dotnet --list-sdks
# Should show .NET 8.x.x in the list
```

### Step 3: Open in Visual Studio

#### Option A: Open Solution File (Recommended)

1. Open **Visual Studio 2022**
2. Click **File** → **Open** → **Solution/Project**
3. Navigate to the cloned folder
4. Select **`RulesEngineEditor.sln`**
5. Click **Open**

Visual Studio will automatically:
- Load the project
- Detect the .NET 8 SDK
- Analyze dependencies
- Prepare the project for building

#### Option B: Open via Visual Studio directly

1. In Visual Studio, click **File** → **Open Folder**
2. Browse to the repository folder
3. Click **Select Folder**

---

## Project Structure in Solution Explorer

Once opened, you should see:

```
RulesEngineEditor (Solution)
├── RulesEngineEditor (Project)
│   ├── 📁 Models/
│   │   └── WorkflowModels.cs
│   ├── 📁 Services/
│   │   └── 📁 Storage/
│   │       ├── IStorageProvider.cs
│   │       ├── JsonFileStorageProvider.cs
│   │       ├── S3StorageProvider.cs
│   │       ├── AzureBlobStorageProvider.cs
│   │       └── StorageProviderFactory.cs
│   ├── 📁 Controllers/
│   │   └── RulesController.cs
│   ├── 📁 Configuration/
│   │   └── ServiceConfiguration.cs
│   ├── 📁 wwwroot/
│   │   └── index.html
│   ├── Program.cs
│   ├── RulesEngineEditor.csproj
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
```

---

## Building the Project

### In Visual Studio

1. **Restore NuGet Packages:**
   - Right-click on the project → **Restore NuGet Packages**
   - Or: **Build** → **Clean Solution** → **Rebuild Solution**

2. **Build Solution:**
   - Press **Ctrl+Shift+B** or
   - Go to **Build** → **Build Solution**

3. **Verify Build:**
   - Check the **Output** window for:
     ```
     Build succeeded.
     ```

### Via Command Line

```bash
cd responsive-rules-engine-editor
dotnet restore
dotnet build
```

---

## Running the Application

### In Visual Studio (F5)

1. Press **F5** or click the **Start** button (play icon) in the toolbar
2. The application will:
   - Compile
   - Start the development server
   - Automatically open your default browser

3. You should see:
   ```
   ========================================
   Rules Engine Editor API Starting
   .NET Version: .NET 8.x.x
   Environment: Development
   Storage Type: JsonFile
   Swagger UI: http://localhost:5000/swagger
   Rules Engine Editor: http://localhost:5000/
   Health Check: http://localhost:5000/health
   ========================================
   ```

### Via Command Line

```bash
dotnet run
```

Then navigate to:
- **UI:** http://localhost:5000/
- **Swagger API:** http://localhost:5000/swagger
- **Health Check:** http://localhost:5000/health

---

## Configuration

### Storage Setup (JSON File)

The default configuration uses JSON File storage. Verify the directory:

#### In appsettings.json:
```json
"Storage": {
  "Type": "JsonFile",
  "JsonFilePath": "D:\\RulesStorage"
}
```

**Action Required:**

1. Create directory if it doesn't exist:
   - Windows: `D:\RulesStorage\Rules`
   - Or modify the path in `appsettings.json` to match your setup

2. Ensure your user has **Read/Write permissions** on the directory

---

## Debugging

### Debug Mode

1. **Set Breakpoints:** Click in the left margin of any code file
2. **Start Debugging:** Press **F5** or **Debug** → **Start Debugging**
3. **Step Through Code:** Use **F10** (step over) or **F11** (step into)
4. **View Variables:** Hover over variables or use the **Debug** window

### Debug Console Output

In Visual Studio, open the **Debug** window:
- **Debug** → **Windows** → **Output** (Ctrl+Alt+O)

You'll see:
```
Rules Engine Editor API Starting
.NET Version: .NET 8.x.x
Environment: Development
Storage Type: JsonFile
```

### Breakpoint Tips

1. **RulesController.cs** - Set breakpoints in API methods to debug requests
2. **JsonFileStorageProvider.cs** - Debug storage operations
3. **Program.cs** - Debug startup configuration

---

## Testing the API

### Option 1: Swagger UI (Recommended)

1. Start the application (F5)
2. Navigate to: `http://localhost:5000/swagger`
3. Expand endpoints and test directly from the UI
4. Example: Click **GET /api/rules/workflows** and click **Try it out**

### Option 2: Using cURL in PowerShell

```powershell
# List workflows
curl "http://localhost:5000/api/rules/workflows?provider=jsonfile"

# Get providers
curl "http://localhost:5000/api/rules/providers"

# Create workflow
$body = @{
    name = "TestWorkflow"
    description = "Test workflow"
    rules = @()
} | ConvertTo-Json

curl -Method POST "http://localhost:5000/api/rules/workflows?provider=jsonfile" `
  -Headers @{"Content-Type"="application/json"} `
  -Body $body
```

### Option 3: Visual Studio REST Client

Create a file named `requests.http` in the project root:

```http
### Get Workflows
GET http://localhost:5000/api/rules/workflows?provider=jsonfile

### Get Providers
GET http://localhost:5000/api/rules/providers

### Create Workflow
POST http://localhost:5000/api/rules/workflows?provider=jsonfile
Content-Type: application/json

{
  "name": "OrderValidation",
  "description": "Validates order details",
  "rules": []
}

### Get Single Workflow
GET http://localhost:5000/api/rules/workflows/OrderValidation?provider=jsonfile

### Delete Workflow
DELETE http://localhost:5000/api/rules/workflows/OrderValidation?provider=jsonfile
```

Then right-click on any request and select **Send Request**.

---

## Common Issues and Solutions

### Issue: "Port 5000 already in use"

**Solution:** Change the port in `launchSettings.json`:

1. Open `Properties\launchSettings.json`
2. Change `"applicationUrl": "http://localhost:5000"` to another port
3. Restart Visual Studio

### Issue: "The target framework does not exist"

**Solution:** Install .NET 8 SDK

```bash
# Check installed SDKs
dotnet --list-sdks

# Download .NET 8 from: https://dotnet.microsoft.com/download/dotnet/8.0
```

### Issue: "NuGet restore failed"

**Solution:** 

1. In Visual Studio: **Tools** → **NuGet Package Manager** → **Package Manager Console**
2. Run:
   ```powershell
   Update-Package -Reinstall
   ```

3. Or via command line:
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   ```

### Issue: "File not found: D:\\RulesStorage"

**Solution:**

1. Create the directory:
   ```bash
   mkdir D:\RulesStorage\Rules
   ```

2. Or modify `appsettings.json`:
   ```json
   "JsonFilePath": "C:\\Temp\\RulesStorage"
   ```

### Issue: "Access Denied" when saving workflows

**Solution:**

1. Check folder permissions on `D:\RulesStorage`
2. Ensure your user can read/write
3. Right-click folder → **Properties** → **Security** → **Edit**
4. Grant **Full Control** for your user

---

## Environment-Specific Builds

### Development Build

```bash
dotnet build --configuration Debug
```

This:
- Includes debugging symbols
- Disables optimizations
- Uses `appsettings.Development.json`
- Better for local development and debugging

### Production Build

```bash
dotnet build --configuration Release
```

This:
- Optimizes for performance
- Removes debugging info
- Uses `appsettings.Production.json`
- Ready for deployment

### In Visual Studio

Configuration dropdown at the top toolbar:
- Select **Debug** for development
- Select **Release** for production builds

---

## Publishing the Application

### Publish to Local Folder

1. Right-click project → **Publish**
2. Choose **Folder**
3. Create new folder: `bin/Release/PublishOutput`
4. Click **Publish**

Files appear in the publish folder, ready to deploy.

### Publish to Azure

1. Right-click project → **Publish**
2. Choose **Azure**
3. Select **Azure App Service**
4. Follow the wizard

### Publish to Docker

See `Dockerfile` in the repo for containerization.

---

## .NET 8 Features Used

### 1. **Minimal APIs**
```csharp
app.MapGet("/api", () => new { ... })
    .WithName("GetApiInfo")
    .WithOpenApi()
    .Produces<object>(StatusCodes.Status200OK);
```

### 2. **WithOpenApi() Extension**
Automatic OpenAPI/Swagger documentation for endpoints.

### 3. **Top-level Statements**
No `Program` class needed - cleaner syntax.

### 4. **Improved Performance**
- Faster startup time
- Better memory usage
- Enhanced JIT compiler optimizations

### 5. **Native AOT Ready**
Supports ahead-of-time compilation for even faster startups.

---

## Performance Profiling

### CPU Profiling

1. **Debug** → **Performance Profiler**
2. Select **CPU Usage**
3. Click **Start**
4. Use the application
5. Click **Stop** to see results

### Memory Profiling

1. **Debug** → **Performance Profiler**
2. Select **.NET Memory Usage**
3. Follow same workflow

---

## Useful Visual Studio Extensions

- **REST Client** - Test APIs directly in VS
- **Code Metrics** - Analyze code complexity
- **SonarLint** - Code quality analysis
- **GitLens** - Enhanced Git integration
- **.NET Enhancement Pack** - .NET 8 specific features

---

## Keyboard Shortcuts (Windows)

| Shortcut | Action |
|----------|--------|
| F5 | Start debugging |
| Shift+F5 | Stop debugging |
| Ctrl+Shift+B | Build solution |
| Ctrl+K, Ctrl+C | Comment code |
| Ctrl+K, Ctrl+U | Uncomment code |
| F12 | Go to definition |
| Shift+F12 | Find all references |
| Ctrl+H | Find and replace |

---

## Next Steps

1. ✅ Clone and open project
2. ✅ Verify .NET 8 SDK installed
3. ✅ Build solution
4. ✅ Create D:\RulesStorage\Rules directory
5. ✅ Press F5 to run
6. ✅ Visit http://localhost:5000/
7. ✅ Create test workflow
8. ✅ Read QUICK_REFERENCE.md for API details

---

## Support

- **Visual Studio Help:** https://docs.microsoft.com/en-us/visualstudio/
- **.NET 8 Docs:** https://learn.microsoft.com/en-us/dotnet/
- **ASP.NET Core Docs:** https://learn.microsoft.com/en-us/aspnet/core/
- **Project Issues:** GitHub Issues

---

**Happy coding with .NET 8! 🚀**
