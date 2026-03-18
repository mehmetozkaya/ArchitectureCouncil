using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ArchitectureCouncil.Web.ApiClients;

public class ArchitectureCouncilApiClient(HttpClient httpClient)
{
    public async IAsyncEnumerable<string> DebateArchitectureStreamingAsync(
        string context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/debate")
        {
            Content = JsonContent.Create(new { Context = context })
        };

        var response = await httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await foreach (var chunk in JsonSerializer.DeserializeAsyncEnumerable<string>(stream, cancellationToken: cancellationToken))
        {
            if (chunk is not null)
            {
                yield return chunk;
            }
        }
    }
}