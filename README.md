# Postgres-MCP

An MCP (Model Context Protocol) server for PostgreSQL databases that allows Claude and other LLMs to execute read-only SQL queries.

## Overview

This MCP server allows Claude to connect to PostgreSQL databases and safely execute read-only queries. It provides a JSON-RPC 2.0 compatible API that follows the MCP specification.

## Key Features

- Execute read-only SQL queries against PostgreSQL databases
- Return results in a structured JSON format
- Secure validation to prevent data modification (SELECT queries only)
- Easy integration with Claude Desktop

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL database
- Claude Desktop (for using the MCP server)

### Installation

#### Option 1: Using Smithery (Easiest)

[Smithery](https://github.com/smithery-io/smithery) is a package manager for MCP servers that makes installation easy:

```bash
# Install Smithery if you don't have it
npm install -g @smithery/cli

# Install the PostgreSQL MCP server
smithery install postgres-mcp --client claude
```

#### Option 2: Using NPX

You can install the PostgreSQL MCP server directly using NPX:

```bash
npx postgres-mcp "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YourPasswordHere;SslMode=Allow;"
```

Replace the connection string with your PostgreSQL connection details.

#### Option 3: Using the PowerShell Install Script (Windows)

1. Clone this repository
2. Run the install script:

```powershell
cd src/PostgresMcp/PostgresMcp
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

3. Restart Claude Desktop to discover the MCP server

#### Option 4: Using the Shell Script (macOS/Linux)

1. Clone this repository
2. Run the install script:

```bash
cd src/PostgresMcp/PostgresMcp
chmod +x install.sh
./install.sh "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YourPasswordHere;SslMode=Allow;"
```

3. Restart Claude Desktop to discover the MCP server

#### Option 5: Manual Setup

1. Clone this repository
2. Build and run the application:

```bash
cd src/PostgresMcp/PostgresMcp
dotnet build
dotnet run
```

3. Register the MCP server with Claude Desktop manually

## Usage

Once installed, you can use Claude to execute SQL queries:

- "Show me the contents of the users table"
- "List all tables in my PostgreSQL database"
- "What columns are in the customers table?"

## API

The MCP server exposes a single endpoint at `/mcp` that accepts POST requests with JSON-RPC 2.0 formatted requests.

### Example Request

```json
{
  "id": "query-1",
  "jsonrpc": "2.0",
  "method": "executeQuery",
  "params": {
    "query": "SELECT * FROM users LIMIT 10",
    "connectionString": "Host=localhost;Port=5432;Database=mydb;Username=user;Password=YourPasswordHere;"
  }
}
```

## Security

- Only SELECT queries are allowed for security reasons
- Queries are validated before execution to prevent data modification
- Connection strings should be handled securely

## License

This project is licensed under the MIT License - see the LICENSE file for details.
