using Microsoft.AspNetCore.SignalR.Client;
using ProxmoxJumpProxy.Models;

namespace ProxmoxJumpProxy.Daemon;

public class HostListener : IHostedService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<HostListener> _logger;
    private readonly List<IDisposable> _handlers = new();

    public HostListener(HubConnection hubConnection, ILogger<HostListener> logger)
    {
        _hubConnection = hubConnection;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hubConnection.Closed += OnClosed;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Reconnecting += OnReconnecting;

        _handlers.AddRange(new[] {
            _hubConnection.On(Operations.NodeStorage.UploadContent, (Func<UploadContentRequest, Task>)OnUploadContent)
        });

        return _hubConnection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _handlers.ForEach(x => x.Dispose());
        return _hubConnection.StopAsync(cancellationToken);
    }

    private async Task OnUploadContent(UploadContentRequest data)
    {
        _logger.LogInformation(data.ToString());

        string node = data.Node;
        string storage = data.Storage;
        string name = data.Name;

        _logger.LogInformation("Requesting channel");
        var reader = await _hubConnection.StreamAsChannelAsync<object>("ReceiveContent", node, storage, name);

        _logger.LogInformation("Waiting to read");
        while (await reader.WaitToReadAsync())
        {
            _logger.LogInformation("Trying to read");
            while (reader.TryRead(out var value))
            {
                _logger.LogInformation(value.ToString());
            }
        }
    }

    private Task OnClosed(Exception exception)
    {
        _logger.LogInformation(exception, "Connection closed");
        return Task.CompletedTask;
    }

    private Task OnReconnected(string connectionId)
    {
        _logger.LogInformation("Reconnected. New id: {ConnectionId}", connectionId);
        return Task.CompletedTask;
    }

    private Task OnReconnecting(Exception exception)
    {
        _logger.LogInformation(exception, "Reconnecting");
        return Task.CompletedTask;
    }
}
