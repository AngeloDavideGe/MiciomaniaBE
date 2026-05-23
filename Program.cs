WebApplicationOptions options = new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(options);
builder = BuilderService.GetBuilderService(builder);

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