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

                    var data = new
                    {
                        Pages = pages,
                        Components = components
                    };

                    var json = JsonSerializer.Serialize(data);
                    var sendBuffer = Encoding.UTF8.GetBytes(json);

                    await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
                if (webSocket.State != WebSocketState.Closed)
                {
                    webSocket.Dispose();
                }
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