# Responsive Rules Engine Editor - Setup Guide

## Overview

This is a production-ready Rules Engine Editor with multi-storage provider support for Microsoft Rules Engine workflows.

**Features:**
- ✅ Responsive UI (works on desktop, tablet, mobile)
- ✅ Multiple storage providers (JSON File, AWS S3, Azure Blob Storage)
- ✅ Real-time form & JSON editing
- ✅ CRUD operations for workflows
- ✅ RESTful API with async/await patterns
- ✅ Dependency injection and factory patterns

---

## Quick Start (Your Current Setup)

### Step 1: Install NuGet Packages

Add these to your `.csproj` file:

```xml
<ItemGroup>
    <PackageReference Include="RulesEngine" Version="6.0.0" />
    <!-- For Azure Blob Storage support -->
    <PackageReference Include="Azure.Storage.Blobs" Version="12.18.0" />
    <!-- For AWS S3 support (optional) -->
    <PackageReference Include="AWSSDK.S3" Version="3.7.300" />
</ItemGroup>
```

Or via Package Manager Console:

```powershell
NuGet\Install-Package RulesEngine -Version 6.0.0
NuGet\Install-Package Azure.Storage.Blobs -Version 12.18.0
NuGet\Install-Package AWSSDK.S3 -Version 3.7.300
```

### Step 2: Copy Backend Files to Your Project

1. Copy the following folders to your .NET project:
   - `Models/` → Your project's Models folder
   - `Services/Storage/` → Create if doesn't exist
   - `Controllers/RulesController.cs` → Your Controllers folder
   - `Configuration/ServiceConfiguration.cs` → Create if doesn't exist

2. Update the namespace in each file to match your project.

### Step 3: Configure appsettings.json

Add this configuration to your `appsettings.json`:

```json
{
  "Storage": {
    "Type": "JsonFile",
    "JsonFilePath": "D:\\RulesStorage",
    "S3": {
      "BucketName": "my-rules-bucket",
      "Prefix": "workflows/"
    },
    "Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
      "ContainerName": "workflows"
    }
  },
  "AWS": {
    "AccessKeyId": "your-access-key",
    "SecretAccessKey": "your-secret-key",
    "Region": "us-east-1"
  }
}
```

### Step 4: Register Services in Program.cs

In your `Program.cs` file, add the following in `builder.Services`:

```csharp
// Add Rules Engine Editor services
builder.Services.AddRulesEngineEditor(builder.Configuration);

// Add CORS if you're serving the UI from a different domain
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

Then add middleware:

```csharp
app.UseCors("AllowAll");
app.MapControllers();
```

### Step 5: Serve the UI

Copy `wwwroot/index.html` to your project's `wwwroot/` folder.

**For static file serving:**

Add to `Program.cs`:
```csharp
app.UseStaticFiles();
```

Then access at: `http://localhost:5000/index.html`

**OR for Blazor integration:**

If you're using Blazor, add as a component page:

```razor
@page "/rules-editor"
<iframe src="/index.html" style="width: 100%; height: 100vh; border: none;"></iframe>
```

---

## API Endpoints

All endpoints use the following query parameter to select storage provider:
`?provider=jsonfile|s3|azureblob`

### List Workflows
```
GET /api/rules/workflows?provider=jsonfile
```

Response:
```json
{
  "success": true,
  "message": "Retrieved 2 workflows from JSON File Storage",
  "data": [
    {
      "name": "OrderValidation",
      "description": "Validates order details",
      "ruleCount": 3,
      "createdAt": "2026-01-07T08:00:00Z",
      "updatedAt": "2026-01-07T08:30:00Z"
    }
  ]
}
```

### Get Single Workflow
```
GET /api/rules/workflows/{name}?provider=jsonfile
```

### Create/Update Workflow
```
POST /api/rules/workflows?provider=jsonfile
Content-Type: application/json

{
  "name": "OrderValidation",
  "description": "Validates orders",
  "rules": [
    {
      "name": "CheckAmount",
      "expression": "input.Amount > 1000",
      "successEvent": "HighValueOrder",
      "errorMessage": "Order amount is below minimum",
      "ruleExpressionType": "LambdaExpression",
      "enabled": true
    }
  ]
}
```

### Delete Workflow
```
DELETE /api/rules/workflows/{name}?provider=jsonfile
```

### Check if Workflow Exists
```
HEAD /api/rules/workflows/{name}?provider=jsonfile
```

### Get Available Providers
```
GET /api/rules/providers
```

---

## Storage Provider Configuration

### JSON File Storage (Default)

**Use Case:** Local development, small teams, file-based storage

**Configuration:**
```json
{
  "Storage": {
    "Type": "JsonFile",
    "JsonFilePath": "D:\\RulesStorage"
  }
}
```

**Files are stored at:** `D:\RulesStorage\Rules\*.json`

---

### AWS S3 Storage

**Use Case:** Production, scalable, cost-effective cloud storage

**Prerequisites:**
1. AWS S3 bucket created
2. IAM user with S3 permissions
3. AWS credentials configured

**Configuration:**
```json
{
  "Storage": {
    "Type": "S3",
    "S3": {
      "BucketName": "my-rules-engine-bucket",
      "Prefix": "workflows/"
    }
  },
  "AWS": {
    "AccessKeyId": "AKIAIOSFODNN7EXAMPLE",
    "SecretAccessKey": "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
    "Region": "us-east-1"
  }
}
```

**IAM Policy Required:**
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::my-rules-engine-bucket",
        "arn:aws:s3:::my-rules-engine-bucket/*"
      ]
    }
  ]
}
```

---

### Azure Blob Storage

**Use Case:** Microsoft ecosystem, integrated with Azure infrastructure

**Prerequisites:**
1. Azure Storage Account created
2. Connection string from storage account
3. Container created (auto-created by code if not exists)

**Configuration:**
```json
{
  "Storage": {
    "Type": "AzureBlob",
    "Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=myaccountname;AccountKey=myaccountkey;EndpointSuffix=core.windows.net",
      "ContainerName": "workflows"
    }
  }
}
```

**Get Connection String:**
1. Go to Azure Storage Account
2. Settings → Access keys
3. Copy connection string

---

## Usage Examples

### Create a Workflow Programmatically

```csharp
private readonly StorageProviderFactory _storageFactory;

public async Task CreateOrderValidationWorkflow()
{
    var provider = _storageFactory.CreateProvider("jsonfile");
    
    var workflow = new WorkflowDefinition
    {
        Name = "OrderValidation",
        Description = "Validates order details before processing",
        Rules = new List<RuleDefinition>
        {
            new RuleDefinition
            {
                Name = "CheckAmount",
                Expression = "input.Amount > 0",
                SuccessEvent = "AmountValid",
                ErrorMessage = "Amount must be greater than zero",
                RuleExpressionType = "LambdaExpression",
                Enabled = true
            },
            new RuleDefinition
            {
                Name = "CheckCustomer",
                Expression = "!string.IsNullOrEmpty(input.CustomerId)",
                SuccessEvent = "CustomerValid",
                ErrorMessage = "Customer ID is required",
                Enabled = true
            }
        }
    };

    await provider.SaveWorkflowAsync(workflow);
}
```

### Load and Execute Workflow

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

## Troubleshooting

### Issue: API returns 500 error

**Check:**
1. Storage directory exists and app has write permissions
2. Connection string is correct
3. AWS/Azure credentials are valid
4. Check application logs for detailed error

### Issue: CORS errors

**Solution:**
Make sure CORS is configured in Program.cs:

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

app.UseCors("AllowAll");
```

### Issue: Can't access index.html

**Solution:**
1. Verify file is in `wwwroot/` folder
2. Ensure `app.UseStaticFiles()` is in Program.cs
3. Try accessing `http://localhost:5000/`

### Issue: Workflows not persisting

**Check:**
1. Storage directory/bucket has write permissions
2. Verify correct storage provider is selected
3. Check API response for error messages
4. Review application logs

---

## Performance Optimization

### For Large Workloads:

1. **Use S3 or Azure Blob** instead of local files
2. **Enable caching** in your API:
   ```csharp
   [ResponseCache(Duration = 300)] // 5 minutes
   [HttpGet("workflows")]
   public async Task<ActionResult<WorkflowResponse>> ListWorkflows()
   ```

3. **Implement pagination** for large workflow lists

4. **Use async/await** throughout (already implemented)

---

## Security Considerations

1. **Validate input** on all API endpoints
2. **Use HTTPS** in production
3. **Implement authentication** (add Auth0, Azure AD, etc.)
4. **Encrypt sensitive data** (AWS/Azure credentials)
5. **Use IAM policies** to limit storage access
6. **Enable audit logging** for compliance

---

## Next Steps

1. ✅ Copy backend files to your project
2. ✅ Configure storage provider in appsettings.json
3. ✅ Register services in Program.cs
4. ✅ Copy index.html to wwwroot/
5. ✅ Test API endpoints
6. ✅ Deploy to production

---

## Support

For issues or questions, check the GitHub repository:
[https://github.com/udesh-piyumantha/responsive-rules-engine-editor](https://github.com/udesh-piyumantha/responsive-rules-engine-editor)

---

## License

MIT License - Feel free to use in your projects
