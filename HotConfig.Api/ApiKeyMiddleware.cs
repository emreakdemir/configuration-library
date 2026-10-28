using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HotConfig.Api
{
    /// <summary>Rejects every <c>/api</c> request without a valid key before the body is even bound.</summary>
    public sealed class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ApiKeyVerifier _verifier;

        public ApiKeyMiddleware(RequestDelegate next, ApiKeyVerifier verifier)
        {
            this._next = next;
            this._verifier = verifier;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api") && !this._verifier.IsValid(context.Request))
            {
                return Results.Problem(title: "Unauthorized",
                                       detail: $"Missing or invalid {ApiKeyVerifier.HeaderName} header.",
                                       statusCode: StatusCodes.Status401Unauthorized).ExecuteAsync(context);
            }

            return this._next(context);
        }
    }
}
