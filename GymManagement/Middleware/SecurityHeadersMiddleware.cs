namespace GymManagement.Web.Middleware
{
    /// <summary>
    /// Appends security-related HTTP response headers on every response.
    /// Registered after UseRouting so it runs for all matched routes.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["X-XSS-Protection"] = "1; mode=block";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

                // CSP: self-hosted assets + Bootstrap CDN
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
                    "font-src 'self' https://cdn.jsdelivr.net; " +
                    "img-src 'self' data:; " +
                    "connect-src 'self';";

                // Remove server fingerprint
                headers.Remove("Server");
                headers.Remove("X-Powered-By");

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

}
