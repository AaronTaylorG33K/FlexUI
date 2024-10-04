using Microsoft.OpenApi.Models;
using FlexUIService = FlexUI.Services.FlexUI;
using FlexUI.Services;
using FlexUI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on a specific port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Replace 5000 with your desired port
});

builder.Services.AddSingleton<FlexUIService>();
builder.Services.AddSingleton<WebSocketService>(); // Register WebSocketService
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlexUI API", Version = "v1" });
    c.DocumentFilter<WebSocketDocumentFilter>();
});

var app = builder.Build();

// swagger for docs
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlexUI Sample Application");
});

app.UseWebSockets();

// websocket endpoint
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        var webSocketService = context.RequestServices.GetRequiredService<WebSocketService>();
        await webSocketService.HandleWebSocketAsync(context);
    }
    else
    {
        await next();
    }
});

app.Run();