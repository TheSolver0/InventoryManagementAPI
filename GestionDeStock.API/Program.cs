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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
    
    // Add JWT authentication to Swagger
    config.AddSecurity("Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Entrez votre token JWT"
    });
    
    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor());
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
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Key"] ?? "your-super-secret-key-change-this-in-production-12345";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "GestionDeStockAPI";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "GestionDeStockClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


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

app.MapGet("/", () => "Welcome to GestionDeStock API!").WithOpenApi();

// Add authentication and authorization middleware
app.UseAuthentication();
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

