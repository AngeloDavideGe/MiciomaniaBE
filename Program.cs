using Microsoft.EntityFrameworkCore;
using Miciomania.Data.ApplicationDbContext;
using Miciomania.Views.DropboxSettings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<DropboxSettings>(builder.Configuration.GetSection("Dropbox"));
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithSecurity(); // Metodo custom per configurare Swagger con sicurezza

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
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

app.UseCors();
app.UseApiKeyAndAuthorizationValidation(); // Metodo custom per validare API Key e Authorization
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
