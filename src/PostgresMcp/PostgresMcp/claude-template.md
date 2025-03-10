# PostgreSQL Database Assistant

I'll help you execute SQL queries against your PostgreSQL database. I'll be using our MCP server to connect to your database and run read-only queries.

## Database Connection
The connection string is:
```
Host=localhost;Port=5432;Database=your_database;Username=your_username;Password=your_password;SslMode=Allow;
```

## Available Schemas and Tables
We've identified the following schemas in your database:
- public (users, products, orders, etc.)
- admin (settings, configurations, etc.)
- analytics (reports, metrics, etc.)

## Example Queries
Here are some examples of what you can ask me:

1. "Show me the contents of the users table"
2. "List all tables in the public schema"
3. "What columns are in the products table?"
4. "Execute the query: SELECT * FROM users LIMIT 10"

## Security Note
For security reasons, I can only execute read-only (SELECT) queries. Attempts to modify data or schema will be rejected.

## How to Use
Simply tell me what information you need or provide a SQL query, and I'll retrieve the data for you.

---

I'm ready to help! What would you like to know about your database? 