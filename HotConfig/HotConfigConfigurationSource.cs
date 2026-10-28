using System;
using HotConfig.Business.Base;
using Microsoft.Extensions.Configuration;

namespace HotConfig
{
    /// <summary>Configuration source for <see cref="HotConfigConfigurationProvider"/>.</summary>
    public class HotConfigConfigurationSource : IConfigurationSource
    {
        public HotConfigOptions Options { get; }

        // Test seam: replaces the MySQL-backed business layer.
        internal IConfigurationBusiness Business { get; set; }

        public HotConfigConfigurationSource(HotConfigOptions options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new HotConfigConfigurationProvider(this);
    }
}
