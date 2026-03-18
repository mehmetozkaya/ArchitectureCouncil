using ArchitectureCouncil.ApiService.Services;
using System.Text.Json;

namespace ArchitectureCouncil.ApiService.Endpoints;

public record WriteRequest(string Context);

public static class ArchitectureCouncilEndpoints
{
    public static void MapArchitectureCouncilEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/debate");

        group.MapPost("/", async (WriteRequest request, ArchitectureCouncilService service, HttpResponse response, CancellationToken ct) =>
        {
            response.ContentType = "application/json";

            await response.WriteAsync("[", ct);
            bool first = true;

            await foreach (var chunk in service.DebateStreamingAsync(request.Context, ct))
            {
                if (!first)
                    await response.WriteAsync(",", ct);

                await response.WriteAsync(JsonSerializer.Serialize(chunk), ct);
                await response.Body.FlushAsync(ct);
                first = false;
            }

            await response.WriteAsync("]", ct);
            await response.Body.FlushAsync(ct);
        })
        .WithName("DebateArchitecture");
    }
}
