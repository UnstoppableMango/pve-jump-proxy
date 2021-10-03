namespace ProxmoxJumpProxy.Models;

public record UploadContentRequest(
    string Node,
    string Storage,
    string Name);
