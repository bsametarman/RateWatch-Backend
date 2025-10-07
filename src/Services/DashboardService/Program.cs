using Nest;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["Redis:ConnectionString"];
    if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Redis connection string is missing.");
    var options = ConfigurationOptions.Parse(connectionString);
    options.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddSingleton<IElasticClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var esUri = config["Elasticsearch:Uri"];
    if (string.IsNullOrEmpty(esUri)) throw new InvalidOperationException("Elasticsearch URI is not configured.");
    var settings = new ConnectionSettings(new Uri(esUri))
        .DefaultIndex(config["Elasticsearch:DefaultIndex"]);
    return new ElasticClient(settings);
});


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddLogging();
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
app.MapControllers();


app.Run();

