using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RulesEngineEditor.Models
{
    /// <summary>
    /// Represents a complete workflow definition compatible with Microsoft Rules Engine
    /// </summary>
    public class WorkflowDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("rules")]
        public List<RuleDefinition> Rules { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Represents a single rule in a workflow
    /// </summary>
    public class RuleDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("expression")]
        public string Expression { get; set; }

        [JsonPropertyName("successEvent")]
        public string SuccessEvent { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("ruleExpressionType")]
        public string RuleExpressionType { get; set; } = "LambdaExpression";

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
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

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
