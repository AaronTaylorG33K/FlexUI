using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;

public class Page
{
    public int id { get; set; }
    public string title { get; set; }
    public string slug { get; set; }
    public string content { get; set; }
    public List<Component> components { get; set; }
}

public class Component
{
    public int id { get; set; }
    public string name { get; set; }
    public string settings { get; set; } // Assuming settings is stored as JSON string
    public int ordinal { get; set; }
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

        public async Task<object> GetAllData()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(@"
                    SELECT jsonb_build_object(
                        'pages', jsonb_agg(
                            jsonb_build_object(
                                'id', p.id,
                                'title', p.title,
                                'slug', p.slug,
                                'content', p.content,
                                'components', (
                                    SELECT jsonb_agg(
                                        jsonb_build_object(
                                            'component_id', c.id,
                                            'id', pc.id,
                                            'name', c.name,
                                            'settings', c.settings,
                                            'ordinal', pc.ordinal
                                        )
                                    )
                                    FROM page_components pc
                                    JOIN components c ON pc.component_id = c.id
                                    WHERE pc.page_id = p.id
                                )
                            )
                        )
                    ) AS data
                    FROM pages p;
                ", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    return JsonSerializer.Deserialize<object>(result.ToString());
                }
            }
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

                    var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    Console.WriteLine("Received message: " + message); // Log the received message

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine("Received an empty message.");
                        continue; // Skip processing this message
                    }

                    WebSocketRequest request;
                    try
                    {
                        request = JsonSerializer.Deserialize<WebSocketRequest>(message);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON deserialization error: {ex.Message}");
                        continue; // Skip processing this message
                    }

                    if (request.Mutations != null && request.Mutations.Count > 0)
                    {
                        Console.WriteLine("Processing mutations:");
                        foreach (var mutation in request.Mutations)
                        {
                            Console.WriteLine($"Mutation: Type={mutation.Type}, NewOrdinal={mutation.NewOrdinal}, DestinationPageID={mutation.DestinationPageID}, PageComponentID={mutation.PageComponentID}");
                        }

                        await ProcessMutationsAsync(request.Mutations);

                        // Fetch the updated data
                        var data = await GetAllData();
                        var responseData = new { data = data };
                        var responseMessage = JsonSerializer.Serialize(responseData);

                        Console.WriteLine("Sending updated response: " + responseMessage); // Log the response message
                        await BroadcastMessageAsync(responseMessage);
                    }
                    else
                    {
                        Console.WriteLine("Did not find any mutations, sending whole dataset");

                        // Fetch the data
                        var data = await GetAllData();
                        var responseData = new { data = data };
                        var responseMessage = JsonSerializer.Serialize(responseData);

                        Console.WriteLine("Sending response: " + responseMessage); // Log the response message
                        await BroadcastMessageAsync(responseMessage);
                    }
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

        private async Task ProcessMutationsAsync(List<Mutation> mutations)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    foreach (var mutation in mutations)
                    {
                        using (var command = new NpgsqlCommand())
                        {
                            command.Connection = connection;
                            command.Transaction = transaction;

                            if (mutation.Type == "ordinalUpdate")
                            {
                                command.CommandText = @"
                                    UPDATE page_components
                                    SET 
                                        ordinal = @newOrdinal
                                    WHERE id = @pageComponentID;
                                ";
                                command.Parameters.AddWithValue("@newOrdinal", mutation.NewOrdinal);
                                command.Parameters.AddWithValue("@pageComponentID", mutation.PageComponentID);

                                Console.WriteLine($"Executing ordinal update: UPDATE page_components SET ordinal = {mutation.NewOrdinal} WHERE id = {mutation.PageComponentID} - {mutation.ComponentName}");
                            }
                            else if (mutation.Type == "pageMove")
                            {
                                command.CommandText = @"
                                    UPDATE page_components
                                    SET 
                                        page_id = @destinationPageID
                                    WHERE id = @pageComponentID;
                                ";
                                command.Parameters.AddWithValue("@destinationPageID", mutation.DestinationPageID);
                                command.Parameters.AddWithValue("@pageComponentID", mutation.PageComponentID);

                                Console.WriteLine($"Executing page move: UPDATE page_components SET page_id = {mutation.DestinationPageID} WHERE id = {mutation.PageComponentID} - {mutation.ComponentName}");
                            }
                            else
                            {
                                Console.WriteLine($"Unknown mutation type: {mutation.Type}");
                                continue; // Skip unknown mutation types
                            }

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
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

    public class WebSocketRequest
    {
        [JsonPropertyName("mutations")]
        public List<Mutation> Mutations { get; set; }
    }

    public class Mutation
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("newOrdinal")]
        public int NewOrdinal { get; set; }

        [JsonPropertyName("destinationPageID")]
        public int DestinationPageID { get; set; }

        [JsonPropertyName("pageComponentID")]
        public int PageComponentID { get; set; }

        [JsonPropertyName("componentName")]
        public string ComponentName { get; set; }
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