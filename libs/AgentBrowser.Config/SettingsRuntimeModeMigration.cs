using System.Text.Json.Nodes;

namespace AgentBrowser.Config;

public static class SettingsRuntimeModeMigration
{
    public static bool HasPresetArray(JsonNode? root)
    {
        return FindProperty(root as JsonObject, "presets") is JsonArray;
    }

    public static bool ApplyMissingPresetRuntimeModes(JsonNode? root, RuntimeMode fallbackRuntimeMode)
    {
        if (FindProperty(root as JsonObject, "presets") is not JsonArray presets)
        {
            return false;
        }

        bool changed = false;
        foreach (JsonNode? presetNode in presets)
        {
            if (presetNode is not JsonObject preset || FindProperty(preset, "runtimeMode") is not null)
            {
                continue;
            }

            preset["runtimeMode"] = fallbackRuntimeMode.ToString();
            changed = true;
        }

        return changed;
    }

    private static JsonNode? FindProperty(JsonObject? jsonObject, string propertyName)
    {
        if (jsonObject is null)
        {
            return null;
        }

        foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
        {
            if (string.Equals(property.Key, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }
}
