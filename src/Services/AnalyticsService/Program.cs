using Nest;
using RateWatch.AnalyticsService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var esUri = hostContext.Configuration["Elasticsearch:Uri"];
        if (string.IsNullOrEmpty(esUri))
            throw new InvalidOperationException("Elasticsearch URI is not configured.");

        var settings = new ConnectionSettings(new Uri(esUri))
            .DefaultIndex("exchange-rates");

        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
