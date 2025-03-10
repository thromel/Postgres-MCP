using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PostgresMcp.Models;

/// <summary>
/// Base class for all MCP requests
/// </summary>
public class McpRequest
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("method")]
    public string Method { get; set; } = string.Empty;
}

/// <summary>
/// Base class for all MCP responses
/// </summary>
public class McpResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
}

/// <summary>
/// MCP response with result data
/// </summary>
public class McpResultResponse : McpResponse
{
    [JsonProperty("result")]
    public object Result { get; set; } = new object();
}

/// <summary>
/// MCP response with error data
/// </summary>
public class McpErrorResponse : McpResponse
{
    [JsonProperty("error")]
    public McpError Error { get; set; } = new McpError();
}

/// <summary>
/// Error information for MCP responses
/// </summary>
public class McpError
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public object? Data { get; set; }
}

/// <summary>
/// Represents the parameters for executing a SQL query
/// </summary>
public class ExecuteQueryParams
{
    [JsonProperty("query")]
    public string Query { get; set; } = string.Empty;
    
    [JsonProperty("connectionString")]
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Represents a request to execute a SQL query
/// </summary>
public class ExecuteQueryRequest : McpRequest
{
    [JsonProperty("params")]
    public ExecuteQueryParams Params { get; set; } = new ExecuteQueryParams();
}

/// <summary>
/// Represents a database column definition
/// </summary>
public class ColumnDefinition
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a result set from a SQL query
/// </summary>
public class QueryResult
{
    [JsonProperty("columns")]
    public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
    
    [JsonProperty("rows")]
    public List<Dictionary<string, object?>> Rows { get; set; } = new List<Dictionary<string, object?>>();
} 