using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RateWatch.AlertService.Application.Services;
using RateWatch.AlertService.Domain.Interfaces;
using RateWatch.AlertService.Infrastructure.Data;
using RateWatch.AlertService.Infrastructure.Repositories;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AlertContext>(options =>
    options.UseNpgsql(connectionString));

var appSettingsToken = builder.Configuration.GetSection("AppSettings:Token").Value;
if (string.IsNullOrEmpty(appSettingsToken))
    throw new Exception("AppSettings Token is not configured.");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var redisConnectionString = configuration["Redis:ConnectionString"];

    if (string.IsNullOrEmpty(redisConnectionString))
    {
        throw new InvalidOperationException("Redis connection string is missing.");
    }

    var options = ConfigurationOptions.Parse(redisConnectionString);
    options.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
