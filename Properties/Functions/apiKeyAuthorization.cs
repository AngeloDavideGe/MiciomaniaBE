using Microsoft.Extensions.Primitives;

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAndAuthorizationValidation(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Request.Headers;
            IConfiguration? config = app.ApplicationServices.GetRequiredService<IConfiguration>();

            if (!headers.TryGetValue("apikey", out StringValues apiKey) || apiKey != config["ApiKey"])
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key non valida o mancante");
                return;
            }

            if (!headers.TryGetValue("Authorization", out StringValues authHeader) || authHeader != $"Bearer {config["ApiKey"]}")
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authorization non valida o mancante");
                return;
            }

            await next();
        });
    }
}
