using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
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
public class NodeStorage : ServerlessHub
{
    private const string NodeStorageHub = "nodeStorage";

    [FunctionName("negotiate")]
    public static SignalRConnectionInfo Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous)]
        HttpRequest req,
        [SignalRConnectionInfo(HubName = NodeStorageHub)]
        SignalRConnectionInfo connectionInfo)
    {
        return connectionInfo;
    }

    [FunctionName(nameof(ReceiveContent))]
    public async Task<ChannelReader<object>> ReceiveContent(
        [SignalRTrigger] InvocationContext invocationContext,
        string node,
        string storage,
        string name,
        ILogger log,
        CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<object>();

        await channel.Writer.WriteAsync(new {
            Node = node,
            Storage = storage,
            Name = name,
        }, cancellationToken);

        channel.Writer.Complete();

        log.LogInformation("Node: {Node}, Storage: {Storage}, Name: {Name}", node, storage, name);

        return channel.Reader;
    }

    [FunctionName(nameof(UploadContent))]
    public async Task<IActionResult> UploadContent(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "nodes/{node}/storage/{storage}/upload/{name}")]
        HttpRequest request,
        string node,
        string storage,
        string name,
        ILogger log,
        CancellationToken cancellationToken)
    {
        await Clients.All.SendAsync(
            Operations.NodeStorage.UploadContent,
            new UploadContentRequest(node, storage, name),
            cancellationToken);

        return new OkResult();
    }
}
