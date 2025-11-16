using HnHMapperServer.Core.DTOs;

namespace HnHMapperServer.Services.Interfaces;

/// <summary>
/// Service interface for managing tenants
/// </summary>
public interface ITenantService
{
    Task<TenantDto?> GetTenantAsync(string tenantId);
    Task<List<TenantDto>> GetAllTenantsAsync();
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto);
    Task<TenantDto> UpdateTenantAsync(string tenantId, UpdateTenantDto dto);
    Task DeleteTenantAsync(string tenantId);
    Task<bool> TenantExistsAsync(string tenantId);
}
