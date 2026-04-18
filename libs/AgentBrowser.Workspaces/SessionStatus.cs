namespace AgentBrowser.Workspaces;

public sealed class SessionStatus
{
    public string WorkspaceId { get; set; } = "default";
    public SessionState State { get; set; } = SessionState.Stopped;
    public string Message { get; set; } = "Session is idle.";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedUtc { get; set; }
    public DateTime? StoppedUtc { get; set; }
    public int? SingBoxPid { get; set; }
    public int? BrowserPid { get; set; }
    public string? SingBoxExecutable { get; set; }
    public string? BrowserExecutable { get; set; }
}
