using HnHMapperServer.Infrastructure.Data;
using HnHMapperServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HnHMapperServer.Services.Services;

/// <summary>
/// Service for managing tenant storage quotas with atomic updates
/// </summary>
public class StorageQuotaService : IStorageQuotaService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<StorageQuotaService> _logger;

    public StorageQuotaService(ApplicationDbContext db, ILogger<StorageQuotaService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a tenant has enough quota for an upload
    /// </summary>
    public async Task<bool> CheckQuotaAsync(string tenantId, double sizeMB)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            _logger.LogWarning("CheckQuota: Tenant {TenantId} not found", tenantId);
            return false;
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("CheckQuota: Tenant {TenantId} is not active", tenantId);
            return false;
        }

        var wouldExceed = tenant.CurrentStorageMB + sizeMB > tenant.StorageQuotaMB;

        if (wouldExceed)
        {
            _logger.LogWarning(
                "CheckQuota: Tenant {TenantId} over quota. Current: {Current}MB, Quota: {Quota}MB, Upload: {Upload}MB",
                tenantId, tenant.CurrentStorageMB, tenant.StorageQuotaMB, sizeMB);
        }

        return !wouldExceed;
    }

    /// <summary>
    /// Increments tenant storage usage (atomic operation)
    /// </summary>
    public async Task IncrementStorageUsageAsync(string tenantId, double sizeMB)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        tenant.CurrentStorageMB += sizeMB;

        // Prevent negative storage (should never happen, but safety check)
        tenant.CurrentStorageMB = Math.Max(0, tenant.CurrentStorageMB);

        await _db.SaveChangesAsync();

        _logger.LogDebug(
            "IncrementStorage: Tenant {TenantId} usage incremented by {SizeMB}MB. New total: {Total}MB",
            tenantId, sizeMB, tenant.CurrentStorageMB);
    }

    /// <summary>
    /// Decrements tenant storage usage (atomic operation)
    /// </summary>
    public async Task DecrementStorageUsageAsync(string tenantId, double sizeMB)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        tenant.CurrentStorageMB -= sizeMB;

        // Prevent negative storage
        tenant.CurrentStorageMB = Math.Max(0, tenant.CurrentStorageMB);

        await _db.SaveChangesAsync();

        _logger.LogDebug(
            "DecrementStorage: Tenant {TenantId} usage decremented by {SizeMB}MB. New total: {Total}MB",
            tenantId, sizeMB, tenant.CurrentStorageMB);
    }

    /// <summary>
    /// Gets the current storage usage for a tenant
    /// </summary>
    public async Task<double> GetCurrentUsageAsync(string tenantId)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        return tenant?.CurrentStorageMB ?? 0;
    }

    /// <summary>
    /// Gets the storage quota limit for a tenant
    /// </summary>
    public async Task<double> GetQuotaLimitAsync(string tenantId)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        return tenant?.StorageQuotaMB ?? 0;
    }

    /// <summary>
    /// Recalculates storage usage from filesystem and updates database
    /// </summary>
    public async Task<double> RecalculateStorageUsageAsync(string tenantId, string gridStorage)
    {
        var tenant = await _db.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        // Calculate from filesystem
        var tenantDir = Path.Combine(gridStorage, "tenants", tenantId);
        var totalSizeBytes = CalculateDirectorySize(tenantDir);
        var totalSizeMB = totalSizeBytes / 1024.0 / 1024.0;

        // Count files
        var fileCount = CountFiles(tenantDir);

        _logger.LogInformation(
            "RecalculateStorage: Tenant {TenantId} - Files: {FileCount}, Size: {SizeMB:F2}MB",
            tenantId, fileCount, totalSizeMB);

        // Update database
        var oldUsage = tenant.CurrentStorageMB;
        tenant.CurrentStorageMB = totalSizeMB;
        await _db.SaveChangesAsync();

        // Write .storage.json file
        await WriteStorageMetadataAsync(tenantDir, tenantId, totalSizeBytes, totalSizeMB, fileCount);

        // Log discrepancy if significant
        var diffMB = Math.Abs(oldUsage - totalSizeMB);
        if (diffMB > 1.0)
        {
            _logger.LogWarning(
                "RecalculateStorage: Tenant {TenantId} discrepancy detected. " +
                "DB: {OldMB:F2}MB, Filesystem: {NewMB:F2}MB, Diff: {DiffMB:F2}MB",
                tenantId, oldUsage, totalSizeMB, diffMB);
        }

        return totalSizeMB;
    }

    /// <summary>
    /// Calculates total size of a directory recursively
    /// </summary>
    private static long CalculateDirectorySize(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            return 0;
        }

        var directoryInfo = new DirectoryInfo(dirPath);

        try
        {
            // Get size of all files in this directory
            var totalSize = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                .Sum(file => file.Length);

            return totalSize;
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we can't access
            return 0;
        }
    }

    /// <summary>
    /// Counts files in a directory recursively
    /// </summary>
    private static int CountFiles(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            return 0;
        }

        var directoryInfo = new DirectoryInfo(dirPath);

        try
        {
            return directoryInfo.GetFiles("*", SearchOption.AllDirectories).Length;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }
    }

    /// <summary>
    /// Writes .storage.json metadata file
    /// </summary>
    private async Task WriteStorageMetadataAsync(
        string tenantDir,
        string tenantId,
        long totalSizeBytes,
        double totalSizeMB,
        int fileCount)
    {
        var metadata = new
        {
            tenantId = tenantId,
            calculatedAt = DateTime.UtcNow.ToString("O"),
            totalSizeBytes = totalSizeBytes,
            totalSizeMB = Math.Round(totalSizeMB, 2),
            fileCount = fileCount
        };

        try
        {
            Directory.CreateDirectory(tenantDir);
            var metadataPath = Path.Combine(tenantDir, ".storage.json");
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(metadataPath, json);

            _logger.LogDebug(
                "WriteStorageMetadata: Written metadata for tenant {TenantId} to {Path}",
                tenantId, metadataPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WriteStorageMetadata: Failed to write metadata for tenant {TenantId}", tenantId);
        }
    }
}
