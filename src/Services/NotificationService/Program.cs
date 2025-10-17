using RateWatch.NotificationService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddHostedService<NotificationWorker>();

var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var connectionManager = app.Services.GetRequiredService<WebSocketConnectionManager>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var userId = context.Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            logger.LogInformation("WebSocket connection request for user: {userId}", userId);
            await connectionManager.AddSocket(userId, webSocket, context.RequestAborted);
        }
        else
        {
            logger.LogWarning("WebSocket request received without a userId.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("userId query parameter is required.");
        }
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();
