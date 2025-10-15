using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SampleGame.GameObjects;

public class SpawnRule
{
    public string EntityType { get; set; } = string.Empty;
    public int MinTurns { get; set; }
    public int MaxTurns { get; set; }
    public float Weight { get; set; }

    [JsonIgnore]
    private int _nextSpawnTurn;

    public void ScheduleNextSpawn(int currentTurn, Random rng)
    {
        int interval = rng.Next(MinTurns, MaxTurns + 1);
        _nextSpawnTurn = currentTurn + interval;
    }

    public bool ShouldSpawn(int currentTurn) => currentTurn >= _nextSpawnTurn;
}

public class SpawnCategory
{
    public string Name { get; set; } = string.Empty;
    public List<SpawnRule> Rules { get; set; } = new();

    [JsonIgnore]
    private int _nextSpawnTurn = 0;

    public bool CanSpawn(int currentTurn) => currentTurn >= _nextSpawnTurn;

    public SpawnRule? PickWeighted(Random rng)
    {
        float total = Rules.Sum(r => r.Weight);
        float roll = (float)rng.NextDouble() * total;
        float cumulative = 0;
        foreach (var r in Rules)
        {
            cumulative += r.Weight;
            if (roll <= cumulative)
                return r;
        }
        return null;
    }

    public void ScheduleNextSpawn(int currentTurn, SpawnRule rule, Random rng)
    {
        int delay = rng.Next(rule.MinTurns, rule.MaxTurns + 1);
        _nextSpawnTurn = currentTurn + delay;
    }

    public void ResetSpawnTimer(Random rng)
    {
        // Start scheduling as if it's turn 0 again
        var initialRule = Rules[0];
        ScheduleNextSpawn(0, initialRule, rng);
    }

}

