using Microsoft.OpenApi.Models;

public static class SwaggerSetup
{
    public static void AddSwaggerGenWithSecurity(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Miciomania API", Version = "v1" });

            // Definizione security per apikey
            c.AddSecurityDefinition("apikey", new OpenApiSecurityScheme
            {
                Name = "apikey",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Inserisci qui la tua API Key"
            });

            // Definizione security per Authorization
            c.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Inserisci qui il token Bearer"
            });

            // Richiedi gli header nelle chiamate
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "apikey" }
                    },
                    new List<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Authorization" }
                    },
                    new List<string>()
                }
            });
        });
    }
}
