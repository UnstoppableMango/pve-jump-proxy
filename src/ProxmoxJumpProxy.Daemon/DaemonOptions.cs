namespace ProxmoxJumpProxy.Daemon;

public record DaemonOptions
{
    public string SignalRUrl { get; init; }
}
