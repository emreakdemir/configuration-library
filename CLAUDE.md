# configuration-library — working notes

Open-source .NET library, modernisation pending. Read ROADMAP.md first; work items are GitHub issues under the "Modernise", "1.0" and "Later" milestones. Take them in issue order; the artifact cleanup goes first because every later diff is unreadable otherwise.

## Ground rules
- Each issue: own branch, small PR, build + tests pass, merge in the same session.
- .NET SDK on this machine: `/usr/local/share/dotnet/dotnet` (10.0.x), not on PATH. Only the .NET 10 runtime is installed; net8.0 targets compile here but their tests run in CI.
- Follow HeroKit (`~/Development/HeroKit`) for csproj metadata, warnings-as-errors, CI and the NuGet Trusted Publishing release workflow.
- Decisions (2026-10-28): MIT license, package name HotConfig, storage stays MySQL, Vue 2 UI is rewritten as a single page after the admin API.
- Redis for integration tests via Testcontainers; no shared Redis in tests.
- Library-specific facts stay in this repo (here or docs); nothing goes to the root workspace memory.

## Verify
- `dotnet build -c Release` with zero warnings on both targets
- `dotnet test`
