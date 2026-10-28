# Roadmap

Decision (2026-10-15): keep configuration-library alive as an open-source .NET library, modernised on the HeroKit pattern (multi-target, CI, NuGet Trusted Publishing). Not a product. Work starts after İZAN; until then this file and the GitHub issues are the plan.

## What it is

A dynamic configuration source for .NET services: values live in a store (today MySQL), are cached in Redis and refreshed on an interval, and are read through `ConfigurationReader.GetValue<T>(key)`. A small admin API and a single-page admin UI manage the records. Written in 2019 as a case study ("CodeSide"); 2 stars, last code change 2023 (dependabot only).

## Where it stands

- .NET Core 2.2 everywhere (end of life December 2019). AutoMapper 8, Autofac 4, Dapper 1.60, ServiceStack.Redis (license-limited), Newtonsoft.
- 221 build artifacts (`bin/`, `obj/`, `.vs/`) committed.
- One MSTest file; no CI; no LICENSE.
- Vue 2 UI (end of life December 2023) with three open dependabot PRs.

## Milestone 1 — Modernise

Goal: builds and tests on .NET 8 + .NET 10 with zero warnings, no committed artifacts, modern dependencies.

- [ ] Remove committed `bin/`, `obj/`, `.vs/`, `.DS_Store`; fix `.gitignore`
- [ ] Target `net8.0;net10.0`; replace ServiceStack.Redis with StackExchange.Redis; Autofac → Microsoft.Extensions.DependencyInjection; Newtonsoft → System.Text.Json; drop AutoMapper
- [x] Reshape the library as a `Microsoft.Extensions.Configuration` provider: `builder.Configuration.AddHotConfig(...)`, periodic refresh, `IOptionsMonitor<T>` change notifications. Keep `ConfigurationReader` as a thin compatibility wrapper
- [x] Tests: MSTest → xUnit; Redis integration tests with Testcontainers; `Build & Test` workflow
- [x] Admin API on minimal APIs; storage stays MySQL (decided 2026-10-28)
- [x] Vue 2 UI: rewrite as a single page served by the admin API, after the API (decided 2026-10-28). One static `index.html` (plain HTML, CSS, JS; no build step) in `HotConfig.Api/wwwroot`; `codeside.ui` is deleted. Close the dependabot PRs when this merges

## Milestone 2 — 1.0 on NuGet

- [x] Rename to **HotConfig** (package id and namespaces; "CodeSide" was the case-study name; decided 2026-10-28)
- [x] LICENSE: MIT (decided 2026-10-28)
- [ ] NuGet packaging: README in package, icon, SourceLink, snupkg; Trusted Publishing policy + release workflow (copy from HeroKit)
- [x] README EN + TR: quickstart, refresh semantics, comparison with Azure App Configuration and Consul KV

## Milestone 3 — Later

- [ ] Second store: PostgreSQL with LISTEN/NOTIFY instead of polling
- [ ] Sample app and a refresh-latency benchmark

## Positioning

Azure App Configuration is paid and Azure-bound; Consul KV needs a Consul cluster; `Microsoft.Extensions.Configuration` has no built-in "shared, hot-reloadable, cross-service" source. A Redis-backed provider with a tiny admin API fills that gap for small teams. That is the pitch; keep the scope that small.
