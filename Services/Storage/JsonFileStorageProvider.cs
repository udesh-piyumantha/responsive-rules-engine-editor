using RulesEngineEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RulesEngineEditor.Services.Storage
{
    /// <summary>
    /// Stores workflows as JSON files on local filesystem
    /// Compatible with: D:\RulesStorage\Rules\*.json
    /// </summary>
    public class JsonFileStorageProvider : IStorageProvider
    {
        private readonly string _basePath;
        private readonly ILogger<JsonFileStorageProvider> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonFileStorageProvider(IConfiguration configuration, ILogger<JsonFileStorageProvider> logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            // Get path from configuration or use default
            var storagePath = configuration["Storage:JsonFilePath"] ?? "D:\\RulesStorage";
            _basePath = Path.Combine(storagePath, "Rules");

            // Ensure directory exists
            try
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation($"JSON Storage initialized at: {_basePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize JSON storage directory: {ex.Message}");
                throw;
            }
        }

        public string GetProviderName() => "JSON File Storage";

        public async Task<List<WorkflowMetadata>> ListWorkflowsAsync()
        {
            try
            {
                var workflows = new List<WorkflowMetadata>();
                var directory = new DirectoryInfo(_basePath);

                if (!directory.Exists)
                {
                    _logger.LogWarning($"Workflow directory does not exist: {_basePath}");
                    return workflows;
                }

                foreach (var file in directory.GetFiles("*.json", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var content = await File.ReadAllTextAsync(file.FullName);
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
                        _logger.LogError($"Error reading workflow file {file.Name}: {ex.Message}");
                    }
                }

                return workflows.OrderByDescending(w => w.UpdatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing workflows: {ex.Message}");
                throw;
            }
        }

        public async Task<WorkflowDefinition> GetWorkflowAsync(string name)
        {
            try
            {
                var filePath = GetWorkflowPath(name);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Workflow '{name}' not found");
                }

                var content = await File.ReadAllTextAsync(filePath);
                var workflow = JsonSerializer.Deserialize<WorkflowDefinition>(content, _jsonOptions);

                _logger.LogInformation($"Retrieved workflow: {name}");
                return workflow;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting workflow '{name}': {ex.Message}");
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
                var filePath = GetWorkflowPath(workflow.Name);

                var json = JsonSerializer.Serialize(workflow, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                _logger.LogInformation($"Saved workflow: {workflow.Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving workflow: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteWorkflowAsync(string name)
        {
            try
            {
                var filePath = GetWorkflowPath(name);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Workflow '{name}' not found");
                }

                File.Delete(filePath);
                _logger.LogInformation($"Deleted workflow: {name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting workflow '{name}': {ex.Message}");
                throw;
            }
        }

        public async Task<bool> WorkflowExistsAsync(string name)
        {
            try
            {
                var filePath = GetWorkflowPath(name);
                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking workflow existence: {ex.Message}");
                return false;
            }
        }

        private string GetWorkflowPath(string name)
        {
            // Sanitize filename to prevent directory traversal
            var sanitizedName = Path.GetFileName(name);
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new ArgumentException("Invalid workflow name");
            }

            if (!sanitizedName.EndsWith(".json"))
            {
                sanitizedName += ".json";
            }

            return Path.Combine(_basePath, sanitizedName);
        }
    }
}
