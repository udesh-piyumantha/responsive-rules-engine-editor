using RulesEngineEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RulesEngineEditor.Services.Storage
{
    /// <summary>
    /// Stores workflows in Azure Blob Storage
    /// Requires NuGet package: Azure.Storage.Blobs
    /// Configuration: appsettings.json
    /// {
    ///   "Storage": {
    ///     "Azure": {
    ///       "ConnectionString": "DefaultEndpointsProtocol=https;...",
    ///       "ContainerName": "workflows"
    ///     }
    ///   }
    /// }
    /// </summary>
    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobStorageProvider> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AzureBlobStorageProvider(
            IConfiguration configuration,
            ILogger<AzureBlobStorageProvider> logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var connectionString = configuration["Storage:Azure:ConnectionString"]
                ?? throw new ArgumentException("Azure connection string not configured");
            var containerName = configuration["Storage:Azure:ContainerName"] ?? "workflows";

            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure container exists
                _containerClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                _logger.LogInformation($"Azure Blob Storage initialized - Container: {containerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize Azure Blob Storage: {ex.Message}");
                throw;
            }
        }

        public string GetProviderName() => "Azure Blob Storage";

        public async Task<List<WorkflowMetadata>> ListWorkflowsAsync()
        {
            try
            {
                var workflows = new List<WorkflowMetadata>();

                await foreach (var blobItem in _containerClient.GetBlobsAsync())
                {
                    if (!blobItem.Name.EndsWith(".json"))
                        continue;

                    try
                    {
                        var content = await GetBlobContentAsync(blobItem.Name);
                        var workflow = JsonSerializer.Deserialize<WorkflowDefinition>(content, _jsonOptions);

                        workflows.Add(new WorkflowMetadata
                        {
                            Name = workflow.Name,
                            Description = workflow.Description,
                            RuleCount = workflow.Rules?.Count ?? 0,
                            CreatedAt = workflow.CreatedAt,
                            UpdatedAt = workflow.UpdatedAt
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error reading Azure blob {blobItem.Name}: {ex.Message}");
                    }
                }

                return workflows.OrderByDescending(w => w.UpdatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing Azure Blob workflows: {ex.Message}");
                throw;
            }
        }

        public async Task<WorkflowDefinition> GetWorkflowAsync(string name)
        {
            try
            {
                var blobName = GetBlobName(name);
                var content = await GetBlobContentAsync(blobName);
                var workflow = JsonSerializer.Deserialize<WorkflowDefinition>(content, _jsonOptions);

                _logger.LogInformation($"Retrieved Azure Blob workflow: {name}");
                return workflow;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting Azure Blob workflow '{name}': {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveWorkflowAsync(WorkflowDefinition workflow)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(workflow.Name))
                {
                    throw new ArgumentException("Workflow name is required");
                }

                workflow.UpdatedAt = DateTime.UtcNow;
                var blobName = GetBlobName(workflow.Name);
                var json = JsonSerializer.Serialize(workflow, _jsonOptions);

                var blobClient = _containerClient.GetBlobClient(blobName);
                using (var ms = new MemoryStream())
                {
                    using (var writer = new StreamWriter(ms))
                    {
                        await writer.WriteAsync(json);
                        await writer.FlushAsync();
                        ms.Position = 0;

                        await blobClient.UploadAsync(ms, overwrite: true);
                    }
                }

                _logger.LogInformation($"Saved Azure Blob workflow: {workflow.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving Azure Blob workflow: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteWorkflowAsync(string name)
        {
            try
            {
                var blobName = GetBlobName(name);
                var blobClient = _containerClient.GetBlobClient(blobName);

                var deleted = await blobClient.DeleteIfExistsAsync();

                if (deleted)
                {
                    _logger.LogInformation($"Deleted Azure Blob workflow: {name}");
                    return true;
                }
                else
                {
                    throw new FileNotFoundException($"Workflow '{name}' not found in Azure Blob Storage");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting Azure Blob workflow '{name}': {ex.Message}");
                throw;
            }
        }

        public async Task<bool> WorkflowExistsAsync(string name)
        {
            try
            {
                var blobName = GetBlobName(name);
                var blobClient = _containerClient.GetBlobClient(blobName);
                var exists = await blobClient.ExistsAsync();
                return exists.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking Azure Blob workflow existence: {ex.Message}");
                return false;
            }
        }

        private string GetBlobName(string workflowName)
        {
            var sanitizedName = Path.GetFileName(workflowName);
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new ArgumentException("Invalid workflow name");
            }

            if (!sanitizedName.EndsWith(".json"))
            {
                sanitizedName += ".json";
            }

            return sanitizedName;
        }

        private async Task<string> GetBlobContentAsync(string blobName)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);
                var download = await blobClient.DownloadAsync();

                using (var reader = new StreamReader(download.Value.Content))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                throw new FileNotFoundException($"Azure Blob not found: {blobName}");
            }
        }
    }
}
