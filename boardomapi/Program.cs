using boardomapi.Database;
using boardomapi.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    Description = "Enter your JWT token"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      new string[] {}
    }
  });
});

//Adding Auth0 JWT validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
  options.Authority = "https://dev-vht5t15d8lck3cag.us.auth0.com/"; //This does not need to be a secret, since its a public domain.
  string? apiAudience = builder.Configuration["Auth0:Audience"];
  string? clientSecret = builder.Configuration["Auth0:ClientSecret"];

  options.Audience = apiAudience;
  // Auth0 access tokens can contain multiple audiences (e.g. API audience + /userinfo).
  // Accept any of the configured valid audiences.
  options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
  {
    ValidAudiences = new[]
    {
      apiAudience,
      $"{options.Authority.TrimEnd('/')}/userinfo"
    }.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray(),

    //Add the client secret as a symmetric key for JWE decryption
    IssuerSigningKey = new SymmetricSecurityKey(
      System.Text.Encoding.UTF8.GetBytes(clientSecret))
  };
});


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

builder.Services.AddHostedService<SoftDeleteCleanupJob>();

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseCors("CloudflareTunnel");

//Auth0 jwt
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
