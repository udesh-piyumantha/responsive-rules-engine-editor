using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RulesEngineEditor.Models
{
    /// <summary>
    /// Represents a single rule in a workflow (compatible with RulesEngine.Models.Rule)
    /// </summary>
    public class RuleDefinition
    {
        [JsonPropertyName("RuleName")]
        public string Name { get; set; }

        [JsonPropertyName("Expression")]
        public string Expression { get; set; }

        [JsonPropertyName("SuccessEvent")]
        public string SuccessEvent { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("RuleExpressionType")]
        public string RuleExpressionType { get; set; } = "LambdaExpression";

        [JsonPropertyName("Enabled")]
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Represents a complete workflow definition compatible with Microsoft Rules Engine
    /// This is the format stored by Inventory.API
    /// </summary>
    public class RulesEngineWorkflow
    {
        [JsonPropertyName("WorkflowName")]
        public string Name { get; set; }

        [JsonPropertyName("Rules")]
        public List<RuleDefinition> Rules { get; set; } = new();

        [JsonPropertyName("GlobalParams")]
        public List<GlobalParam> GlobalParams { get; set; } = new();
    }

    /// <summary>
    /// Global parameter for rules
    /// </summary>
    public class GlobalParam
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Expression")]
        public string Expression { get; set; }
    }

    /// <summary>
    /// Represents our simplified workflow definition for the editor
    /// </summary>
    public class WorkflowDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("rules")]
        public List<RuleDefinition> Rules { get; set; } = new();

        [JsonPropertyName("globalParams")]
        public List<GlobalParam> GlobalParams { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request DTO for creating/updating workflows
    /// </summary>
    public class CreateWorkflowRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("rules")]
        public List<RuleDefinition> Rules { get; set; } = new();

        [JsonPropertyName("globalParams")]
        public List<GlobalParam> GlobalParams { get; set; } = new();
    }

    /// <summary>
    /// Response DTO for workflow operations
    /// </summary>
    public class WorkflowResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Workflow metadata for list operations
    /// </summary>
    public class WorkflowMetadata
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("ruleCount")]
        public int RuleCount { get; set; }

        [JsonPropertyName("globalParamCount")]
        public int GlobalParamCount { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Utility class for converting between formats
    /// </summary>
    public static class WorkflowConverter
    {
        /// <summary>
        /// Convert RulesEngineWorkflow array (Inventory.API format) to WorkflowDefinition
        /// </summary>
        public static WorkflowDefinition ConvertToWorkflowDefinition(RulesEngineWorkflow[] rulesEngineWorkflows)
        {
            if (rulesEngineWorkflows == null || rulesEngineWorkflows.Length == 0)
                return null;

            var firstWorkflow = rulesEngineWorkflows[0];
            return new WorkflowDefinition
            {
                Name = firstWorkflow.Name,
                Description = $"Workflow for {firstWorkflow.Name}",
                Rules = firstWorkflow.Rules ?? new List<RuleDefinition>(),
                GlobalParams = firstWorkflow.GlobalParams ?? new List<GlobalParam>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Convert WorkflowDefinition to RulesEngineWorkflow array (Inventory.API format)
        /// </summary>
        public static RulesEngineWorkflow[] ConvertToRulesEngineWorkflows(WorkflowDefinition workflow)
        {
            return new[]
            {
                new RulesEngineWorkflow
                {
                    Name = workflow.Name,
                    Rules = workflow.Rules,
                    GlobalParams = workflow.GlobalParams
                }
            };
        }

        /// <summary>
        /// Extract metadata from RulesEngineWorkflow array
        /// </summary>
        public static WorkflowMetadata ExtractMetadata(RulesEngineWorkflow[] rulesEngineWorkflows, string fileName)
        {
            if (rulesEngineWorkflows == null || rulesEngineWorkflows.Length == 0)
                return null;

            var firstWorkflow = rulesEngineWorkflows[0];
            return new WorkflowMetadata
            {
                Name = firstWorkflow.Name,
                Description = $"Workflow for {firstWorkflow.Name}",
                RuleCount = firstWorkflow.Rules?.Count ?? 0,
                GlobalParamCount = firstWorkflow.GlobalParams?.Count ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}