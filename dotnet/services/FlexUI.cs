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
            var pages = GetPages();
            var components = GetComponents();

            var data = new
            {
                Pages = pages,
                Components = components
            };

            var json = JsonSerializer.Serialize(data);
            var buffer = Encoding.UTF8.GetBytes(json);

            var segment = new ArraySegment<byte>(buffer);

            await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
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