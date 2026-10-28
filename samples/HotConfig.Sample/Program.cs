using HotConfig;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var hotConfig = builder.Configuration.GetSection("HotConfig").Get<HotConfigOptions>()!;
builder.Configuration.AddHotConfig(o =>
{
    o.ApplicationName = hotConfig.ApplicationName;
    o.Redis = hotConfig.Redis;
    o.RefreshInterval = hotConfig.RefreshInterval;
});

// Records named "Features:Banner" and "Features:MaxItems" in Redis bind to FeatureOptions.
builder.Services.Configure<FeatureOptions>(builder.Configuration.GetSection("Features"));

var app = builder.Build();
app.MapGet("/features", (IOptionsMonitor<FeatureOptions> features) => features.CurrentValue);
app.Run();

public class FeatureOptions
{
    public string Banner { get; set; } = "";
    public int MaxItems { get; set; }
}
