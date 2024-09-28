using Microsoft.OpenApi.Models;
using FlexUIService = FlexUI.Services.FlexUI;
using FlexUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FlexUIService>();
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
        var flexUI = context.RequestServices.GetRequiredService<FlexUIService>();
        await flexUI.HandleWebSocketAsync(context);
    }
    else
    {
        await next();
    }
});

// couple classic http endpoints for REST API testing
app.MapGet("/pages", (FlexUIService flexUI) => flexUI.GetPages())
   .WithName("GetPages")
   .WithTags("Pages");

app.MapGet("/components", (FlexUIService flexUI) => flexUI.GetComponents())
   .WithName("GetComponents")
   .WithTags("Components");

var port = builder.Environment.IsDevelopment() ? "5555" : "80";
app.Run($"http://+:{port}");