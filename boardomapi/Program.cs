using boardomapi.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS — allowed origins are read from the CORS_ORIGINS environment variable
// Set CORS_ORIGINS as a comma-separated list, e.g. "https://example.com,https://api.example.com"
var corsOrigins = builder.Configuration.GetValue<string>("CORS_ORIGINS");
builder.Services.AddCors(options =>
{
    options.AddPolicy("CloudflareTunnel", policy =>
    {
        if (!string.IsNullOrWhiteSpace(corsOrigins))
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            // Fallback: allow any origin in Development
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
  throw new InvalidOperationException("The connection string 'DefaultConnection' is not configured.");
}
var dataSource = DbConfig.CreateDataSource(connectionString!);
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));

var app = builder.Build();

// Apply pending EF Core migrations automatically on startup only in Development
if (app.Environment.IsDevelopment())
{
  using (var scope = app.Services.CreateScope())
  {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
  }
}

// Configure the HTTP request pipeline.

app.UseCors("CloudflareTunnel");

app.UseSwagger();
app.UseSwaggerUI();

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Health check endpoint for Docker/Kubernetes
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.MapControllers();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
      var forecast = Enumerable.Range(1, 5).Select(index =>
              new WeatherForecast
              (
                  DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                  Random.Shared.Next(-20, 55),
                  summaries[Random.Shared.Next(summaries.Length)]
              ))
          .ToArray();
      return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
