# PowerShell script to install the PostgreSQL MCP server for Claude Desktop

param (
    [string]$ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YourPasswordHere;SslMode=Allow;"
)

# Create config directory if it doesn't exist
$configDir = Join-Path $env:USERPROFILE ".postgres-mcp"
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
    Write-Host "Created config directory: $configDir"
}

# Save connection string to config file
$configPath = Join-Path $configDir "config.json"
$config = @{
    defaultConnectionString = $ConnectionString
} | ConvertTo-Json
Set-Content -Path $configPath -Value $config
Write-Host "Saved connection string to $configPath"

# Build the application
Write-Host "Building PostgreSQL MCP server..."
dotnet build

# Create startup script
$startupScript = @"
`$env:DOTNET_ENVIRONMENT = 'Development'
Set-Location -Path '$PWD'
dotnet run
"@

$startupPath = Join-Path $configDir "start-postgres-mcp.ps1"
Set-Content -Path $startupPath -Value $startupScript
Write-Host "Created startup script: $startupPath"

# Create registration file for Claude Desktop
$claudeConfigDir = Join-Path $env:LOCALAPPDATA "Claude\mcp"
if (-not (Test-Path $claudeConfigDir)) {
    New-Item -ItemType Directory -Path $claudeConfigDir -Force | Out-Null
    Write-Host "Created Claude MCP directory: $claudeConfigDir"
}

$registration = @{
    name = "postgres-mcp"
    displayName = "PostgreSQL MCP"
    description = "MCP server for PostgreSQL databases"
    version = "1.0.0"
    start = "powershell.exe -ExecutionPolicy Bypass -File $startupPath"
    tools = @(
        @{
            name = "executeQuery"
            description = "Execute a read-only SQL query against a PostgreSQL database"
            parameters = @{
                query = @{
                    type = "string"
                    description = "The SQL query to execute (SELECT statements only)"
                }
                connectionString = @{
                    type = "string"
                    description = "The PostgreSQL connection string"
                }
            }
        }
    )
} | ConvertTo-Json -Depth 10

$registrationPath = Join-Path $claudeConfigDir "postgres-mcp.json"
Set-Content -Path $registrationPath -Value $registration
Write-Host "Created MCP registration file: $registrationPath"

Write-Host "`nInstallation complete! Please restart Claude Desktop to use the PostgreSQL MCP server."
Write-Host "You can now use the following commands with Claude:"
Write-Host "- 'Execute a SQL query on my PostgreSQL database: SELECT * FROM my_table'"
Write-Host "- 'Show me the schema of the table my_table'"
Write-Host "- 'List all tables in my database'" 