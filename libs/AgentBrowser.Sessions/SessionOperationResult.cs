namespace AgentBrowser.Sessions;

public sealed class SessionOperationResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static SessionOperationResult Ok() => new() { Success = true };

    public static SessionOperationResult Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}
