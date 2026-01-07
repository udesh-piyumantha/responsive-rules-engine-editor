# Responsive Rules Engine Editor

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4?logo=.net)](https://dotnet.microsoft.com)
[![GitHub Stars](https://img.shields.io/github/stars/udesh-piyumantha/responsive-rules-engine-editor?style=social)](https://github.com/udesh-piyumantha/responsive-rules-engine-editor)

A **production-ready, responsive Rules Engine Editor** for Microsoft Rules Engine with multi-storage provider support. Create, edit, and manage business logic workflows from a modern web UI.

## 🎯 Features

### ✅ Responsive Design
- Works on desktop, tablet, and mobile devices
- Touch-friendly interface
- Sidebar collapses on mobile for better UX
- Fluid grid layout adapts to screen size

### ✅ Multi-Storage Support
- **JSON File** - Local storage (D:\\RulesStorage)
- **AWS S3** - Cloud scalability and cost-effectiveness
- **Azure Blob Storage** - Microsoft cloud integration
- **Extensible** - Easy to add more providers (Google Cloud, etc.)

### ✅ Complete CRUD Operations
- Create new workflows
- Edit existing rules and conditions
- Delete workflows
- Real-time form and JSON synchronization

### ✅ Dual Editing Modes
- **Form View** - User-friendly interface for non-technical users
- **JSON View** - Direct JSON editing for advanced users
- Toggle between modes seamlessly

### ✅ Enterprise-Ready
- RESTful API with async/await patterns
- Dependency injection and factory patterns
- Comprehensive error handling and logging
- CORS support
- Input validation and sanitization

---

## 🚀 Quick Start

### Prerequisites
- .NET 6.0 or higher
- Microsoft Rules Engine 6.0+
- (Optional) AWS S3 or Azure Storage credentials

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/udesh-piyumantha/responsive-rules-engine-editor.git
   ```

2. **Copy backend files to your .NET project:**
   ```bash
   # Copy these folders to your project
   cp -r Models/ /path/to/your/project/Models/
   cp -r Services/ /path/to/your/project/Services/
   cp -r Controllers/ /path/to/your/project/Controllers/
   cp -r Configuration/ /path/to/your/project/Configuration/
   ```

3. **Install NuGet packages:**
   ```bash
   dotnet add package RulesEngine --version 6.0.0
   dotnet add package Azure.Storage.Blobs --version 12.18.0
   dotnet add package AWSSDK.S3 --version 3.7.300
   ```

4. **Configure appsettings.json:**
   ```json
   {
     "Storage": {
       "Type": "JsonFile",
       "JsonFilePath": "D:\\RulesStorage"
     }
   }
   ```

5. **Update Program.cs:**
   ```csharp
   using RulesEngineEditor.Configuration;
   
   builder.Services.AddRulesEngineEditor(builder.Configuration);
   builder.Services.AddCors(options =>
   {
       options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
   });
   
   app.UseCors("AllowAll");
   app.UseStaticFiles();
   app.MapControllers();
   ```

6. **Copy UI files:**
   ```bash
   cp wwwroot/index.html /path/to/your/project/wwwroot/
   ```

7. **Run your application:**
   ```bash
   dotnet run
   ```

8. **Access the UI:**
   ```
   http://localhost:5000/
   ```

---

## 📖 Documentation

- **[SETUP_GUIDE.md](./SETUP_GUIDE.md)** - Comprehensive setup instructions
- **[PROGRAM_CS_SETUP.md](./PROGRAM_CS_SETUP.md)** - Program.cs configuration examples
- **[appsettings.example.json](./appsettings.example.json)** - Configuration template

---

## 🏗️ Architecture

### Backend Structure

```
RulesEngineEditor/
├── Models/
│   └── WorkflowModels.cs           # Domain models and DTOs
├── Services/Storage/
│   ├── IStorageProvider.cs         # Interface abstraction
│   ├── JsonFileStorageProvider.cs  # Local storage implementation
│   ├── S3StorageProvider.cs        # AWS S3 implementation
│   ├── AzureBlobStorageProvider.cs # Azure Blob implementation
│   └── StorageProviderFactory.cs   # Factory pattern
├── Controllers/
│   └── RulesController.cs          # REST API endpoints
├── Configuration/
│   └── ServiceConfiguration.cs     # Dependency injection setup
└── wwwroot/
    └── index.html                  # Responsive UI
```

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/rules/workflows` | List all workflows |
| GET | `/api/rules/workflows/{name}` | Get specific workflow |
| POST | `/api/rules/workflows` | Create or update workflow |
| DELETE | `/api/rules/workflows/{name}` | Delete workflow |
| HEAD | `/api/rules/workflows/{name}` | Check if workflow exists |
| GET | `/api/rules/providers` | Get available storage providers |

---

## 🔧 Storage Providers

### JSON File (Default)

```json
{
  "Storage": {
    "Type": "JsonFile",
    "JsonFilePath": "D:\\RulesStorage"
  }
}
```

**Files stored at:** `D:\RulesStorage\Rules\*.json`

### AWS S3

```json
{
  "Storage": {
    "Type": "S3",
    "S3": {
      "BucketName": "my-rules-bucket",
      "Prefix": "workflows/"
    }
  },
  "AWS": {
    "AccessKeyId": "your-key",
    "SecretAccessKey": "your-secret",
    "Region": "us-east-1"
  }
}
```

### Azure Blob Storage

```json
{
  "Storage": {
    "Type": "AzureBlob",
    "Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;...",
      "ContainerName": "workflows"
    }
  }
}
```

---

## 💻 Usage Example

### Create a Workflow

```csharp
private readonly StorageProviderFactory _storageFactory;

public async Task CreateWorkflow()
{
    var provider = _storageFactory.CreateProvider("jsonfile");
    
    var workflow = new WorkflowDefinition
    {
        Name = "OrderValidation",
        Description = "Validates order details",
        Rules = new List<RuleDefinition>
        {
            new RuleDefinition
            {
                Name = "CheckAmount",
                Expression = "input.Amount > 0",
                SuccessEvent = "AmountValid",
                Enabled = true
            }
        }
    };

    await provider.SaveWorkflowAsync(workflow);
}
```

### Execute a Workflow

```csharp
public async Task<bool> ValidateOrder(Order order)
{
    var provider = _storageFactory.CreateProvider("jsonfile");
    var workflowDef = await provider.GetWorkflowAsync("OrderValidation");
    
    var rulesEngine = new RulesEngine.RulesEngine(new[] { workflowDef });
    var result = await rulesEngine.ExecuteAllRulesAsync("OrderValidation", new { order });
    
    return result.All(r => r.IsSuccess);
}
```

---

## 🎨 UI Features

### Responsive Layout
- **Desktop:** Sidebar + Main content (two-column grid)
- **Tablet:** Sidebar collapses, responsive UI
- **Mobile:** Full-width interface, touch-optimized

### Workflow Management
- Create new workflows with description
- Select workflows from sidebar
- View workflow metadata (rule count, last updated)
- Delete workflows with confirmation

### Rule Editing
- Add rules with name, condition, success event
- Edit rule conditions
- Delete individual rules
- Real-time JSON synchronization

### Form & JSON Modes
- **Form View:** Structured input for non-technical users
- **JSON View:** Direct JSON editing with syntax support
- Seamless switching between modes
- Auto-sync between form and JSON

---

## 🔒 Security Considerations

1. **Input Validation** - All inputs are sanitized
2. **CORS** - Configurable for production
3. **Async Operations** - No blocking calls
4. **Error Handling** - Comprehensive exception handling
5. **Logging** - Full operation logging for audit trails

**Production Recommendations:**
- Use HTTPS only
- Implement authentication (Azure AD, Auth0)
- Use environment-specific configurations
- Enable HTTPS redirect
- Restrict CORS to specific origins
- Use managed identities for cloud storage

---

## 📊 Performance

- **Async/Await:** Non-blocking operations
- **Lazy Loading:** Workflows loaded on demand
- **Cloud Scalability:** S3 and Azure Blob for unlimited scale
- **Error Recovery:** Graceful error handling

---

## 🐛 Troubleshooting

### API not responding
```bash
# Check API health
curl http://localhost:5000/api/rules/providers
```

### Workflows not loading
1. Verify storage path exists and has write permissions
2. Check appsettings.json configuration
3. Review application logs
4. Verify correct storage provider is selected

### CORS errors
- Ensure CORS is configured in Program.cs
- Check allowed origins match your deployment

### Workflow not saving
- Verify storage credentials are correct
- Check file system permissions
- Verify S3/Azure credentials if using cloud storage

---

## 📝 Workflow JSON Schema

```json
{
  "name": "OrderValidation",
  "description": "Validates order details",
  "rules": [
    {
      "name": "CheckAmount",
      "expression": "input.Amount > 1000",
      "successEvent": "HighValueOrder",
      "errorMessage": "Order amount is below minimum",
      "ruleExpressionType": "LambdaExpression",
      "enabled": true
    }
  ],
  "createdAt": "2026-01-07T08:00:00Z",
  "updatedAt": "2026-01-07T08:30:00Z"
}
```

---

## 🚢 Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
CMD ["dotnet", "YourProject.dll"]
```

### Azure App Service

1. Configure Application Settings (appsettings.json values)
2. Set Storage Type to AzureBlob
3. Provide Azure Storage connection string
4. Deploy using Azure DevOps or GitHub Actions

### AWS

1. Deploy to EC2 or Elastic Beanstalk
2. Configure IAM roles for S3 access
3. Set Storage Type to S3
4. Provide S3 bucket name and region

---

## 📚 Learn More

- [Microsoft Rules Engine Documentation](https://github.com/microsoft/RulesEngine)
- [AWS S3 Documentation](https://docs.aws.amazon.com/s3/)
- [Azure Blob Storage Documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/)
- [.NET Dependency Injection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## 📄 License

MIT License - See [LICENSE](./LICENSE) for details

---

## 💡 Support

For issues, questions, or suggestions:
1. Check the [SETUP_GUIDE.md](./SETUP_GUIDE.md) for common problems
2. Open an [GitHub Issue](https://github.com/udesh-piyumantha/responsive-rules-engine-editor/issues)
3. Create a [GitHub Discussion](https://github.com/udesh-piyumantha/responsive-rules-engine-editor/discussions)

---

**Built with ❤️ for the .NET community**
