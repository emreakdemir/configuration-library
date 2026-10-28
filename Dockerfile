# HotConfig admin API (API + the single-page admin UI served from wwwroot). Build from the repository root:
#   docker build -t hotconfig-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the layer is cached until a project file changes.
COPY Directory.Build.props ./
COPY HotConfig.Api/HotConfig.Api.csproj HotConfig.Api/
COPY HotConfig.Business/HotConfig.Business.csproj HotConfig.Business/
COPY HotConfig.Data/HotConfig.Data.csproj HotConfig.Data/
COPY HotConfig.Data.Dapper/HotConfig.Data.Dapper.csproj HotConfig.Data.Dapper/
COPY HotConfig.Domain/HotConfig.Domain.csproj HotConfig.Domain/
COPY HotConfig.Extensions/HotConfig.Extensions.csproj HotConfig.Extensions/
COPY HotConfig.Redis/HotConfig.Redis.csproj HotConfig.Redis/
RUN dotnet restore HotConfig.Api/HotConfig.Api.csproj

COPY HotConfig.Api/ HotConfig.Api/
COPY HotConfig.Business/ HotConfig.Business/
COPY HotConfig.Data/ HotConfig.Data/
COPY HotConfig.Data.Dapper/ HotConfig.Data.Dapper/
COPY HotConfig.Domain/ HotConfig.Domain/
COPY HotConfig.Extensions/ HotConfig.Extensions/
COPY HotConfig.Redis/ HotConfig.Redis/
RUN dotnet publish HotConfig.Api/HotConfig.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Set ConnectionStrings__MySql, ConnectionStrings__Redis and HotConfig__ApiKey at run time. The API refuses to start without a key.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "HotConfig.Api.dll"]
