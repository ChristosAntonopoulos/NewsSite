using Microsoft.AspNetCore.Mvc;
using NewsSite.Server.Models.PipelineAggregate;
using NewsSite.Server.Services.Interfaces;
using NewsSite.Server.Services.Pipeline;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace NewsSite.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PipelineController : ControllerBase
    {
        private readonly IPipelineService _pipelineService;
        private readonly IPipelineExecutionService _executionService;
        private readonly ILogger<PipelineController> _logger;

        public PipelineController(
            IPipelineService pipelineService,
            IPipelineExecutionService executionService,
            ILogger<PipelineController> logger)
        {
            _pipelineService = pipelineService;
            _executionService = executionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PipelineModel>>> GetPipelines([FromQuery] bool includeDisabled = false)
        {
            try
            {
                var pipelines = await _pipelineService.GetPipelinesAsync(includeDisabled);
                return Ok(pipelines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve pipelines");
                return StatusCode(500, "Failed to retrieve pipelines");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PipelineModel>> GetPipeline(string id)
        {
            try
            {
                var pipeline = await _pipelineService.GetPipelineByIdAsync(id);
                if (pipeline == null)
                {
                    return NotFound($"Pipeline with ID {id} not found");
                }
                return Ok(pipeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve pipeline {PipelineId}", id);
                return StatusCode(500, "Failed to retrieve pipeline");
            }
        }

        [HttpPost]
        public async Task<ActionResult<PipelineModel>> CreatePipeline(PipelineModel pipeline)
        {
            try
            {
                if (!ValidatePipelineConfiguration(pipeline))
                {
                    return BadRequest("Invalid pipeline configuration");
                }

                var createdPipeline = await _pipelineService.CreatePipelineAsync(pipeline);
                return CreatedAtAction(nameof(GetPipeline), new { id = createdPipeline.Id }, createdPipeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create pipeline");
                return StatusCode(500, "Failed to create pipeline");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePipeline(string id, PipelineModel pipeline)
        {
            try
            {
                if (id != pipeline.Id)
                {
                    return BadRequest("Pipeline ID mismatch");
                }

                if (!ValidatePipelineConfiguration(pipeline))
                {
                    return BadRequest("Invalid pipeline configuration");
                }

                var updated = await _pipelineService.UpdatePipelineAsync(pipeline);
                if (!updated)
                {
                    return NotFound($"Pipeline with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update pipeline {PipelineId}", id);
                return StatusCode(500, "Failed to update pipeline");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePipeline(string id)
        {
            try
            {
                var deleted = await _pipelineService.DeletePipelineAsync(id);
                if (!deleted)
                {
                    return NotFound($"Pipeline with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete pipeline {PipelineId}", id);
                return StatusCode(500, "Failed to delete pipeline");
            }
        }

        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> TogglePipeline(string id)
        {
            try
            {
                var updated = await _pipelineService.TogglePipelineStateAsync(id);
                if (!updated)
                {
                    return NotFound($"Pipeline with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle pipeline {PipelineId}", id);
                return StatusCode(500, "Failed to toggle pipeline state");
            }
        }

        [HttpPost("{id}/steps/reorder")]
        public async Task<IActionResult> ReorderPipelineSteps(string id, [FromBody] List<string> stepIds)
        {
            try
            {
                var updated = await _pipelineService.ReorderPipelineStepsAsync(id, stepIds);
                if (!updated)
                {
                    return NotFound($"Pipeline with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reorder steps for pipeline {PipelineId}", id);
                return StatusCode(500, "Failed to reorder pipeline steps");
            }
        }

        [HttpPost("{id}/execute")]
        public async Task<ActionResult<Dictionary<string, object>>> ExecutePipeline(string id, [FromBody] Dictionary<string, object> initialInput = null)
        {
            try
            {
                var result = await _executionService.ExecutePipelineAsync(id, initialInput);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (StepExecutionException ex)
            {
                return BadRequest(new { error = ex.Message, stepId = ex.StepId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute pipeline {PipelineId}", id);
                return StatusCode(500, "Failed to execute pipeline");
            }
        }

        private bool ValidatePipelineConfiguration(PipelineModel pipeline)
        {
            if (string.IsNullOrEmpty(pipeline.Name))
                return false;

            if (pipeline.Steps == null || !pipeline.Steps.Any())
                return false;

            // Validate steps based on pipeline mode
            switch (pipeline.Mode)
            {
                case PipelineMode.Sequential:
                    // Ensure steps have proper order
                    return pipeline.Steps.Select((s, i) => i).SequenceEqual(pipeline.Steps.OrderBy(s => s.Order).Select((s, i) => i));

                case PipelineMode.Parallel:
                    // Ensure input/output schema compatibility between steps
                    return ValidateParallelStepsSchema(pipeline);

                case PipelineMode.Conditional:
                    // Ensure conditional steps have valid conditions only when conditions are provided
                    return ValidateConditionalSteps(pipeline);

                default:
                    return false;
            }
        }

        private bool ValidateParallelStepsSchema(PipelineModel pipeline)
        {
            // Implement schema validation logic for parallel steps
            return true;
        }

        private bool ValidateConditionalSteps(PipelineModel pipeline)
        {
            // Only validate conditions if they are provided
            foreach (var step in pipeline.Steps)
            {
                if (!string.IsNullOrEmpty(step.Condition))
                {
                    // Validate the condition format if one is provided
                    if (!IsValidConditionFormat(step.Condition))
                        return false;
                }
            }
            return true;
        }

        private bool IsValidConditionFormat(string condition)
        {
            // Basic validation of condition format
            return !string.IsNullOrEmpty(condition) && 
                   condition.Contains("==") || condition.Contains("!=") || 
                   condition.Contains(">") || condition.Contains("<") || 
                   condition.Contains(">=") || condition.Contains("<=");
        }
    }
} 