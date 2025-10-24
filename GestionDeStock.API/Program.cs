using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Builder;
using GestionDeStock.API.Data;
using GestionDeStock.API.Middleware;
using Microsoft.Extensions.Options;
using NSwag.Generation.Processors.Security; // Add this for NSwag
using NSwag.AspNetCore;
using System.Text.Json.Serialization; // Add this for NSwag

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite("Data Source=GestionDeStockAPP.db"));
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "GestionDeStock API";
    config.Version = "v1";
    config.Description = "API pour un système de gestion de stock.";

});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedFrontEnd", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

        /*policy.WithOrigins("http://localhost:5173/")
            .AllowAnyHeader()
            .AllowAnyMethod();*/
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}
// Configuration pour la production
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddEnvironmentVariables();
}


var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Logging de requêtes
app.Use(async (context, next) =>
{
    logger.LogInformation("Request: {method} {path}", context.Request.Method, context.Request.Path);
    await next.Invoke();
});

app.MapGet("/", () => "Welcome to VoteApp API!").WithOpenApi();


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowedFrontEnd");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Cette méthode crée la base et les tables automatiquement
    context.Database.EnsureCreated();
    
    // Seed des données
    await AppDbContextSeeder.SeedAsync(context);
}

app.Run();

