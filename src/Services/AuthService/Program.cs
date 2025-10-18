using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RateWatch.AuthService.Application.DTOs;
using RateWatch.AuthService.Application.Services;
using RateWatch.AuthService.Application.Validators;
using RateWatch.AuthService.Domain.Interfaces;
using RateWatch.AuthService.Infrastructure.Data;
using RateWatch.AuthService.Infrastructure.Messaging;
using RateWatch.AuthService.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthContext>(options =>
    options.UseNpgsql(connectionString));

var appSettingsToken = builder.Configuration.GetSection("AppSettings:Token").Value;
if (string.IsNullOrEmpty(appSettingsToken))
    throw new Exception("AppSettings Token is not configured.");

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

builder.Services.AddScoped<IValidator<UserForRegisterDto>, UserForRegisterDtoValidator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddSingleton<IMessageProducer, KafkaProducer>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthContext>();
        await dbContext.Database.MigrateAsync();
    }
    app.Logger.LogInformation("Database migrations applied successfully for AuthService.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while applying database migrations for AuthService.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthorization();
app.MapControllers();

app.Run();

