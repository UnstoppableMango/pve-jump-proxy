using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace ProxmoxJumpProxy.Host;

public static class Isos
{
    [FunctionName("negotiate")]
    public static SignalRConnectionInfo Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
        [SignalRConnectionInfo(HubName = "serverlessSample")] SignalRConnectionInfo connectionInfo)
    {
        return connectionInfo;
    }

    [FunctionName("Isos")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "Isos")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
        var httpClient = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/azure/azure-signalr");
        request.Headers.UserAgent.ParseAdd("Serverless");
        var response = await httpClient.SendAsync(request);
        var result = JsonConvert.DeserializeObject<GitResult>(await response.Content.ReadAsStringAsync());
        await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "newMessage",
                Arguments = new[] { $"" }
            });
    }
}
