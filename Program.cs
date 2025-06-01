using Microsoft.EntityFrameworkCore;
using Miciomania.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Miciomania.Views.DropboxSettings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Connessione a PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurazione Dropbox (legge da appsettings.json -> sezione "Dropbox")
builder.Services.Configure<DropboxSettings>(builder.Configuration.GetSection("Dropbox"));

// Aggiungi HttpClient per chiamate esterne (Dropbox API)
builder.Services.AddHttpClient();

// Aggiungi servizi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithSecurity(); // Metodo custom per configurare Swagger con sicurezza

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin() // qualsiasi origine
            .AllowAnyHeader() // permette qualsiasi header, incluso apikey e authorization
            .AllowAnyMethod(); // GET, POST, PUT, DELETE, etc.
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
