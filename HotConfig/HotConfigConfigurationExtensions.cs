using System;
using HotConfig;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>Extension methods to add HotConfig to an <see cref="IConfigurationBuilder"/>.</summary>
    public static class HotConfigConfigurationExtensions
    {
        /// <summary>
        /// Adds the HotConfig provider: active records of an application are read from a Redis hash and
        /// refreshed every <see cref="HotConfigOptions.RefreshInterval"/>. Changes raise reload tokens,
        /// so <c>IOptionsMonitor&lt;T&gt;</c> sees them.
        /// </summary>
        public static IConfigurationBuilder AddHotConfig(this IConfigurationBuilder builder, Action<HotConfigOptions> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var options = new HotConfigOptions();
            configure(options);
            return builder.Add(new HotConfigConfigurationSource(options));
        }
    }
}
