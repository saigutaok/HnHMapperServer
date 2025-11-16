var builder = DistributedApplication.CreateBuilder(args);

// Configure SHARED database storage path (absolute path)
var solutionDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var gridStorage = System.IO.Path.Combine(solutionDir, "map");

// Ensure the directory exists
if (!System.IO.Directory.Exists(gridStorage))
{
    System.IO.Directory.CreateDirectory(gridStorage);
}

Console.WriteLine($"Shared database storage: {gridStorage}");

// Add the API backend with database configuration
var api = builder.AddProject<Projects.HnHMapperServer_Api>("api")
    .WithEnvironment("GridStorage", gridStorage);

// Add the Web frontend with SAME database configuration
builder.AddProject<Projects.HnHMapperServer_Web>("web")
    .WithEnvironment("GridStorage", gridStorage)
    .WithReference(api);

builder.Build().Run();
