using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var AllowCrossOrigins = "_allowCrossOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowCrossOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


var appSettingsToken = builder.Configuration.GetSection("AppSettings:Token").Value;
if (string.IsNullOrEmpty(appSettingsToken))
    throw new Exception("AppSettings Token is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettingsToken)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var ocelotConfigFile = builder.Environment.IsProduction() 
    ? "ocelot.k8s.json" 
    : "ocelot.json";

builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors(AllowCrossOrigins);
app.UseAuthentication();
await app.UseOcelot();

app.Run();
