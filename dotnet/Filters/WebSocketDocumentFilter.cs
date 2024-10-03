using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FlexUI.Filters
{
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