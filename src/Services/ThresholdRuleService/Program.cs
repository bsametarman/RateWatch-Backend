using RateWatch.ThresholdRuleService;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config["Redis:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("...");

            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
