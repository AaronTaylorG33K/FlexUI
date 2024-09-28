using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;

public class Page
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
}

namespace FlexUI.Services
{
    public class FlexUI
    {
        private static readonly ConcurrentDictionary<WebSocket, WebSocket> _clients = new ConcurrentDictionary<WebSocket, WebSocket>();
        private readonly string _connectionString;

        public FlexUI(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Page>> GetPages()
        {
            var pages = new List<Page>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT id, title, slug, content FROM pages", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var page = new Page
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Slug = reader.GetString(2),
                                Content = reader.GetString(3)
                            };
                            pages.Add(page);
                        }
                    }
                }
            }

            return pages;
        }

        public async Task<IEnumerable<Component>> GetComponents()
        {
            var components = new List<Component>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT id, page_id, name, settings, ordinal FROM components", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var component = new Component
                            {
                                Id = reader.GetInt32(0),
                                PageId = reader.GetInt32(1),
                                Name = reader.GetString(2),
                                Settings = reader.GetString(3),
                                Ordinal = reader.GetInt32(4)
                            };
                            components.Add(component);
                        }
                    }
                }
            }

            return components;
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

                    var pages = await GetPages();
                    var components = await GetComponents();

                    var responseData = new
                    {
                        data = new
                        {
                            pages = pages,
                            components = components
                        }
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

    public class Component
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public string Name { get; set; }
        public string Settings { get; set; }
        public int Ordinal { get; set; }
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