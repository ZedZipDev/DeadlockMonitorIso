1
Install prerequisites
Do first
You need the runtime, tools, and local emulators before creating the project.

Install .NET SDK 10.x from the official .NET download page

Install Azure Functions Core Tools v4 (x64) using funx64.msi

Install Azurite (local Storage emulator), e.g. via npm install -g azurite or VS Code extension

Ensure dotnet, func, and azurite are available in your PATH (dotnet --version, func --version, azurite --help)

2
Create the Azure Functions isolated worker project
Functions app
Scaffold a new .NET isolated Azure Functions app for timer-based execution.

In an empty folder: func init DeadlockMonitorIso --worker-runtime dotnet-isolated --target-framework net10.0

Create a folder, e.g. D:\_GIT\AZ\DeadlockMonitorIso

Run: func init . --worker-runtime dotnet-isolated --target-framework net10.0

Add a timer function: func new --name DeadlockTimer --template "Timer trigger"

Confirm it builds once with dotnet build

3
Add required NuGet packages
Packages
Add SQL client and console logging support for the isolated worker.

Run in project folder:

dotnet add package Microsoft.Data.SqlClient

dotnet add package Microsoft.Extensions.Logging.Console

(Optional but already present) Azure.Monitor.OpenTelemetry.Exporter and Microsoft.Azure.Functions.Worker.Extensions.OpenTelemetry if you use Azure Monitor

4
Configure local.settings.json
Local config
Define storage, runtime, logging, and SQL connection for local development.

Edit local.settings.json in project root

Use this structure:

json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=00000000-0000-0000-0000-000000000000",
    "SqlConnectionString": "Server=VMUX20\\SQL2025;Database=AdventureWorks2025;Persist Security Info=True;User ID=sa;Password=***;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Application Name=AZFunc;Command Timeout=0"
  },
  "Host": {
    "LocalHttpPort": 5004
  }
}
Make sure backslashes in instance name are doubled (\\)

Keep JSON valid: no comments, no stray quotes, no trailing commas

5
Implement Program.cs for isolated worker + logging
Host setup
Wire up the Functions host, OpenTelemetry, Azure Monitor, and console logging.

Replace contents of Program.cs with:

csharp
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

// Console logging so we see our LogInformation output
builder.Logging.AddConsole();

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .UseAzureMonitorExporter();

builder.Build().Run();
6
Implement DeadlockTimer function with SQL test call
Timer + SQL
Create a timer-triggered function that connects to local SQL Server and logs a simple query result.

Edit DeadlockTimer.cs (or create it if needed)

csharp
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
7
Run Azurite and start the function
Run it
Start the local Storage emulator and then run the Functions host with verbose logging.

In one terminal:

azurite

Confirm it listens on 127.0.0.1:10000/10001/10002

In the project folder terminal:

dotnet build

func start --verbose

You should see every minute:

**** Timer fired, connecting to SQL...

**** SQL responded: <timestamp>

8
Add project to GitHub
Checkpoint
Initialize Git, commit the working state, and push to a new GitHub repository.

In project root:

git init

git add .

git commit -m "Initial Azure Functions isolated worker with SQL + Azurite"

Create a new repo on GitHub (e.g. DeadlockMonitorIso)

Add remote: git remote add origin https://github.com/<user>/DeadlockMonitorIso.git

Push: git push -u origin main (or master, depending on your branch)