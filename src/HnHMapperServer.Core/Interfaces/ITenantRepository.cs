using HnHMapperServer.Core.Models;

namespace HnHMapperServer.Core.Interfaces;

/// <summary>
/// Repository interface for tenant operations
/// </summary>
public interface ITenantRepository
{
    Task<TenantEntity?> GetByIdAsync(string id);
    Task<List<TenantEntity>> GetAllAsync();
    Task<TenantEntity> CreateAsync(TenantEntity tenant);
    Task UpdateAsync(TenantEntity tenant);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}
