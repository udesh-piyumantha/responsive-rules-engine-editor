using RulesEngineEditor.Models;
using RulesEngineEditor.Services.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RulesEngineEditor.Controllers
{
    /// <summary>
    /// REST API endpoints for Rules Engine workflow management
    /// Supports multiple storage providers (JSON File, AWS S3, Azure Blob)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly StorageProviderFactory _storageFactory;
        private readonly ILogger<RulesController> _logger;

        public RulesController(
            StorageProviderFactory storageFactory,
            ILogger<RulesController> logger)
        {
            _storageFactory = storageFactory;
            _logger = logger;
        }

        /// <summary>
        /// List all available workflows
        /// GET: api/rules/workflows
        /// </summary>
        [HttpGet("workflows")]
        public async Task<ActionResult<WorkflowResponse>> ListWorkflows([FromQuery] string provider = null)
        {
            try
            {
                var storageProvider = provider != null
                    ? _storageFactory.CreateProvider(provider)
                    : _storageFactory.CreateProvider();

                var workflows = await storageProvider.ListWorkflowsAsync();

                return Ok(new WorkflowResponse
                {
                    Success = true,
                    Message = $"Retrieved {workflows.Count} workflows from {storageProvider.GetProviderName()}",
                    Data = workflows
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing workflows: {ex.Message}");
                return StatusCode(500, new WorkflowResponse
                {
                    Success = false,
                    Message = "Error retrieving workflows",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Get a specific workflow by name
        /// GET: api/rules/workflows/{name}
        /// </summary>
        [HttpGet("workflows/{name}")]
        public async Task<ActionResult<WorkflowResponse>> GetWorkflow(string name, [FromQuery] string provider = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new WorkflowResponse
                    {
                        Success = false,
                        Message = "Workflow name is required"
                    });
                }

                var storageProvider = provider != null
                    ? _storageFactory.CreateProvider(provider)
                    : _storageFactory.CreateProvider();

                var workflow = await storageProvider.GetWorkflowAsync(name);

                return Ok(new WorkflowResponse
                {
                    Success = true,
                    Message = $"Retrieved workflow '{name}'",
                    Data = workflow
                });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new WorkflowResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving workflow '{name}': {ex.Message}");
                return StatusCode(500, new WorkflowResponse
                {
                    Success = false,
                    Message = "Error retrieving workflow",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Create or update a workflow
        /// POST: api/rules/workflows
        /// </summary>
        [HttpPost("workflows")]
        public async Task<ActionResult<WorkflowResponse>> SaveWorkflow(
            [FromBody] CreateWorkflowRequest request,
            [FromQuery] string provider = null)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new WorkflowResponse
                    {
                        Success = false,
                        Message = "Workflow name is required"
                    });
                }

                var storageProvider = provider != null
                    ? _storageFactory.CreateProvider(provider)
                    : _storageFactory.CreateProvider();

                var workflow = new WorkflowDefinition
                {
                    Name = request.Name,
                    Description = request.Description,
                    Rules = request.Rules ?? new List<RuleDefinition>()
                };

                await storageProvider.SaveWorkflowAsync(workflow);

                _logger.LogInformation($"Saved workflow: {request.Name}");

                return Ok(new WorkflowResponse
                {
                    Success = true,
                    Message = $"Workflow '{request.Name}' saved successfully",
                    Data = workflow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving workflow: {ex.Message}");
                return StatusCode(500, new WorkflowResponse
                {
                    Success = false,
                    Message = "Error saving workflow",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Delete a workflow
        /// DELETE: api/rules/workflows/{name}
        /// </summary>
        [HttpDelete("workflows/{name}")]
        public async Task<ActionResult<WorkflowResponse>> DeleteWorkflow(string name, [FromQuery] string provider = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new WorkflowResponse
                    {
                        Success = false,
                        Message = "Workflow name is required"
                    });
                }

                var storageProvider = provider != null
                    ? _storageFactory.CreateProvider(provider)
                    : _storageFactory.CreateProvider();

                await storageProvider.DeleteWorkflowAsync(name);

                _logger.LogInformation($"Deleted workflow: {name}");

                return Ok(new WorkflowResponse
                {
                    Success = true,
                    Message = $"Workflow '{name}' deleted successfully"
                });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new WorkflowResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting workflow '{name}': {ex.Message}");
                return StatusCode(500, new WorkflowResponse
                {
                    Success = false,
                    Message = "Error deleting workflow",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Check if a workflow exists
        /// HEAD: api/rules/workflows/{name}
        /// </summary>
        [HttpHead("workflows/{name}")]
        public async Task<ActionResult> WorkflowExists(string name, [FromQuery] string provider = null)
        {
            try
            {
                var storageProvider = provider != null
                    ? _storageFactory.CreateProvider(provider)
                    : _storageFactory.CreateProvider();

                var exists = await storageProvider.WorkflowExistsAsync(name);

                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking workflow existence: {ex.Message}");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get available storage providers
        /// GET: api/rules/providers
        /// </summary>
        [HttpGet("providers")]
        public ActionResult<object> GetAvailableProviders()
        {
            return Ok(new
            {
                providers = new[] 
                { 
                    new { name = "jsonfile", label = "JSON File (Local)", configured = true },
                    new { name = "s3", label = "AWS S3", configured = false },
                    new { name = "azureblob", label = "Azure Blob Storage", configured = false }
                }
            });
        }
    }
}
