using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace RateWatch.NotificationService
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly ILogger<WebSocketConnectionManager> _logger;

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public async Task AddSocket(string userId, WebSocket socket, CancellationToken stoppingToken)
        {
            if (_sockets.TryAdd(userId, socket))
            {
                _logger.LogInformation($"WebSocket connection added for user with id: {userId}");
            }

            var buffer = new byte[4096];

            try
            {
                while(socket.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
                {
                    var recieveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), stoppingToken);
                    if (recieveResult.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"WebSocket for user {userId} closed via token cancellation.");
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                _logger.LogWarning($"WebSocket connection for user {userId} closed prematurely.");
            }
            finally
            {
                if (_sockets.TryRemove(userId, out _))
                {
                    _logger.LogInformation($"WebSocket connection removed for user: {userId}");
                }
            }
        }

        public async Task SendMessageToUserAsync(string userId, string message)
        {
            if(_sockets.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);

                _logger.LogInformation($"Message sent to user with id: {userId}");
            }
            else
            {
                _logger.LogWarning($"Active WebSocket connection could not found for ser with id {userId}.");
            }
        }
    }
}
