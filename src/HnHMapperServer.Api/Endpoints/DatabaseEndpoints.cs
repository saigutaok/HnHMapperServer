using HnHMapperServer.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HnHMapperServer.Api.Endpoints;

public static class DatabaseEndpoints
{
    public static void MapDatabaseEndpoints(this IEndpointRouteBuilder app)
    {
        // Database endpoints - Admin only
        var database = app.MapGroup("/admin/database")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        database.MapGet("/tables", GetTables).DisableAntiforgery();
        database.MapPost("/query", ExecuteQuery).DisableAntiforgery();
        database.MapGet("/table/{tableName}", GetTableData).DisableAntiforgery();
        database.MapGet("/schema", GetSchema).DisableAntiforgery();
    }

    private static async Task<IResult> GetTables(ApplicationDbContext db)
    {
        try
        {
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name,
                       (SELECT COUNT(*) FROM sqlite_master sm WHERE sm.name = m.name AND sm.type = 'table') as row_count
                FROM sqlite_master m
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                  AND name NOT LIKE '__EF%'
                ORDER BY name";

            var tables = new List<object>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString(0);

                    // Get actual row count
                    var countCommand = connection.CreateCommand();
                    countCommand.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
                    var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

                    tables.Add(new { Name = tableName, RowCount = count });
                }
            }

            return Results.Json(tables);
        }
        catch (Exception ex)
        {
            return Results.Json(new List<object>());
        }
    }

    private static async Task<IResult> ExecuteQuery(
        [FromBody] QueryRequest request,
        ApplicationDbContext db)
    {
        try
        {
            // Security: Only allow SELECT queries
            var query = request.Query.Trim();
            if (!query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Json(new
                {
                    Columns = new List<string>(),
                    Rows = new List<Dictionary<string, object?>>(),
                    RowCount = 0,
                    Error = "Only SELECT queries are allowed for security reasons"
                });
            }

            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = query;

            var columns = new List<string>();
            var rows = new List<Dictionary<string, object?>>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                // Get column names
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                // Get rows
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    rows.Add(row);
                }
            }

            return Results.Json(new
            {
                Columns = columns,
                Rows = rows,
                RowCount = rows.Count,
                Error = (string?)null
            });
        }
        catch (Exception ex)
        {
            return Results.Json(new
            {
                Columns = new List<string>(),
                Rows = new List<Dictionary<string, object?>>(),
                RowCount = 0,
                Error = ex.Message
            });
        }
    }

    private static async Task<IResult> GetTableData(
        string tableName,
        ApplicationDbContext db)
    {
        try
        {
            // Validate table name exists
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type='table' AND name=@tableName";
            checkCommand.Parameters.Add(new SqliteParameter("@tableName", tableName));

            var exists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
            if (!exists)
            {
                return Results.Json(new
                {
                    Columns = new List<string>(),
                    Rows = new List<Dictionary<string, object?>>(),
                    RowCount = 0,
                    Error = "Table not found"
                });
            }

            // Get table data
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM \"{tableName}\" LIMIT 1000";

            var columns = new List<string>();
            var rows = new List<Dictionary<string, object?>>();

            using (var reader = await command.ExecuteReaderAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    rows.Add(row);
                }
            }

            return Results.Json(new
            {
                Columns = columns,
                Rows = rows,
                RowCount = rows.Count,
                Error = (string?)null
            });
        }
        catch (Exception ex)
        {
            return Results.Json(new
            {
                Columns = new List<string>(),
                Rows = new List<Dictionary<string, object?>>(),
                RowCount = 0,
                Error = ex.Message
            });
        }
    }

    private static async Task<IResult> GetSchema(ApplicationDbContext db)
    {
        try
        {
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name
                FROM sqlite_master
                WHERE type='table'
                  AND name NOT LIKE 'sqlite_%'
                  AND name NOT LIKE '__EF%'
                ORDER BY name";

            var tables = new List<object>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString(0);

                    // Get columns for this table
                    var colCommand = connection.CreateCommand();
                    colCommand.CommandText = $"PRAGMA table_info(\"{tableName}\")";

                    var columns = new List<object>();
                    using (var colReader = await colCommand.ExecuteReaderAsync())
                    {
                        while (await colReader.ReadAsync())
                        {
                            columns.Add(new
                            {
                                Name = colReader.GetString(1),  // name
                                Type = colReader.GetString(2),  // type
                                IsNullable = colReader.GetInt32(3) == 0,  // notnull
                                IsPrimaryKey = colReader.GetInt32(5) > 0  // pk
                            });
                        }
                    }

                    tables.Add(new
                    {
                        TableName = tableName,
                        Columns = columns
                    });
                }
            }

            return Results.Json(new { Tables = tables });
        }
        catch (Exception ex)
        {
            return Results.Json(new { Tables = new List<object>() });
        }
    }

    private class QueryRequest
    {
        public string Query { get; set; } = string.Empty;
    }
}
