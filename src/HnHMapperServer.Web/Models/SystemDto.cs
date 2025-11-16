namespace HnHMapperServer.Web.Models;

public class DatabaseStatsDto
{
    public int Users { get; set; }
    public int Sessions { get; set; }
    public int Grids { get; set; }
    public int Markers { get; set; }
    public int Tiles { get; set; }
    public int Maps { get; set; }
    public int Tokens { get; set; }
    public int Config { get; set; }
}

public class BackupResultDto
{
    public string BackupPath { get; set; } = string.Empty;
}

public class SystemOperationResultDto
{
    public string Message { get; set; } = string.Empty;
}
