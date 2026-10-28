# HotConfig

[![Build & Test](https://github.com/emreakdemir/configuration-library/actions/workflows/build-tests.yml/badge.svg)](https://github.com/emreakdemir/configuration-library/actions/workflows/build-tests.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/HotConfig.svg)](https://www.nuget.org/packages/HotConfig)

Shared, hot-reloadable configuration for .NET services, stored in Redis.

HotConfig is a `Microsoft.Extensions.Configuration` provider. Each service reads its records from Redis and polls for changes. When a value changes, `IOptionsMonitor<T>` gets the new value within one refresh interval, with no restart. A small admin API and a single admin page manage the records in MySQL and write every change to Redis.

[Türkçe](README.TR.md)

## Install

```bash
dotnet add package HotConfig
```

The package targets .NET 8 and .NET 10.

## Quickstart

1. Add the provider and bind a section:

```csharp
using HotConfig;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddHotConfig(o =>
{
    o.ApplicationName = "orders";
    o.Redis = "localhost:6379";
    o.RefreshInterval = TimeSpan.FromSeconds(10);
});

builder.Services.Configure<FeatureOptions>(builder.Configuration.GetSection("Features"));

var app = builder.Build();
app.MapGet("/features", (IOptionsMonitor<FeatureOptions> features) => features.CurrentValue);
app.Run();

public class FeatureOptions
{
    public string Banner { get; set; } = "";
    public int MaxItems { get; set; }
}
```

2. Add the records `Features:Banner` and `Features:MaxItems` for the application `orders`. Use the admin API or the admin page.

3. Change a value. `/features` returns the new value after the next refresh.

A complete sample is in [samples/HotConfig.Sample](samples/HotConfig.Sample).

## Options

| Option | Required | Default | Meaning |
|---|---|---|---|
| `ApplicationName` | yes | | The application whose records are read. It is also the Redis hash key. |
| `Redis` | yes | | StackExchange.Redis connection string. |
| `RefreshInterval` | no | 30 seconds | How often the provider reads Redis again. |
| `MySql` | no | | Legacy mode. If set, the provider copies the application's active records from MySQL to Redis before each read. |

## Refresh semantics

- The first load reads all active records of the application. If Redis cannot be read, the first load throws.
- After that, a timer reads Redis on every `RefreshInterval`. If the data changed, the provider raises the reload token. `IOptionsMonitor<T>` and `IConfiguration` then return the new values.
- If a refresh fails, the provider keeps the last good data and tries again on the next interval.
- Inactive records are not visible. A record that becomes inactive or is deleted disappears at the next refresh.
- A record name can contain `:` to form sections, for example `Features:Banner`. Keys are case-insensitive.

## Admin API

The admin API is in `HotConfig.Api`. It runs as one container.

| Method | Path | Action |
|---|---|---|
| `GET` | `/api/configurations` | List records. Filters: `applicationName`, `name` (contains), `isActive`. |
| `GET` | `/api/configurations/{id}` | Get one record. |
| `POST` | `/api/configurations` | Create a record. |
| `PUT` | `/api/configurations/{id}` | Update a record. |
| `DELETE` | `/api/configurations/{id}` | Delete a record. |
| `POST` | `/api/configurations/{id}/toggle` | Make a record active or inactive. |
| `POST` | `/api/applications/{applicationName}/sync` | Write the application's active records from MySQL to Redis again. |
| `GET` | `/health` | Health check. No API key. |

- Every `/api` request needs the `X-Api-Key` header. The API does not start without a key.
- `Type` is one of `String`, `Int32`, `Int64`, `Boolean`, `Double`, `Decimal` and `DateTime`. The API also accepts `string`, `int`, `long`, `bool`, `double` and `decimal`. The value must parse as the type.
- Each write goes to MySQL first, then to Redis. If the Redis write fails, the API returns 500. Call `sync` to repair Redis.
- OpenAPI is at `/openapi/v1.json` in the Development environment.

Run it:

```bash
mysql -u root -p hotconfig < db/schema.sql
docker build -t hotconfig-api .
docker run -p 8080:8080 \
  -e HotConfig__ApiKey=change-me \
  -e ConnectionStrings__MySql="Server=host.docker.internal;Database=hotconfig;User ID=hotconfig;Password=..." \
  -e ConnectionStrings__Redis=host.docker.internal:6379 \
  hotconfig-api
```

## Admin page

Open `http://localhost:8080/`. Enter the API key. You can then list, filter, create, edit, delete and toggle records, and sync an application. The page is one static HTML file with no build step.

## Compared with other options

| | HotConfig | Azure App Configuration | Consul KV |
|---|---|---|---|
| Cost | Free, MIT | Limited free tier, then paid per store and request | Free (open source) |
| Runs on | Any Redis + MySQL | Azure only | A Consul cluster |
| .NET integration | `IConfiguration` provider | `IConfiguration` provider | Community packages |
| Change delivery | Polling | Polling or push (Event Grid) | Blocking queries |
| Admin UI | Single page included | Azure portal | Consul UI |

HotConfig is for small teams that already run Redis and want shared, hot-reloadable settings without a cloud service or a new cluster.

## Legacy API

`ConfigurationReader` from the earlier versions still works but is obsolete:

```csharp
var reader = new ConfigurationReader("orders", connectionStrings, refreshInterval: 10_000);
var rate = reader.GetValue<decimal>("Rate");
```

It returns the default value when the record's `Type` does not match `T`. Use `AddHotConfig` in new code.

## Build and test

```bash
dotnet build HotConfig.sln -c Release
dotnet test HotConfig.sln -c Release
```

The tests start Redis and MySQL with Testcontainers, so Docker must be running.

## License

[MIT](LICENSE)
