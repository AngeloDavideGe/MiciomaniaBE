using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using Views.DropboxSettings;
using Squadre.Services;
using AppTask.Services;

WebApplicationOptions options = new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, lifetime: ServiceLifetime.Scoped);

builder.Services.Configure<DropboxSettings>(builder.Configuration.GetSection("Dropbox"));
builder.Services.AddScoped<SquadreService>();
builder.Services.AddScoped<AppTaskService>();
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

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string configSourcePath = Path.Combine(builder.Environment.ContentRootPath, "Configs", "config.json");
string configTargetPath = Path.Combine(builder.Environment.WebRootPath, "config.runtime.json");

if (File.Exists(configSourcePath))
{
    File.Copy(configSourcePath, configTargetPath, overwrite: true);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseApiKeyAndAuthorizationValidation();

app.UseAuthorization();
app.MapControllers();

app.Run();