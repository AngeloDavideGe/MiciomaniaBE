using Microsoft.EntityFrameworkCore;
using Miciomania.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Connessione a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Aggiungi servizi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithSecurity(); // Metodo custom per configurare Swagger con sicurezza

builder.Services.AddMemoryCache();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .WithHeaders("apikey", "authorization", "content-type")
              .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiKeyAndAuthorizationValidation(); // Metodo custom per validare API Key e Authorization
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.Run();
