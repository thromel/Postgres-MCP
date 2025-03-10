using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostgresMcp.Models;
using PostgresMcp.Services;
using System.Net;

namespace PostgresMcp.Controllers;

/// <summary>
/// Controller for handling MCP protocol requests
/// </summary>
[ApiController]
[Route("[controller]")]
public class McpController : ControllerBase
{
    private readonly ILogger<McpController> _logger;
    private readonly IPostgresService _postgresService;

    public McpController(ILogger<McpController> logger, IPostgresService postgresService)
    {
        _logger = logger;
        _postgresService = postgresService;
    }

    /// <summary>
    /// Handles MCP requests
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(McpResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(McpErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(McpErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HandleMcpRequest([FromBody] JObject requestObject)
    {
        try
        {
            _logger.LogInformation("Received MCP request: {Request}", requestObject.ToString());

            // Extract the method from the request
            var method = requestObject["method"]?.ToString();
            var id = requestObject["id"]?.ToString() ?? Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(method))
            {
                _logger.LogWarning("Missing method in MCP request");
                return BadRequest(CreateErrorResponse(id, -32600, "Invalid request: Missing method"));
            }

            // Handle different methods
            return method switch
            {
                "executeQuery" => await HandleExecuteQueryAsync(id, requestObject),
                _ => BadRequest(CreateErrorResponse(id, -32601, $"Method not found: {method}"))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MCP request");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                CreateErrorResponse("", -32603, "Internal error processing request", ex.Message));
        }
    }

    /// <summary>
    /// Handles executeQuery method
    /// </summary>
    private async Task<IActionResult> HandleExecuteQueryAsync(string id, JObject requestObject)
    {
        try
        {
            // Parse parameters
            var paramsObj = requestObject["params"];
            if (paramsObj == null)
            {
                return BadRequest(CreateErrorResponse(id, -32602, "Invalid params: Missing params object"));
            }

            var query = paramsObj["query"]?.ToString();
            var connectionString = paramsObj["connectionString"]?.ToString();

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(CreateErrorResponse(id, -32602, "Invalid params: Missing query"));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return BadRequest(CreateErrorResponse(id, -32602, "Invalid params: Missing connectionString"));
            }

            // Execute the query
            try
            {
                var result = await _postgresService.ExecuteReadOnlyQueryAsync(query, connectionString);
                return Ok(CreateResultResponse(id, result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(CreateErrorResponse(id, -32602, "Invalid params: " + ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    CreateErrorResponse(id, -32000, "Error executing query", ex.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling executeQuery method");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                CreateErrorResponse(id, -32603, "Internal error handling executeQuery", ex.Message));
        }
    }

    /// <summary>
    /// Creates a result response with the given data
    /// </summary>
    private McpResultResponse CreateResultResponse(string id, object result)
    {
        return new McpResultResponse
        {
            Id = id,
            Result = result
        };
    }

    /// <summary>
    /// Creates an error response with the given details
    /// </summary>
    private McpErrorResponse CreateErrorResponse(string id, int code, string message, object? data = null)
    {
        return new McpErrorResponse
        {
            Id = id,
            Error = new McpError
            {
                Code = code,
                Message = message,
                Data = data
            }
        };
    }
} 