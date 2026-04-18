using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentBrowser.Workspaces;

public static class SessionLifecycleService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static SessionStatus LoadStatus(WorkspacePaths paths)
    {
        if (!File.Exists(paths.SessionStatusPath))
        {
            SessionStatus status = CreateDefaultStatus(paths);
            SaveStatus(paths, status);
            return status;
        }

        try
        {
            SessionStatus? loaded = JsonSerializer.Deserialize<SessionStatus>(File.ReadAllText(paths.SessionStatusPath), JsonOptions);
            if (loaded is null)
            {
                return ResetToDefault(paths, "Session status file was empty. Reset to default.");
            }

            loaded.WorkspaceId = string.IsNullOrWhiteSpace(loaded.WorkspaceId) ? paths.WorkspaceId : loaded.WorkspaceId;
            return loaded;
        }
        catch
        {
            return ResetToDefault(paths, "Session status file could not be parsed. Reset to default.");
        }
    }

    public static void SaveStatus(WorkspacePaths paths, SessionStatus status)
    {
        status.WorkspaceId = paths.WorkspaceId;
        status.UpdatedUtc = DateTime.UtcNow;

        Directory.CreateDirectory(paths.WorkspaceRootDir);
        string json = JsonSerializer.Serialize(status, JsonOptions);
        File.WriteAllText(paths.SessionStatusPath, json, new UTF8Encoding(false));
    }

    public static SessionStatus MarkStarting(WorkspacePaths paths, string message, string? singBoxExecutable = null)
    {
        SessionStatus status = LoadStatus(paths);
        status.State = SessionState.Starting;
        status.Message = message;
        status.StartedUtc = DateTime.UtcNow;
        status.StoppedUtc = null;
        status.SingBoxExecutable = singBoxExecutable ?? status.SingBoxExecutable;
        status.SingBoxPid = null;
        status.BrowserExecutable = null;
        status.BrowserPid = null;
        SaveStatus(paths, status);
        return status;
    }

    public static SessionStatus MarkRunning(
        WorkspacePaths paths,
        string message,
        int? singBoxPid = null,
        string? singBoxExecutable = null,
        int? browserPid = null,
        string? browserExecutable = null)
    {
        SessionStatus status = LoadStatus(paths);
        status.State = SessionState.Running;
        status.Message = message;
        status.StartedUtc ??= DateTime.UtcNow;
        status.StoppedUtc = null;
        status.SingBoxPid = singBoxPid ?? status.SingBoxPid;
        status.SingBoxExecutable = singBoxExecutable ?? status.SingBoxExecutable;
        status.BrowserPid = browserPid ?? status.BrowserPid;
        status.BrowserExecutable = browserExecutable ?? status.BrowserExecutable;
        SaveStatus(paths, status);
        return status;
    }

    public static SessionStatus MarkStopping(WorkspacePaths paths, string message)
    {
        SessionStatus status = LoadStatus(paths);
        status.State = SessionState.Stopping;
        status.Message = message;
        SaveStatus(paths, status);
        return status;
    }

    public static SessionStatus MarkStopped(WorkspacePaths paths, string message)
    {
        SessionStatus status = LoadStatus(paths);
        status.State = SessionState.Stopped;
        status.Message = message;
        status.StoppedUtc = DateTime.UtcNow;
        status.SingBoxPid = null;
        status.BrowserPid = null;
        SaveStatus(paths, status);
        return status;
    }

    public static SessionStatus MarkError(WorkspacePaths paths, string message)
    {
        SessionStatus status = LoadStatus(paths);
        status.State = SessionState.Error;
        status.Message = message;
        status.StoppedUtc = DateTime.UtcNow;
        SaveStatus(paths, status);
        return status;
    }

    private static SessionStatus ResetToDefault(WorkspacePaths paths, string message)
    {
        SessionStatus status = CreateDefaultStatus(paths);
        status.Message = message;
        SaveStatus(paths, status);
        return status;
    }

    private static SessionStatus CreateDefaultStatus(WorkspacePaths paths)
    {
        return new SessionStatus
        {
            WorkspaceId = paths.WorkspaceId,
            State = SessionState.Stopped,
            Message = "Session is idle.",
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            StoppedUtc = DateTime.UtcNow
        };
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
