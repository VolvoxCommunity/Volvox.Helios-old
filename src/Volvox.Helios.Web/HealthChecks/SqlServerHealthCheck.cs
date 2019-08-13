using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Volvox.Helios.Web.HealthChecks
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        SqlConnection _connection;

        public SqlServerHealthCheck(SqlConnection connection)
        {
            _connection = connection;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthCheckContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                _connection.Open();
            }
            catch (SqlException)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy());
            }
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
