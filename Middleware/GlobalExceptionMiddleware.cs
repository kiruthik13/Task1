using System.Net;
using System.Text.Json;

namespace HospitalManagement.Web.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next   = next;
            _logger = logger;
            _env    = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Return JSON for API requests, redirect for MVC
            if (context.Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    StatusCode = 500,
                    Message    = "An unexpected error occurred.",
                    Detail     = _env.IsDevelopment() ? exception.Message : null
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            else
            {
                // Redirect to error page for MVC
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    // Extension method for clean registration in Program.cs
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
