using System.IO.Compression;
using System.Text;
using System.Text.Json;
using AgentBrowser.Workspaces;

namespace AgentBrowser.Support;

public static class SupportBundleService
{
    public static SupportBundleResult CreateBundle(string baseDir, string destinationZipPath)
    {
        WorkspacePaths workspacePaths = WorkspaceService.EnsureDefaultWorkspace(baseDir);
        var includedFiles = new List<string>();

        try
        {
            string? destinationDir = Path.GetDirectoryName(destinationZipPath);
            if (!string.IsNullOrWhiteSpace(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            if (File.Exists(destinationZipPath))
            {
                File.Delete(destinationZipPath);
            }

            using ZipArchive archive = ZipFile.Open(destinationZipPath, ZipArchiveMode.Create);

            AddIfExists(archive, includedFiles, Path.Combine(baseDir, "ui.log"), "logs/ui.log");
            AddIfExists(archive, includedFiles, Path.Combine(baseDir, "start.log"), "logs/start.log");
            AddIfExists(archive, includedFiles, Path.Combine(baseDir, "stop.log"), "logs/stop.log");
            AddIfExists(archive, includedFiles, Path.Combine(baseDir, "ui-settings.json"), "root/ui-settings.json");
            AddIfExists(archive, includedFiles, Path.Combine(baseDir, "core", "config.json"), "core/config.json");
            AddIfExists(archive, includedFiles, Path.Combine(baseDir, "core", "sing-box.log"), "core/sing-box.log");
            AddIfExists(archive, includedFiles, workspacePaths.ManifestPath, "workspace/manifest.json");
            AddIfExists(archive, includedFiles, workspacePaths.WorkspaceSettingsPath, "workspace/ui-settings.json");
            AddIfExists(archive, includedFiles, workspacePaths.SessionStatusPath, "workspace/session-status.json");

            WriteManifest(archive, workspacePaths, baseDir, includedFiles);

            return new SupportBundleResult
            {
                Success = true,
                BundlePath = destinationZipPath,
                IncludedFileCount = includedFiles.Count
            };
        }
        catch (Exception ex)
        {
            return new SupportBundleResult
            {
                Success = false,
                BundlePath = destinationZipPath,
                IncludedFileCount = includedFiles.Count,
                ErrorMessage = ex.Message
            };
        }
    }

    private static void AddIfExists(ZipArchive archive, List<string> includedFiles, string sourcePath, string archivePath)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        ZipArchiveEntry entry = archive.CreateEntry(archivePath, CompressionLevel.Optimal);
        using Stream entryStream = entry.Open();
        using FileStream sourceStream = File.OpenRead(sourcePath);
        sourceStream.CopyTo(entryStream);
        includedFiles.Add(archivePath);
    }

    private static void WriteManifest(ZipArchive archive, WorkspacePaths workspacePaths, string baseDir, List<string> includedFiles)
    {
        var manifest = new SupportBundleManifest
        {
            CreatedUtc = DateTime.UtcNow,
            WorkspaceId = workspacePaths.WorkspaceId,
            SourceBaseDir = baseDir,
            IncludedFiles = includedFiles.ToList()
        };

        ZipArchiveEntry entry = archive.CreateEntry("bundle-manifest.json", CompressionLevel.Optimal);
        using StreamWriter writer = new(entry.Open(), new UTF8Encoding(false));
        string json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        writer.Write(json);
    }
}
