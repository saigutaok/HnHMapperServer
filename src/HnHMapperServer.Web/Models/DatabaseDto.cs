namespace HnHMapperServer.Web.Models;

public class DatabaseQueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class DatabaseQueryResult
{
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public int RowCount { get; set; }
    public string? Error { get; set; }
}

public class DatabaseTableInfo
{
    public string Name { get; set; } = string.Empty;
    public int RowCount { get; set; }
}

public class DatabaseSchemaInfo
{
    public List<TableSchema> Tables { get; set; } = new();
}

public class TableSchema
{
    public string TableName { get; set; } = string.Empty;
    public List<ColumnInfo> Columns { get; set; } = new();
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
}
