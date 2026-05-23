using AppTask.Services;
using CacheName;
using Data.ApplicationDbContext;
using Interazioni.Services;
using Manga.Services;
using Microsoft.EntityFrameworkCore;
using Parodie.Services;
using Posts.Services;
using Squadre.Services;
using Utenti.Services;
using Views.DropboxSettings;

public static class BuilderService
{
    public static WebApplicationBuilder GetBuilderService(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, lifetime: ServiceLifetime.Scoped);

        builder.Services.Configure<DropboxSettings>(builder.Configuration.GetSection("Dropbox"));
        builder.Services.AddScoped<AppTaskService>();
        builder.Services.AddScoped<CacheService>();
        builder.Services.AddScoped<SquadreService>();
        builder.Services.AddScoped<PostsService>();
        builder.Services.AddScoped<MangaService>();
        builder.Services.AddScoped<ParodieService>();
        builder.Services.AddScoped<InterazioniService>();
        builder.Services.AddScoped<UtentiService>();
        builder.Services.AddHttpClient();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGenWithSecurity();
        builder.Services.AddMemoryCache();

        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return builder;
    }
}