using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace DeadlockMonitorIso;

public class DeadlockTimer
{
    private readonly ILogger<DeadlockTimer> _logger;
    private readonly string _connString;

    public DeadlockTimer(ILogger<DeadlockTimer> logger)
    {
        _logger = logger;
        _connString = Environment.GetEnvironmentVariable("SqlConnectionString")
            ?? throw new InvalidOperationException("SqlConnectionString is missing.");
    }

    [Function("DeadlockTimer")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo timer)
    {
        _logger.LogInformation("**** Timer fired, connecting to SQL...");

        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT GETDATE()", conn);
        var result = await cmd.ExecuteScalarAsync();

        _logger.LogInformation($"**** SQL responded: {result}");
    }
}
