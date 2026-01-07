# Quick Reference Guide

## 🔗 Project Links

- **GitHub:** https://github.com/udesh-piyumantha/responsive-rules-engine-editor
- **Setup Guide:** [SETUP_GUIDE.md](./SETUP_GUIDE.md)
- **Program.cs Config:** [PROGRAM_CS_SETUP.md](./PROGRAM_CS_SETUP.md)
- **Implementation Checklist:** [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md)

---

## 📦 File Structure

```
responsive-rules-engine-editor/
├── Models/
│   └── WorkflowModels.cs                 # Domain models
├── Services/Storage/
│   ├── IStorageProvider.cs               # Interface
│   ├── JsonFileStorageProvider.cs        # Local storage
│   ├── S3StorageProvider.cs              # AWS S3
│   ├── AzureBlobStorageProvider.cs       # Azure Blob
│   └── StorageProviderFactory.cs         # Factory pattern
├── Controllers/
│   └── RulesController.cs                # REST API
├── Configuration/
│   └── ServiceConfiguration.cs           # DI setup
├── wwwroot/
│   └── index.html                        # UI
├── README.md                             # Overview
├── SETUP_GUIDE.md                        # Detailed setup
├── PROGRAM_CS_SETUP.md                   # Program.cs examples
├── IMPLEMENTATION_CHECKLIST.md           # Integration checklist
├── appsettings.example.json              # Config template
└── QUICK_REFERENCE.md                    # This file
```

---

## ⚡ 5-Minute Setup

### 1. Install Packages
```bash
dotnet add package RulesEngine --version 6.0.0
dotnet add package Azure.Storage.Blobs --version 12.18.0
dotnet add package AWSSDK.S3 --version 3.7.300
```

### 2. Copy Files
```bash
# Copy from responsive-rules-engine-editor repo to your project
cp -r Models/ Services/ Controllers/ Configuration/ wwwroot/ .
```

### 3. Update appsettings.json
```json
{
  "Storage": {
    "Type": "JsonFile",
    "JsonFilePath": "D:\\RulesStorage"
  }
}
```

### 4. Update Program.cs
```csharp
using RulesEngineEditor.Configuration;

builder.Services.AddRulesEngineEditor(builder.Configuration);
builder.Services.AddCors(options => options.AddPolicy("AllowAll", b => 
    b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

app.UseCors("AllowAll");
app.UseStaticFiles();
app.MapControllers();
```

### 5. Run & Test
```bash
dotnet run
# Navigate to http://localhost:5000/
```

---

## 📋 API Quick Reference

### List Workflows
```bash
curl http://localhost:5000/api/rules/workflows?provider=jsonfile
```

### Create Workflow
```bash
curl -X POST http://localhost:5000/api/rules/workflows?provider=jsonfile \
  -H "Content-Type: application/json" \
  -d '{
    "name": "OrderValidation",
    "description": "Validate orders",
    "rules": []
  }'
```

### Get Workflow
```bash
curl http://localhost:5000/api/rules/workflows/OrderValidation?provider=jsonfile
```

### Delete Workflow
```bash
curl -X DELETE http://localhost:5000/api/rules/workflows/OrderValidation?provider=jsonfile
```

### Get Providers
```bash
curl http://localhost:5000/api/rules/providers
```

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

### AWS S3
```json
{
  "Storage": {
    "Type": "S3",
    "S3": {
      "BucketName": "my-bucket",
      "Prefix": "workflows/"
    }
  },
  "AWS": {
    "AccessKeyId": "AKIA...",
    "SecretAccessKey": "wJal...",
    "Region": "us-east-1"
  }
}
```

### Azure Blob
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

## 💻 Common Code Snippets

### Inject Storage Factory
```csharp
public class MyService
{
    private readonly StorageProviderFactory _factory;

    public MyService(StorageProviderFactory factory)
    {
        _factory = factory;
    }
}
```

### Save Workflow
```csharp
var provider = _factory.CreateProvider("jsonfile");
var workflow = new WorkflowDefinition
{
    Name = "MyWorkflow",
    Rules = new List<RuleDefinition>()
};
await provider.SaveWorkflowAsync(workflow);
```

### Load Workflow
```csharp
var workflow = await provider.GetWorkflowAsync("MyWorkflow");
```

### List Workflows
```csharp
var workflows = await provider.ListWorkflowsAsync();
```

### Delete Workflow
```csharp
await provider.DeleteWorkflowAsync("MyWorkflow");
```

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| "AddRulesEngineEditor not found" | Add `using RulesEngineEditor.Configuration;` |
| CORS errors | Add `app.UseCors("AllowAll");` in Program.cs |
| 404 on `/api/rules/workflows` | Verify `app.MapControllers();` in Program.cs |
| Workflows not saving | Check storage directory exists and has write permissions |
| UI not loading | Verify `app.UseStaticFiles();` and wwwroot/index.html exists |
| API offline | Check API URL in index.html matches your port |

---

## 🎯 UI Features

### Toolbar
- **New** - Create new workflow
- **Reload** - Refresh workflow list
- **Save** - Save current workflow
- **Delete** - Delete current workflow
- Storage Provider dropdown - Switch providers

### Sidebar
- Lists all workflows
- Click to select workflow
- Shows rule count and description
- Last updated timestamp

### Form View
- Workflow name input
- Description textarea
- Add Rule button
- Rules displayed as cards
- Remove button per rule

### JSON View
- Direct JSON editing
- Full workflow structure
- Real-time sync with form view

---

## 📱 Responsive Breakpoints

- **Desktop:** > 1024px (Sidebar + Content)
- **Tablet:** 768px - 1024px (Collapsed sidebar)
- **Mobile:** < 768px (Full width, touch optimized)

---

## 🔐 Security Checklist

- [ ] Use HTTPS in production
- [ ] Store secrets in environment variables
- [ ] Implement authentication
- [ ] Use IAM roles for cloud storage
- [ ] Enable logging and monitoring
- [ ] Validate all inputs
- [ ] Use CORS with specific origins

---

## 📊 Workflow JSON Schema

```typescript
interface WorkflowDefinition {
  name: string;
  description?: string;
  rules: Rule[];
  createdAt: Date;
  updatedAt: Date;
}

interface Rule {
  name: string;
  expression: string;           // Lambda expression
  successEvent: string;
  errorMessage?: string;
  ruleExpressionType: string;   // "LambdaExpression"
  enabled: boolean;
}
```

---

## 🚀 Deployment Checklist

### Before Deploy
- [ ] Verify all tests pass
- [ ] Review security settings
- [ ] Configure production appsettings
- [ ] Test all storage providers
- [ ] Verify HTTPS is configured

### After Deploy
- [ ] Test API endpoints
- [ ] Verify UI loads
- [ ] Create test workflow
- [ ] Monitor logs for errors
- [ ] Test workflow persistence

---

## 📚 Documentation Map

| Document | For | Contents |
|----------|-----|----------|
| README.md | Overview | Features, quick start, architecture |
| SETUP_GUIDE.md | Setup | Detailed configuration guide |
| PROGRAM_CS_SETUP.md | Config | Program.cs examples |
| IMPLEMENTATION_CHECKLIST.md | Integration | Step-by-step checklist |
| QUICK_REFERENCE.md | Daily use | This file - quick lookup |

---

## 🆘 Getting Help

1. **Check Docs:** Start with the relevant guide above
2. **Check Logs:** Enable debug logging to see detailed errors
3. **GitHub Issues:** Open an issue with error details
4. **GitHub Discussions:** Ask questions in discussions

---

## 📞 Support Resources

- Microsoft Rules Engine: https://github.com/microsoft/RulesEngine
- Azure Storage: https://docs.microsoft.com/azure/storage
- AWS S3: https://docs.aws.amazon.com/s3/
- .NET Docs: https://docs.microsoft.com/dotnet/

---

## 💡 Tips & Tricks

1. **Switch Storage Providers:** Use dropdown in UI or query parameter
2. **Export Workflows:** Download JSON from JSON view
3. **Batch Import:** Create JSON files and copy to storage location
4. **Version Control:** Commit workflow JSONs to git
5. **Backup:** Regularly backup your workflows

---

## 🎓 Learning Path

1. Read README.md (5 min)
2. Follow 5-Minute Setup (5 min)
3. Run local test (5 min)
4. Read SETUP_GUIDE.md (15 min)
5. Integrate into project (30 min)
6. Test and deploy (varies)

**Total:** ~1 hour to production ready

---

**Happy workflow editing! 🚀**
