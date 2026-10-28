using System;
using HotConfig.Business.Base;
using Microsoft.Extensions.Configuration;

namespace HotConfig
{
    /// <summary>Configuration source for <see cref="HotConfigConfigurationProvider"/>.</summary>
    public class HotConfigConfigurationSource : IConfigurationSource
    {
        /// <summary>The options this source was created with.</summary>
        public HotConfigOptions Options { get; }

        // Test seam: replaces the MySQL-backed business layer.
        internal IConfigurationBusiness Business { get; set; }

        /// <summary>Creates a source from the given options.</summary>
        /// <param name="options">The HotConfig options.</param>
        public HotConfigConfigurationSource(HotConfigOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new HotConfigConfigurationProvider(this);
    }
}
