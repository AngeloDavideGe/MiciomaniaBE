using Microsoft.EntityFrameworkCore;
using Data.ApplicationDbContext;
using Views.DropboxSettings;

WebApplicationOptions options = new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<DropboxSettings>(builder.Configuration.GetSection("Dropbox"));
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithSecurity(); // Metodo custom per configurare Swagger con sicurezza

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

// app.UseWhen(
//     context =>
//         !context.Request.Path.StartsWithSegments("/config.runtime.json"),
//     appBuilder =>
//     {
//         appBuilder.UseApiKeyAndAuthorizationValidation();
//     });

app.UseAuthorization();
app.MapControllers();

app.Run();
