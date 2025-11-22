using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HnHMapperServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateGlobalConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Consolidate 'prefix' config from all tenants into a single global config
            // Strategy: Take the first non-null prefix value we find, create global entry, delete tenant-specific entries

            migrationBuilder.Sql(@"
                -- Create global prefix config entry using first available prefix value
                INSERT INTO Config (Key, TenantId, Value)
                SELECT 'prefix', '__global__', Value
                FROM Config
                WHERE Key = 'prefix'
                LIMIT 1;

                -- Delete all tenant-specific prefix entries
                DELETE FROM Config WHERE Key = 'prefix' AND TenantId != '__global__';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Delete global prefix and restore tenant-specific ones
            // Note: We can't perfectly restore the old state, but we can create a default entry for each tenant

            migrationBuilder.Sql(@"
                -- Get the global prefix value
                -- Re-create tenant-specific prefix entries for all existing tenants
                INSERT INTO Config (Key, TenantId, Value)
                SELECT 'prefix', t.Id, (SELECT Value FROM Config WHERE Key = 'prefix' AND TenantId = '__global__')
                FROM Tenants t
                WHERE NOT EXISTS (SELECT 1 FROM Config WHERE Key = 'prefix' AND TenantId = t.Id);

                -- Delete global prefix entry
                DELETE FROM Config WHERE Key = 'prefix' AND TenantId = '__global__';
            ");
        }
    }
}
