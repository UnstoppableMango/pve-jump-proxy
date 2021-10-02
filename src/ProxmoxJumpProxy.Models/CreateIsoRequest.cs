namespace ProxmoxJumpProxy.Models;

public record CreateIsoRequest(
    string Name,
    string RemoteDirectory,
    string Host,
    int ConnectTimeout);
