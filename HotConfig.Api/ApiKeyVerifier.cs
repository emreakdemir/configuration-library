using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HotConfig.Api
{
    /// <summary>Checks the <c>X-Api-Key</c> header against <c>HotConfig:ApiKey</c> in constant time.</summary>
    public sealed class ApiKeyVerifier
    {
        public const string HeaderName = "X-Api-Key";
        public const string ConfigurationKey = "HotConfig:ApiKey";

        private readonly byte[] _expectedHash;

        public ApiKeyVerifier(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException(
                        $"{ConfigurationKey} is not configured. The admin API refuses to start without an API key; set it in configuration or with the HotConfig__ApiKey environment variable.");

            this._expectedHash = Hash(apiKey);
        }

        public static ApiKeyVerifier FromConfiguration(IConfiguration configuration) => new ApiKeyVerifier(configuration[ConfigurationKey]);

        public bool IsValid(string candidate)
        {
            // Compare fixed-length hashes so neither the content nor the length of the key leaks through timing.
            var candidateHash = Hash(candidate ?? string.Empty);
            return CryptographicOperations.FixedTimeEquals(candidateHash, this._expectedHash);
        }

        public bool IsValid(HttpRequest request) => this.IsValid(request.Headers[HeaderName].ToString());

        private static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
}
