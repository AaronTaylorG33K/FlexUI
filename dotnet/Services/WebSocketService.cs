using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlexUI.Models;
using Microsoft.AspNetCore.Http;

namespace FlexUI.Services
{
    public class WebSocketService
    {
        private static readonly ConcurrentDictionary<WebSocket, WebSocket> _clients = new ConcurrentDictionary<WebSocket, WebSocket>();
        private readonly FlexUI _flexUI;

        public WebSocketService(FlexUI flexUI)
        {
            _flexUI = flexUI;
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

                        await _flexUI.ProcessMutationsAsync(request.Mutations);

                        // Fetch the updated data
                        var data = await _flexUI.GetAllData();
                        var responseData = new { data = data };
                        var responseMessage = JsonSerializer.Serialize(responseData);

                        Console.WriteLine("Sending updated response: " + responseMessage); // Log the response message
                        await BroadcastMessageAsync(responseMessage);
                    }
                    else
                    {
                        Console.WriteLine("Did not find any mutations, sending whole dataset");

                        // Fetch the data
                        var data = await _flexUI.GetAllData();
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
}