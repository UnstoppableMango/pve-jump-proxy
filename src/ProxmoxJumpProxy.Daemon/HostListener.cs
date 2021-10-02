using Microsoft.AspNetCore.SignalR.Client;

namespace ProxmoxJumpProxy.Daemon;

public class HostListener : IHostedService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<HostListener> _logger;

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

        return _hubConnection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _hubConnection.StopAsync(cancellationToken);
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
