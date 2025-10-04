using Microsoft.EntityFrameworkCore;
using RateWatch.UserService.Application.Messaging;
using RateWatch.UserService.Application.Services;
using RateWatch.UserService.Domain.Interfaces;
using RateWatch.UserService.Infrastructure.Data;
using RateWatch.UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UserContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHostedService<UserRegistrationConsumer>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
