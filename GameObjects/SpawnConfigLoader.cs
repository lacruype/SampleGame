using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SampleGame.GameObjects;

public class SpawnConfig
{
    public List<SpawnCategory> Categories { get; set; } = new();
}

public static class SpawnConfigLoader
{
    public static SpawnConfig Load(string path)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<SpawnConfig>(json, options) ?? new SpawnConfig();
    }
}

