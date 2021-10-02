using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using ProxmoxJumpProxy.Models;

namespace ProxmoxJumpProxy.Host;

[PublicAPI]
public class Isos : ServerlessHub
{
    [FunctionName("negotiate")]
    public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
    {
        return Negotiate(req.Headers["x-ms-signalr-user-id"], GetClaims(req.Headers["Authorization"]));
    }

    [FunctionName("Isos")]
    public async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
        ILogger log,
        CancellationToken cancellationToken)
    {
        var createRequest = await request.ReadFromJsonAsync<CreateIsoRequest>(cancellationToken);
        await Clients.All.SendAsync(Operations.CreateIso, createRequest, cancellationToken);

        return new OkResult();
    }
}
