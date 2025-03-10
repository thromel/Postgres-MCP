#!/bin/bash

# PostgreSQL MCP server installation script for macOS/Linux

# Default connection string
CONNECTION_STRING=${1:-"Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YourPasswordHere;SslMode=Allow;"}

# Create config directory if it doesn't exist
CONFIG_DIR="$HOME/.postgres-mcp"
mkdir -p "$CONFIG_DIR"
echo "Created config directory: $CONFIG_DIR"

# Save connection string to config file
CONFIG_PATH="$CONFIG_DIR/config.json"
echo "{\"defaultConnectionString\": \"$CONNECTION_STRING\"}" > "$CONFIG_PATH"
echo "Saved connection string to $CONFIG_PATH"

# Build the application
echo "Building PostgreSQL MCP server..."
dotnet build

# Create startup script
STARTUP_SCRIPT="$CONFIG_DIR/start-postgres-mcp.sh"
cat << EOF > "$STARTUP_SCRIPT"
#!/bin/bash
export DOTNET_ENVIRONMENT="Development"
cd "$(pwd)"
dotnet run
EOF

chmod +x "$STARTUP_SCRIPT"
echo "Created startup script: $STARTUP_SCRIPT"

# Create registration file for Claude Desktop
CLAUDE_CONFIG_DIR="$HOME/.claude/mcp"
mkdir -p "$CLAUDE_CONFIG_DIR"
echo "Created Claude MCP directory: $CLAUDE_CONFIG_DIR"

REGISTRATION_PATH="$CLAUDE_CONFIG_DIR/postgres-mcp.json"
cat << EOF > "$REGISTRATION_PATH"
{
  "name": "postgres-mcp",
  "displayName": "PostgreSQL MCP",
  "description": "MCP server for PostgreSQL databases",
  "version": "1.0.0",
  "start": "$STARTUP_SCRIPT",
  "tools": [
    {
      "name": "executeQuery",
      "description": "Execute a read-only SQL query against a PostgreSQL database",
      "parameters": {
        "query": {
          "type": "string",
          "description": "The SQL query to execute (SELECT statements only)"
        },
        "connectionString": {
          "type": "string",
          "description": "The PostgreSQL connection string"
        }
      }
    }
  ]
}
EOF

echo "Created MCP registration file: $REGISTRATION_PATH"

echo -e "\nInstallation complete! Please restart Claude Desktop to use the PostgreSQL MCP server."
echo "You can now use the following commands with Claude:"
echo "- 'Execute a SQL query on my PostgreSQL database: SELECT * FROM my_table'"
echo "- 'Show me the schema of the table my_table'"
echo "- 'List all tables in my database'" 