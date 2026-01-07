# Implementation Checklist

Use this checklist to track your integration progress.

## Phase 1: Preparation

- [ ] Review the project structure and requirements
- [ ] Clone the [responsive-rules-engine-editor](https://github.com/udesh-piyumantha/responsive-rules-engine-editor) repository
- [ ] Read SETUP_GUIDE.md completely
- [ ] Identify your storage provider (JSON File, S3, Azure Blob)
- [ ] Prepare any cloud credentials (AWS keys, Azure connection string)

## Phase 2: Backend Integration

### NuGet Packages
- [ ] Install RulesEngine 6.0.0
- [ ] Install Azure.Storage.Blobs (if using Azure)
- [ ] Install AWSSDK.S3 (if using AWS S3)

### Copy Backend Files
- [ ] Copy `Models/WorkflowModels.cs` to your project
- [ ] Copy `Services/Storage/` folder to your project
  - [ ] IStorageProvider.cs
  - [ ] JsonFileStorageProvider.cs
  - [ ] S3StorageProvider.cs
  - [ ] AzureBlobStorageProvider.cs
  - [ ] StorageProviderFactory.cs
- [ ] Copy `Controllers/RulesController.cs` to your Controllers folder
- [ ] Copy `Configuration/ServiceConfiguration.cs` to your project

### Update Namespaces
- [ ] Update namespace in WorkflowModels.cs to match your project
- [ ] Update namespace in IStorageProvider.cs
- [ ] Update namespace in all storage provider implementations
- [ ] Update namespace in StorageProviderFactory.cs
- [ ] Update namespace in RulesController.cs
- [ ] Update namespace in ServiceConfiguration.cs
- [ ] Update using statements in Program.cs

### Configure appsettings.json
- [ ] Add Storage configuration section
- [ ] Set Storage:Type to your chosen provider
  - [ ] For JsonFile: Set JsonFilePath to "D:\\RulesStorage" or your path
  - [ ] For S3: Add BucketName and Prefix
  - [ ] For Azure: Add ConnectionString and ContainerName
- [ ] Add AWS configuration (if using S3)
  - [ ] AccessKeyId
  - [ ] SecretAccessKey
  - [ ] Region
- [ ] Verify directory exists (for JSON File provider)

### Update Program.cs
- [ ] Add `using YourNamespace.Configuration;`
- [ ] Add `using YourNamespace.Services.Storage;`
- [ ] Call `builder.Services.AddRulesEngineEditor(builder.Configuration);`
- [ ] Add CORS configuration:
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
- [ ] Add `app.UseCors("AllowAll");` after `app.Build()`
- [ ] Add `app.UseStaticFiles();`
- [ ] Verify `app.MapControllers();` is present
- [ ] (Optional) Add `app.MapGet("/", () => Results.File("wwwroot/index.html", "text/html"));`

### File Permissions
- [ ] Create/verify storage directory exists (for JSON File provider)
- [ ] Ensure application has write permissions to storage directory
- [ ] Create Rules subfolder if using JSON File provider

## Phase 3: Frontend Integration

### Copy UI Files
- [ ] Create `wwwroot/` folder if it doesn't exist
- [ ] Copy `wwwroot/index.html` to your project's wwwroot folder

### Update API URL (if needed)
- [ ] Open `wwwroot/index.html` in editor
- [ ] Find `const API_BASE_URL = 'http://localhost:5000/api/rules';`
- [ ] Update localhost:5000 to match your application port
- [ ] Verify URL is correct for your deployment environment

## Phase 4: Testing

### Local Development
- [ ] Build solution (Ctrl+Shift+B or `dotnet build`)
- [ ] Resolve any compilation errors
- [ ] Run the application (F5 or `dotnet run`)
- [ ] Wait for application to start
- [ ] Open browser to `http://localhost:5000/`
- [ ] Verify UI loads without errors
- [ ] Check browser console for JavaScript errors

### API Health Check
- [ ] Navigate to `http://localhost:5000/api/rules/providers`
- [ ] Verify JSON response shows available providers
- [ ] Confirm provider status matches your configuration

### Storage Provider Test
- [ ] Open Rules Engine Editor UI
- [ ] Create a new workflow with name "TestWorkflow"
- [ ] Click Create button
- [ ] Check for success message
- [ ] Verify workflow appears in sidebar
- [ ] Click workflow to select it
- [ ] Add a test rule
- [ ] Click Save
- [ ] Verify save success message
- [ ] Refresh page (F5)
- [ ] Verify workflow and rules persist

### CRUD Operations Test
- [ ] **Create:** Add new workflow - [ ] PASS [ ] FAIL
- [ ] **Read:** Load workflow from storage - [ ] PASS [ ] FAIL
- [ ] **Update:** Edit and save workflow - [ ] PASS [ ] FAIL
- [ ] **Delete:** Delete workflow - [ ] PASS [ ] FAIL
- [ ] **Form View:** Test form UI - [ ] PASS [ ] FAIL
- [ ] **JSON View:** Test JSON editing - [ ] PASS [ ] FAIL
- [ ] **Switch Views:** Toggle between form/JSON - [ ] PASS [ ] FAIL

### Storage Provider Specific Tests

#### JSON File Provider
- [ ] Check that files are created in the correct directory
- [ ] Verify file naming (should be `{WorkflowName}.json`)
- [ ] Open JSON file manually and verify content
- [ ] Edit file manually and reload in UI

#### AWS S3 Provider (if configured)
- [ ] Verify bucket exists in AWS console
- [ ] Upload a workflow
- [ ] Check that object appears in S3 console
- [ ] Download and verify content
- [ ] Test all CRUD operations

#### Azure Blob Provider (if configured)
- [ ] Verify container exists in Azure Storage Explorer
- [ ] Upload a workflow
- [ ] Check that blob appears in container
- [ ] Download and verify content
- [ ] Test all CRUD operations

### Responsive Design Test
- [ ] Test on desktop (1920x1080 or larger)
  - [ ] Sidebar displays correctly
  - [ ] Main content area is readable
  - [ ] All buttons are accessible
- [ ] Test on tablet (1024x768)
  - [ ] Layout is responsive
  - [ ] Sidebar is visible
  - [ ] No horizontal scrolling needed
- [ ] Test on mobile (375x667)
  - [ ] Sidebar collapses or adapts
  - [ ] UI is touch-friendly
  - [ ] All features are accessible

### Error Handling Test
- [ ] Try to create workflow with empty name - should show error
- [ ] Try to add rule with empty condition - should show error
- [ ] Try invalid JSON in JSON view - should show error
- [ ] Disconnect network and try operation - should show offline

## Phase 5: Production Preparation

### Security
- [ ] Review appsettings.json - no hardcoded secrets
- [ ] Move secrets to appsettings.Production.json or environment variables
- [ ] Configure CORS for production domain only
- [ ] Enable HTTPS
- [ ] Add authentication middleware
- [ ] Review access control for storage providers

### Configuration
- [ ] Create appsettings.Production.json
- [ ] Update database connection string
- [ ] Configure appropriate logging level
- [ ] Set correct storage provider for production
- [ ] Verify storage credentials are valid

### Deployment
- [ ] Choose deployment target (Azure App Service, AWS, Docker, etc.)
- [ ] Configure deployment environment
- [ ] Test in staging environment
- [ ] Verify API endpoints are accessible
- [ ] Verify UI loads correctly
- [ ] Test workflows work in production

### Documentation
- [ ] Document your storage provider choice
- [ ] Document configuration steps taken
- [ ] Create troubleshooting guide for your team
- [ ] Add to your project's README
- [ ] Document API usage for other developers

## Phase 6: Team Handoff

- [ ] Train team on using the editor UI
- [ ] Share API documentation
- [ ] Document custom storage providers (if any)
- [ ] Set up monitoring and logging
- [ ] Create backup strategy for workflows
- [ ] Document disaster recovery procedures
- [ ] Schedule knowledge transfer sessions

## Troubleshooting

If you encounter issues, check:

1. **Build Errors**
   - [ ] Verify all NuGet packages are installed
   - [ ] Check namespace consistency
   - [ ] Verify using statements are correct

2. **Runtime Errors**
   - [ ] Check application logs
   - [ ] Verify appsettings.json syntax
   - [ ] Check file permissions
   - [ ] Verify storage credentials

3. **UI Not Loading**
   - [ ] Check browser console for errors
   - [ ] Verify API_BASE_URL is correct in index.html
   - [ ] Verify UseStaticFiles() in Program.cs
   - [ ] Check that wwwroot/index.html exists

4. **Workflows Not Saving**
   - [ ] Verify storage directory exists and is writable
   - [ ] Check cloud storage credentials
   - [ ] Verify storage provider is correctly selected
   - [ ] Check application logs for errors

## Sign Off

Once all items are complete:

- [ ] Development team lead: _________________ Date: _______
- [ ] Testing team lead: _________________ Date: _______
- [ ] Project manager: _________________ Date: _______

## Next Steps

After successful implementation:

1. Monitor application performance
2. Gather user feedback
3. Plan for enhancements
4. Consider integrating with other systems
5. Document lessons learned

---

**Need help?** Refer back to [SETUP_GUIDE.md](./SETUP_GUIDE.md) or open a GitHub issue.
