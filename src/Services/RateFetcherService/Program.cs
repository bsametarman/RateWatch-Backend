using RateWatch.RateFetcherService;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var redisConnectionString = hostContext.Configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
        }

        services.AddHttpClient();

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
