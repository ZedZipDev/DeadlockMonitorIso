Here is your **final, clean, GitHub‑ready `HowTo.md`** — written exactly for your project, with every step reproducible from scratch.

No fluff. No ambiguity. No missing steps.  
This is the definitive guide for future‑you.

---

# **HowTo: Build & Run the DeadlockMonitorIso Azure Function (Isolated Worker, .NET 10)**

This document describes **exactly** how to recreate the Azure Function project from scratch, including required installations, NuGet packages, configuration, logging, and GitHub setup.

---

## **1. Install Required Tools**

### **1.1 Install .NET 10 SDK**
Download from:  
`https://dotnet.microsoft.com/en-us/download/dotnet/10.0` [(dotnet.microsoft.com in Bing)](https://www.bing.com/search?q="https%3A%2F%2Fdotnet.microsoft.com%2Fen-us%2Fdownload%2Fdotnet%2F10.0")

Verify:

```
dotnet --version
```

---

### **1.2 Install Azure Functions Core Tools (funx64.msi)**  
Download the Windows installer:

```
funx64.msi
```

From:  
`https://learn.microsoft.com/azure/azure-functions/functions-run-local` [(learn.microsoft.com in Bing)](https://www.bing.com/search?q="https%3A%2F%2Flearn.microsoft.com%2Fazure%2Fazure-functions%2Ffunctions-run-local")

Verify:

```
func --version
```

---

### **1.3 Install Azurite (local Azure Storage emulator)**

Install via npm:

```
npm install -g azurite
```

Run:

```
azurite
```

It should start listening on ports 10000/10001/10002.

---

## **2. Create the Azure Function Project**

### **2.1 Create folder**

```
mkdir DeadlockMonitorIso
cd DeadlockMonitorIso
```

### **2.2 Create the isolated worker project**

```
func init . --worker-runtime dotnet-isolated
```

This generates:

- `Program.cs`
- `local.settings.json`
- `DeadlockMonitorIso.csproj`

---

## **3. Add the TimerTrigger Function**

```
func new --name DeadlockTimer --template "Timer trigger"
```

This creates `DeadlockTimer.cs`.

---

## **4. Add Required NuGet Packages**

### **4.1 Modern SQL Client**

```
dotnet add package Microsoft.Data.SqlClient
```

### **4.2 Console Logging (required for isolated worker)**

```
dotnet add package Microsoft.Extensions.Logging.Console
```

---

## **5. Configure Program.cs**

Replace contents with:

```csharp
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

// Enable console logging so LogInformation appears in func start
builder.Logging.AddConsole();

builder.ConfigureFunctionsWebApplication();

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .UseAzureMonitorExporter();

builder.Build().Run();
```

---

## **6. Implement the Timer Function**

`DeadlockTimer.cs`:

```csharp
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
```

---

## **7. Configure local.settings.json**

Add your SQL connection string and logging level:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SqlConnectionString": "Server=VMUX20\\SQL2025;Database=AdventureWorks2025;Persist Security Info=True;User ID=sa;Password=Oleg_a604;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Application Name=AZFunc;Command Timeout=0",
    "Logging:Console:LogLevel:Default": "Information"
  }
}
```

---

## **8. Run the Function Locally**

### **8.1 Start Azurite**

```
azurite
```

### **8.2 Start the Azure Function**

```
func start --verbose
```

Expected output every minute:

```
**** Timer fired, connecting to SQL...
**** SQL responded: <timestamp>
```

---

## **9. GitHub Setup**

### **9.1 Create the GitHub repo manually**

1. Go to [https://github.com/new](https://github.com/new)  
2. Name: **DeadlockMonitorIso**  
3. Do NOT add README, .gitignore, or license  
4. Create repository

---

### **9.2 Initialize Git locally**

```
git init
git add .
git commit -m "Initial commit - Azure Function isolated worker"
```

### **9.3 Add remote**

```
git branch -M master
git remote add origin https://github.com/<your-account>/DeadlockMonitorIso.git
git push -u origin master
```

---

### **9.4 Fix GitHub branches (if needed)**

If GitHub created an empty `main` branch:

1. Go to **Settings → Branches**  
2. Change default branch to **master**  
3. Delete the empty `main` branch from the **Branches** page  

---

## **10. Project is Ready**

You now have:

- A working .NET 10 isolated worker Azure Function  
- TimerTrigger firing every minute  
- SQL connectivity  
- Console logging  
- Azurite storage emulator  
- Clean GitHub repository  

This is the exact reproducible setup.
