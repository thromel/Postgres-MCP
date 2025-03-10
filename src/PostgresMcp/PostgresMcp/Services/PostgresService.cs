using Npgsql;
using PostgresMcp.Models;
using System.Data;

namespace PostgresMcp.Services;

/// <summary>
/// Service for interacting with PostgreSQL databases
/// </summary>
public interface IPostgresService
{
    /// <summary>
    /// Executes a read-only SQL query against a PostgreSQL database
    /// </summary>
    /// <param name="query">The SQL query to execute</param>
    /// <param name="connectionString">The connection string to the PostgreSQL database</param>
    /// <returns>A QueryResult containing columns and rows</returns>
    Task<QueryResult> ExecuteReadOnlyQueryAsync(string query, string connectionString);
}

/// <summary>
/// Implementation of the PostgreSQL service
/// </summary>
public class PostgresService : IPostgresService
{
    private readonly ILogger<PostgresService> _logger;

    public PostgresService(ILogger<PostgresService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes a read-only SQL query against a PostgreSQL database
    /// </summary>
    /// <param name="query">The SQL query to execute</param>
    /// <param name="connectionString">The connection string to the PostgreSQL database</param>
    /// <returns>A QueryResult containing columns and rows</returns>
    public async Task<QueryResult> ExecuteReadOnlyQueryAsync(string query, string connectionString)
    {
        var result = new QueryResult();
        
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("Empty query provided");
            return result;
        }
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Empty connection string provided");
            return result;
        }
        
        // Validate that the query is read-only
        if (!IsReadOnlyQuery(query))
        {
            _logger.LogWarning("Non-read-only query attempted: {Query}", query);
            throw new InvalidOperationException("Only SELECT queries are allowed for security reasons");
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            await using var command = new NpgsqlCommand(query, connection);
            command.CommandTimeout = 30; // 30 seconds timeout
            
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult);
            
            // Get column information
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.Columns.Add(new ColumnDefinition
                {
                    Name = reader.GetName(i),
                    Type = reader.GetFieldType(i).Name
                });
            }
            
            // Get row data
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value;
                }
                
                result.Rows.Add(row);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {Query}", query);
            throw;
        }
    }
    
    /// <summary>
    /// Checks if a query is read-only (SELECT only)
    /// </summary>
    private bool IsReadOnlyQuery(string query)
    {
        // Normalize the query for checking
        var normalizedQuery = query.Trim().ToUpperInvariant();
        
        // Check if the query starts with SELECT
        if (!normalizedQuery.StartsWith("SELECT"))
        {
            return false;
        }
        
        // Basic check for disallowed statements
        var disallowedKeywords = new[]
        {
            "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "TRUNCATE", "GRANT", "REVOKE"
        };
        
        // Check if any of the disallowed keywords are present in the query
        foreach (var keyword in disallowedKeywords)
        {
            // Use word boundary check to avoid false positives
            if (normalizedQuery.Contains($" {keyword} ") || normalizedQuery.Contains($";{keyword} "))
            {
                return false;
            }
        }
        
        return true;
    }
} 