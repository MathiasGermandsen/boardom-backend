using boardomapi.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var dataSource = DbConfig.CreateDataSource(connectionString);
builder.Services.AddSingleton(dataSource);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource));

var app = builder.Build();

// Apply migrations automatically on startup (optional - can remove in production)
using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

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
