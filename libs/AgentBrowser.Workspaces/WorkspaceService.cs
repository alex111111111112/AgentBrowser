using System.Text;
using System.Text.Json;

namespace AgentBrowser.Workspaces;

public static class WorkspaceService
{
    private const string DefaultWorkspaceId = "default";

    public static WorkspacePaths EnsureDefaultWorkspace(string baseDir)
    {
        var paths = new WorkspacePaths(baseDir, DefaultWorkspaceId);
        Directory.CreateDirectory(paths.WorkspacesRootDir);
        Directory.CreateDirectory(paths.WorkspaceRootDir);

        if (!File.Exists(paths.ManifestPath))
        {
            SaveManifest(paths, CreateDefaultManifest());
        }

        _ = SessionLifecycleService.LoadStatus(paths);

        return paths;
    }

    public static WorkspaceManifest LoadManifest(WorkspacePaths paths)
    {
        if (!File.Exists(paths.ManifestPath))
        {
            WorkspaceManifest manifest = CreateDefaultManifest();
            SaveManifest(paths, manifest);
            return manifest;
        }

        WorkspaceManifest? loaded = JsonSerializer.Deserialize<WorkspaceManifest>(File.ReadAllText(paths.ManifestPath));
        return loaded ?? CreateDefaultManifest();
    }

    public static void SaveManifest(WorkspacePaths paths, WorkspaceManifest manifest)
    {
        manifest.UpdatedUtc = DateTime.UtcNow;
        Directory.CreateDirectory(paths.WorkspaceRootDir);

        string json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(paths.ManifestPath, json, new UTF8Encoding(false));
    }

    private static WorkspaceManifest CreateDefaultManifest()
    {
        return new WorkspaceManifest
        {
            Id = DefaultWorkspaceId,
            Name = "Default Workspace",
            SchemaVersion = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
    }
}
