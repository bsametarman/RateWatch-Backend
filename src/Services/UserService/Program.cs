using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RateWatch.UserService.Application.Messaging;
using RateWatch.UserService.Application.Services;
using RateWatch.UserService.Domain.Interfaces;
using RateWatch.UserService.Infrastructure.Data;
using RateWatch.UserService.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UserContext>(options =>
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

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHostedService<UserRegistrationConsumer>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<UserContext>();
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
