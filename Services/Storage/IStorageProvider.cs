using RulesEngineEditor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RulesEngineEditor.Services.Storage
{
    /// <summary>
    /// Abstraction for workflow storage providers
    /// Implementations support JSON File, AWS S3, Azure Blob Storage, etc.
    /// </summary>
    public interface IStorageProvider
    {
        /// <summary>
        /// List all available workflows
        /// </summary>
        Task<List<WorkflowMetadata>> ListWorkflowsAsync();

        /// <summary>
        /// Get a specific workflow by name
        /// </summary>
        Task<WorkflowDefinition> GetWorkflowAsync(string name);

        /// <summary>
        /// Save or update a workflow
        /// </summary>
        Task<bool> SaveWorkflowAsync(WorkflowDefinition workflow);

        /// <summary>
        /// Delete a workflow
        /// </summary>
        Task<bool> DeleteWorkflowAsync(string name);

        /// <summary>
        /// Check if a workflow exists
        /// </summary>
        Task<bool> WorkflowExistsAsync(string name);

        /// <summary>
        /// Get provider name for identification
        /// </summary>
        string GetProviderName();
    }
}
