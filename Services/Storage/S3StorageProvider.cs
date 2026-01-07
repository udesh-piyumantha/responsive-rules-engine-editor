using RulesEngineEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RulesEngineEditor.Services.Storage
{
    /// <summary>
    /// Stores workflows in AWS S3
    /// Requires AWS SDK NuGet package: AWSSDK.S3
    /// Configuration: appsettings.json
    /// {
    ///   "Storage": {
    ///     "S3": {
    ///       "BucketName": "my-rules-bucket",
    ///       "Prefix": "workflows/"
    ///     }
    ///   }
    /// }
    /// </summary>
    public class S3StorageProvider : IStorageProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _prefix;
        private readonly ILogger<S3StorageProvider> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public S3StorageProvider(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<S3StorageProvider> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            _bucketName = configuration["Storage:S3:BucketName"] ?? throw new ArgumentException("S3 bucket name not configured");
            _prefix = configuration["Storage:S3:Prefix"] ?? "workflows/";

            _logger.LogInformation($"S3 Storage initialized - Bucket: {_bucketName}, Prefix: {_prefix}");
        }

        public string GetProviderName() => "AWS S3 Storage";

        public async Task<List<WorkflowMetadata>> ListWorkflowsAsync()
        {
            try
            {
                var workflows = new List<WorkflowMetadata>();
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = _prefix
                };

                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);

                    if (response.S3Objects != null)
                    {
                        foreach (var obj in response.S3Objects.Where(o => o.Key.EndsWith(".json")))
                        {
                            try
                            {
                                var content = await GetObjectContentAsync(obj.Key);
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
                                _logger.LogError($"Error reading S3 object {obj.Key}: {ex.Message}");
                            }
                        }
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while (response.IsTruncated);

                return workflows.OrderByDescending(w => w.UpdatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing S3 workflows: {ex.Message}");
                throw;
            }
        }

        public async Task<WorkflowDefinition> GetWorkflowAsync(string name)
        {
            try
            {
                var key = GetObjectKey(name);
                var content = await GetObjectContentAsync(key);
                var workflow = JsonSerializer.Deserialize<WorkflowDefinition>(content, _jsonOptions);

                _logger.LogInformation($"Retrieved S3 workflow: {name}");
                return workflow;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting S3 workflow '{name}': {ex.Message}");
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
                var key = GetObjectKey(workflow.Name);
                var json = JsonSerializer.Serialize(workflow, _jsonOptions);

                using (var ms = new MemoryStream())
                {
                    using (var writer = new StreamWriter(ms))
                    {
                        await writer.WriteAsync(json);
                        await writer.FlushAsync();
                        ms.Position = 0;

                        var putRequest = new PutObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = key,
                            InputStream = ms,
                            ContentType = "application/json"
                        };

                        await _s3Client.PutObjectAsync(putRequest);
                    }
                }

                _logger.LogInformation($"Saved S3 workflow: {workflow.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving S3 workflow: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteWorkflowAsync(string name)
        {
            try
            {
                var key = GetObjectKey(name);
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation($"Deleted S3 workflow: {name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting S3 workflow '{name}': {ex.Message}");
                throw;
            }
        }

        public async Task<bool> WorkflowExistsAsync(string name)
        {
            try
            {
                var key = GetObjectKey(name);
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking S3 workflow existence: {ex.Message}");
                return false;
            }
        }

        private string GetObjectKey(string workflowName)
        {
            var sanitizedName = Path.GetFileName(workflowName);
            if (!sanitizedName.EndsWith(".json"))
            {
                sanitizedName += ".json";
            }
            return $"{_prefix.TrimEnd('/')}/{sanitizedName}";
        }

        private async Task<string> GetObjectContentAsync(string key)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException($"S3 object not found: {key}");
            }
        }
    }
}
