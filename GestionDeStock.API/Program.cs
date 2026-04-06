using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using GestionDeStock.API.Data;
using GestionDeStock.API.Middleware;
using Microsoft.Extensions.Options;
using NSwag.Generation.Processors.Security; // Add this for NSwag
using NSwag.AspNetCore;
using System.Text.Json.Serialization; // Add this for NSwag
using GestionDeStock.API.Interfaces;
using GestionDeStock.API.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

});
builder.Services.AddEndpointsApiExplorer();

// Database configuration - use MySQL in production, SQLite in development
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Fallback to SQLite for development
    connectionString = "Data Source=GestionDeStockAPP.db";
    builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseSqlite(connectionString));
}
else
{
    // Use MySQL for production
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, 
            ServerVersion.AutoDetect(connectionString)));
}
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
    });
});

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>(); 
builder.Services.AddScoped<IImageService, ImageService>();


var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
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
    await AppDbContextSeeder.SeedAsync(context);
}

await app.RunAsync();

