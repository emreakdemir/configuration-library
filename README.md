# CodeSide Configuration Library

> **Durum (Ekim 2026):** .NET Core 2.2 üzerinde yazılmış 2019 tarihli bir çalışma. Modernizasyon planı [ROADMAP.md](ROADMAP.md) ve GitHub issue'larında; iş İZAN'dan sonra başlayacak. Aşağıdaki kurulum notları eski yığın içindir.

## CodeSide.ConfigurationApi

- `/api/configurations` (GET list with `applicationName`, `name`, `isActive` filters, GET/PUT/DELETE `/{id}`, POST, POST `/{id}/toggle`) and `POST /api/applications/{name}/sync`. Every write is mirrored into the Redis hash the provider reads.

- All `/api` calls need the `X-Api-Key` header. Set `HotConfig:ApiKey` (env `HotConfig__ApiKey`), `ConnectionStrings:MySql` and `ConnectionStrings:Redis`; the API refuses to start without a key. `GET /health` needs no key; OpenAPI is at `/openapi/v1.json` in Development.

- `docker build -t hotconfig-api .` builds the single container; create the table with `db/schema.sql`.

## CodeSide.ConfigurationLibrary

- ConfigurationReader class'ı Application, RefreshTime ve ConnectionString bilgileri ile initialize edildiğinde kullanılabilir. Configuration kayıtlarını Redis üzerinde bekletir, belirlenen aralıkta yenileyerek istenilen Type için kullanılmasını sağlar.

## Admin UI

- `HotConfig.Api/wwwroot/index.html` is a single static page (plain HTML, CSS and JavaScript, no build step) served at `/`. Enter the API key, then list, filter, create, edit, delete and toggle records, or sync an application to Redis.

## MySQL Table
```
-- auto-generated definition
CREATE TABLE Configuration
(
    Id              INT AUTO_INCREMENT PRIMARY KEY,
    ApplicationName VARCHAR(100)     NOT NULL,
    Name            VARCHAR(100)     NOT NULL,
    Type            VARCHAR(50)      NOT NULL,
    Value           VARCHAR(500)     NOT NULL,
    IsActive        BIT DEFAULT b'1' NULL
);
```
