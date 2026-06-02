using System.Collections.Generic;
using UnityEngine;

namespace ConveyorPuzzle
{
    public sealed class LevelValidationReport
    {
        private readonly List<string> issues = new List<string>();

        public bool IsValid
        {
            get { return issues.Count == 0; }
        }

        public int IssueCount
        {
            get { return issues.Count; }
        }

        public void AddIssue(string issue)
        {
            issues.Add(issue);
        }

        public string ToSummary()
        {
            return IsValid ? "Validation: OK" : "Validation: " + IssueCount + " issue(s), see Console.";
        }

        public string ToMultilineString(string levelName)
        {
            if (IsValid)
            {
                return levelName + " passed validation.";
            }

            string message = levelName + " validation issues:";
            for (int i = 0; i < issues.Count; i++)
            {
                message += "\n- " + issues[i];
            }

            return message;
        }
    }

    public static class LevelValidator
    {
        public static LevelValidationReport Validate(PuzzleLevel level)
        {
            LevelValidationReport report = new LevelValidationReport();
            Dictionary<Vector2Int, TileDefinition> tilesByPosition = new Dictionary<Vector2Int, TileDefinition>();
            Dictionary<int, int> teleporterCounts = new Dictionary<int, int>();
            bool hasStart = false;
            bool hasGate = false;

            for (int i = 0; i < level.Tiles.Length; i++)
            {
                TileDefinition tile = level.Tiles[i];

                if (!PuzzleGrid.IsInside(level, tile.Position))
                {
                    report.AddIssue(tile.Kind + " at " + tile.Position + " is outside the board.");
                    continue;
                }

                if (tilesByPosition.ContainsKey(tile.Position))
                {
                    report.AddIssue("Duplicate tile position at " + tile.Position + ".");
                }
                else
                {
                    tilesByPosition.Add(tile.Position, tile);
                }

                if (tile.Kind == TileKind.Start)
                {
                    hasStart = true;
                }
                else if (tile.Kind == TileKind.End)
                {
                    hasGate = true;
                }
                else if (tile.Kind == TileKind.Switch && tile.Direction == tile.AlternateDirection)
                {
                    report.AddIssue("Switch at " + tile.Position + " has matching primary and alternate directions.");
                }
                else if (tile.Kind == TileKind.Teleporter)
                {
                    if (!teleporterCounts.ContainsKey(tile.TeleporterId))
                    {
                        teleporterCounts.Add(tile.TeleporterId, 0);
                    }

                    teleporterCounts[tile.TeleporterId]++;
                }
            }

            if (!hasStart)
            {
                report.AddIssue("Level has no start tile.");
            }

            if (!hasGate)
            {
                report.AddIssue("Level has no color gate.");
            }

            for (int i = 0; i < level.Parcels.Length; i++)
            {
                ParcelSpawn spawn = level.Parcels[i];
                TileDefinition tile;
                if (!tilesByPosition.TryGetValue(spawn.Position, out tile))
                {
                    report.AddIssue("Parcel spawn at " + spawn.Position + " has no tile.");
                }
                else if (tile.Kind != TileKind.Start)
                {
                    report.AddIssue("Parcel spawn at " + spawn.Position + " is on " + tile.Kind + " instead of Start.");
                }
            }

            foreach (KeyValuePair<int, int> teleporterCount in teleporterCounts)
            {
                if (teleporterCount.Value != 2)
                {
                    report.AddIssue("Teleporter id " + teleporterCount.Key + " has " + teleporterCount.Value + " endpoint(s); expected 2.");
                }
            }

            return report;
        }
    }
}
