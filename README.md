# VisitCounterAspire

A tiny demo of .NET Aspire orchestration: a web app, an API and a Redis container, wired together by an AppHost.

## Architecture

```
Web (Blazor)  -->  VisitcountAPI (minimal API)  -->  Redis (container)
```

| Project | Role |
|---|---|
| **AppHost** | Orchestrates everything: starts Redis, the API and the web app in order |
| **VisitcountAPI** | Minimal API that stores and reads the counter in Redis |
| **Web** | Blazor app with a button that calls the API |
| **ServiceDefaults** | Shared telemetry, health checks and service discovery setup |

## API

| Method | Route | Description |
|---|---|---|
| `GET` | `/hits` | Returns the current count (0 if not set) |
| `POST` | `/hit` | Increments the count and returns it |

## Aspire features shown

- Container orchestration (Redis started by the AppHost)
- Service discovery (the web app calls `https+http://api`, no hardcoded ports)
- Dependency ordering with `WaitFor`
- The Aspire dashboard: logs, traces and metrics

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or later
- Aspire workload or templates installed
- Docker Desktop (or another container runtime) running

## Run

```bash
dotnet run --project AppHost
```

Then open the Aspire dashboard (the URL is printed in the console) and open the `web` resource.

## Demo flow

1. Start the AppHost and watch the resources start in order in the dashboard.
2. Open the web app and go to the Counter page. It shows the current count from `GET /hits`.
3. Click the button. It calls `POST /hit` and the count goes up.
4. Open the trace view to see the request go Web -> API -> Redis.
5. Refresh the page to show the count survives, because it lives in Redis.

## Orchestration (AppHost)

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("cache");

var api = builder.AddProject<Projects.VisitcountAPI>("api")
                 .WithReference(redis)
                 .WaitFor(redis);

builder.AddProject<Projects.Web>("web")
       .WithReference(api)
       .WaitFor(api)
       .WithExternalHttpEndpoints();

builder.Build().Run();
```

## Notes

- `appsettings.json` in the API holds a fallback `ConnectionStrings:cache` for running it standalone. Under the AppHost, Aspire overrides it with the real Redis address.
- Project paths in the AppHost are relative to the AppHost folder.