#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const os = require('os');

console.log('PostgreSQL MCP Server Installer');
console.log('===============================');

// Get connection string from command line
const connectionString = process.argv[2] || '';
if (!connectionString) {
  console.log('No connection string provided. Using default connection string.');
  console.log('To provide a connection string, use:');
  console.log('  npx postgres-mcp "Host=localhost;Port=5432;Database=mydb;Username=user;Password=password;"');
  console.log('');
}

// Determine platform-specific paths
const isWindows = os.platform() === 'win32';
const homedir = os.homedir();

// Create config directory if it doesn't exist
const configDir = path.join(homedir, '.postgres-mcp');
if (!fs.existsSync(configDir)) {
  fs.mkdirSync(configDir, { recursive: true });
  console.log(`Created config directory: ${configDir}`);
}

// Save connection string to config file
const defaultConnectionString = connectionString || 'Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YourPasswordHere;SslMode=Allow;';
const configPath = path.join(configDir, 'config.json');
fs.writeFileSync(configPath, JSON.stringify({ defaultConnectionString }, null, 2));
console.log(`Saved connection string to ${configPath}`);

// Clone repository if installing via NPX
const repoUrl = 'https://github.com/thromel/Postgres-MCP.git';
const installDir = path.join(configDir, 'postgres-mcp');

if (!fs.existsSync(installDir)) {
  console.log(`Cloning repository to ${installDir}...`);
  try {
    execSync(`git clone ${repoUrl} "${installDir}"`, { stdio: 'inherit' });
  } catch (error) {
    console.error('Error cloning repository:', error.message);
    console.log('Continuing with local files...');
  }
}

// Determine source directory
const srcDir = fs.existsSync(path.join(process.cwd(), 'src/PostgresMcp/PostgresMcp')) 
  ? path.join(process.cwd(), 'src/PostgresMcp/PostgresMcp')
  : fs.existsSync(installDir) 
    ? path.join(installDir, 'src/PostgresMcp/PostgresMcp')
    : process.cwd();

// Build the application
console.log('Building PostgreSQL MCP server...');
try {
  execSync(`cd "${srcDir}" && dotnet build`, { stdio: 'inherit' });
} catch (error) {
  console.error('Error building the application:', error.message);
  process.exit(1);
}

// Create startup script
let startupScriptPath;
if (isWindows) {
  startupScriptPath = path.join(configDir, 'start-postgres-mcp.ps1');
  const startupScript = `
$env:DOTNET_ENVIRONMENT = 'Development'
Set-Location -Path '${srcDir}'
dotnet run
`;
  fs.writeFileSync(startupScriptPath, startupScript);
} else {
  startupScriptPath = path.join(configDir, 'start-postgres-mcp.sh');
  const startupScript = `#!/bin/bash
export DOTNET_ENVIRONMENT="Development"
cd "${srcDir}"
dotnet run
`;
  fs.writeFileSync(startupScriptPath, startupScript);
  execSync(`chmod +x "${startupScriptPath}"`);
}
console.log(`Created startup script: ${startupScriptPath}`);

// Create registration file for Claude Desktop
const claudeConfigDir = isWindows
  ? path.join(process.env.LOCALAPPDATA || path.join(homedir, 'AppData/Local'), 'Claude/mcp')
  : path.join(homedir, '.claude/mcp');

if (!fs.existsSync(claudeConfigDir)) {
  fs.mkdirSync(claudeConfigDir, { recursive: true });
  console.log(`Created Claude MCP directory: ${claudeConfigDir}`);
}

const registration = {
  name: 'postgres-mcp',
  displayName: 'PostgreSQL MCP',
  description: 'MCP server for PostgreSQL databases',
  version: '1.0.0',
  start: isWindows 
    ? `powershell.exe -ExecutionPolicy Bypass -File "${startupScriptPath}"`
    : startupScriptPath,
  tools: [
    {
      name: 'executeQuery',
      description: 'Execute a read-only SQL query against a PostgreSQL database',
      parameters: {
        query: {
          type: 'string',
          description: 'The SQL query to execute (SELECT statements only)'
        },
        connectionString: {
          type: 'string',
          description: 'The PostgreSQL connection string'
        }
      }
    }
  ]
};

const registrationPath = path.join(claudeConfigDir, 'postgres-mcp.json');
fs.writeFileSync(registrationPath, JSON.stringify(registration, null, 2));
console.log(`Created MCP registration file: ${registrationPath}`);

console.log('\nInstallation complete! Please restart Claude Desktop to use the PostgreSQL MCP server.');
console.log('You can now use the following commands with Claude:');
console.log('- "Execute a SQL query on my PostgreSQL database: SELECT * FROM my_table"');
console.log('- "Show me the schema of the table my_table"');
console.log('- "List all tables in my database"'); 