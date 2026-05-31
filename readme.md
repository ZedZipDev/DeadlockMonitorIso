## License
This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

# DeadlockMonitorIso  
Azure Function (.NET 10, Isolated Worker) for scheduled SQL Server monitoring

## Overview
DeadlockMonitorIso is an Azure Function running in the **.NET Isolated Worker model**.  
It executes on a **TimerTrigger** (every 1 minute) and connects to SQL Server using the modern  
`Microsoft.Data.SqlClient` provider.

The function is designed as the foundation for a future **deadlock collector**, which will execute a stored procedure that captures and logs SQL Server deadlock graphs.

This repository contains:
- A working isolated worker Azure Function
- TimerTrigger firing every minute
- SQL connectivity using `Microsoft.Data.SqlClient`
- Console logging enabled for local development
- Azurite support for local Azure Storage emulation
- A clean GitHub setup with `.gitignore`

---

## Features
- ✔ .NET 10 isolated worker runtime  
- ✔ TimerTrigger (`0 */1 * * * *`)  
- ✔ SQL Server connectivity  
- ✔ Console logging (via `Microsoft.Extensions.Logging.Console`)  
- ✔ OpenTelemetry + Azure Monitor exporter  
- ✔ Local development with Azurite  
- ✔ Ready for extension into a deadlock monitoring system  

---

## Requirements

### Install:
- **.NET 10 SDK**  
- **Azure Functions Core Tools** (`funx64.msi`)  
- **Azurite** (via npm)  
- **SQL Server** (local or remote)

### NuGet packages:
- `Microsoft.Data.SqlClient`  
- `Microsoft.Extensions.Logging.Console`  

---

## Local Development

### 1. Start Azurite
```bash
azurite

------------
+-------------------------------------------------------------+
|                     DeadlockMonitorIso                      |
|                 Azure Function (Isolated)                   |
+-------------------------------------------------------------+
                | TimerTrigger (every 1 min)
                v
+-------------------------------------------------------------+
|                    DeadlockTimer Function                   |
|  - Logs start                                               |
|  - Opens SQL connection (Microsoft.Data.SqlClient)          |
|  - Executes SQL command / stored procedure                  |
|  - Logs result                                              |
+-------------------------------------------------------------+
                |
                v
+-------------------------------------------------------------+
|                        SQL Server                           |
|  - AdventureWorks2025 (example)                             |
|  - Future: Deadlock collector stored procedure              |
+-------------------------------------------------------------+

                Logging Pipeline
                ----------------
                Console Logger  --->  Visible in func start
                OpenTelemetry   --->  Azure Monitor exporter


--------
DeadlockMonitorIso/
│
├── DeadlockMonitorIso.csproj
├── Program.cs
├── DeadlockTimer.cs
├── local.settings.json
├── .gitignore
├── README.md
└── HowTo.md

