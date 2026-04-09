using Microsoft.Extensions.Diagnostics.HealthChecks;
using RVM.ShopEngine.Infrastructure.Data;

namespace RVM.ShopEngine.API.Health;

public class DatabaseHealthCheck(ShopEngineDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await db.Database.CanConnectAsync(ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database unavailable", ex);
        }
    }
}
