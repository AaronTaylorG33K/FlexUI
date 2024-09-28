using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FlexUI.Services
{
    public class FlexUI
    {
        private static readonly ConcurrentDictionary<WebSocket, WebSocket> _clients = new ConcurrentDictionary<WebSocket, WebSocket>();

        public IEnumerable<string> GetPages()
        {
            return new List<string> { "Home", "About", "Contact" };
        }

        public IEnumerable<string> GetComponents()
        {
            return new List<string> { "Header", "Footer", "Sidebar" };
        }

        public async Task HandleWebSocketAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _clients.TryAdd(webSocket, webSocket);
                await SendPagesAndComponentsAsync(webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        private async Task SendPagesAndComponentsAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }


                    var pages = GetPages();
                    var components = GetComponents();

                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

                    var responseData = new
                    {
                        data = new
                        {
                            pages = pages,
                            components = components
                        },
                        message = receivedMessage
                    };

                    var message = JsonSerializer.Serialize(responseData);
                    await BroadcastMessageAsync(message);
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "WebSocket error", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Unexpected error", CancellationToken.None);
                }
            }
            finally
            {
                _clients.TryRemove(webSocket, out _);
                if (webSocket.State != WebSocketState.Closed)
                {
                    webSocket.Dispose();
                }
            }
        }

        private async Task BroadcastMessageAsync(string message)
        {
            var sendBuffer = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            foreach (var client in _clients.Keys)
            {
                if (client.State == WebSocketState.Open)
                {
                    tasks.Add(client.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }
    }

    public class WebSocketDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Paths.Add("/ws", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Summary = "WebSocket endpoint - ws:// protocol",
                        Description = "Endpoint for WebSocket connections. Access via ws:// protocol.",
                        Tags = new List<OpenApiTag> { new OpenApiTag { Name = "WebSocket" } },
                        Responses = new OpenApiResponses
                        {
                            ["101"] = new OpenApiResponse { Description = "Switching Protocols" }
                        }
                    }
                }
            });
        }
    }
}