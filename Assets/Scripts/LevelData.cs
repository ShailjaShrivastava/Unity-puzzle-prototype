using System.Collections.Generic;
using UnityEngine;

namespace ConveyorPuzzle
{
    public enum TileKind
    {
        Conveyor,
        Start,
        End,
        Switch,
        Button,
        Blocker,
        Teleporter
    }

    public enum ParcelColor
    {
        Red,
        Blue,
        Green,
        Yellow
    }

    public sealed class TileDefinition
    {
        public readonly Vector2Int Position;
        public readonly TileKind Kind;
        public readonly GridDirection Direction;
        public readonly GridDirection AlternateDirection;
        public readonly ParcelColor GateColor;
        public readonly int TeleporterId;
        public readonly bool StartsToggled;
        public readonly string Label;

        public TileDefinition(
            int x,
            int y,
            TileKind kind,
            GridDirection direction,
            GridDirection alternateDirection,
            ParcelColor gateColor,
            int teleporterId,
            bool startsToggled,
            string label)
        {
            Position = new Vector2Int(x, y);
            Kind = kind;
            Direction = direction;
            AlternateDirection = alternateDirection;
            GateColor = gateColor;
            TeleporterId = teleporterId;
            StartsToggled = startsToggled;
            Label = label;
        }

        public static TileDefinition Conveyor(int x, int y, GridDirection direction)
        {
            return new TileDefinition(x, y, TileKind.Conveyor, direction, direction, ParcelColor.Red, -1, false, string.Empty);
        }

        public static TileDefinition Start(int x, int y, GridDirection direction)
        {
            return new TileDefinition(x, y, TileKind.Start, direction, direction, ParcelColor.Red, -1, false, "START");
        }

        public static TileDefinition End(int x, int y, ParcelColor gateColor)
        {
            return new TileDefinition(x, y, TileKind.End, GridDirection.Right, GridDirection.Right, gateColor, -1, false, gateColor.ToString().ToUpperInvariant());
        }

        public static TileDefinition Switch(int x, int y, GridDirection direction, GridDirection alternateDirection, bool startsToggled)
        {
            return new TileDefinition(x, y, TileKind.Switch, direction, alternateDirection, ParcelColor.Red, -1, startsToggled, "SW");
        }

        public static TileDefinition Button(int x, int y)
        {
            return new TileDefinition(x, y, TileKind.Button, GridDirection.Right, GridDirection.Right, ParcelColor.Red, -1, false, "REV");
        }

        public static TileDefinition Blocker(int x, int y)
        {
            return new TileDefinition(x, y, TileKind.Blocker, GridDirection.Right, GridDirection.Right, ParcelColor.Red, -1, false, "STOP");
        }

        public static TileDefinition Teleporter(int x, int y, int teleporterId, GridDirection exitDirection)
        {
            return new TileDefinition(x, y, TileKind.Teleporter, exitDirection, exitDirection, ParcelColor.Red, teleporterId, false, "TP" + teleporterId);
        }
    }

    public sealed class ParcelSpawn
    {
        public readonly Vector2Int Position;
        public readonly ParcelColor Color;
        public readonly float Delay;

        public ParcelSpawn(int x, int y, ParcelColor color, float delay)
        {
            Position = new Vector2Int(x, y);
            Color = color;
            Delay = delay;
        }
    }

    public sealed class PuzzleLevel
    {
        public readonly string Name;
        public readonly int Width;
        public readonly int Height;
        public readonly TileDefinition[] Tiles;
        public readonly ParcelSpawn[] Parcels;
        public readonly string GoalText;
        public readonly int StepLimit;

        public PuzzleLevel(
            string name,
            int width,
            int height,
            TileDefinition[] tiles,
            ParcelSpawn[] parcels,
            string goalText,
            int stepLimit)
        {
            Name = name;
            Width = width;
            Height = height;
            Tiles = tiles;
            Parcels = parcels;
            GoalText = goalText;
            StepLimit = stepLimit;
        }
    }

    public static class LevelLibrary
    {
        private static readonly List<PuzzleLevel> BuiltLevels = new List<PuzzleLevel>
        {
            new PuzzleLevel(
                "Level 1 - Fork Sorting",
                7,
                5,
                new[]
                {
                    TileDefinition.Start(0, 2, GridDirection.Right),
                    TileDefinition.Conveyor(1, 2, GridDirection.Right),
                    TileDefinition.Conveyor(2, 2, GridDirection.Right),
                    TileDefinition.Conveyor(3, 2, GridDirection.Right),
                    TileDefinition.Switch(4, 2, GridDirection.Up, GridDirection.Right, false),
                    TileDefinition.Conveyor(4, 3, GridDirection.Up),
                    TileDefinition.End(4, 4, ParcelColor.Blue),
                    TileDefinition.Conveyor(5, 2, GridDirection.Right),
                    TileDefinition.End(6, 2, ParcelColor.Red)
                },
                new[]
                {
                    new ParcelSpawn(0, 2, ParcelColor.Red, 0f)
                },
                "Route the red parcel to the red gate.",
                24),

            new PuzzleLevel(
                "Level 2 - Reverse Belt",
                8,
                5,
                new[]
                {
                    TileDefinition.End(1, 2, ParcelColor.Blue),
                    TileDefinition.Conveyor(2, 2, GridDirection.Right),
                    TileDefinition.Conveyor(3, 2, GridDirection.Right),
                    TileDefinition.Start(4, 2, GridDirection.Right),
                    TileDefinition.Conveyor(5, 2, GridDirection.Right),
                    TileDefinition.End(6, 2, ParcelColor.Red),
                    TileDefinition.Button(4, 0)
                },
                new[]
                {
                    new ParcelSpawn(4, 2, ParcelColor.Blue, 0f)
                },
                "Use the reverse button so the blue parcel reaches the blue gate.",
                20),

            new PuzzleLevel(
                "Level 3 - Teleporter Fork",
                9,
                7,
                new[]
                {
                    TileDefinition.Start(0, 3, GridDirection.Right),
                    TileDefinition.Conveyor(1, 3, GridDirection.Right),
                    TileDefinition.Conveyor(2, 3, GridDirection.Right),
                    TileDefinition.Switch(3, 3, GridDirection.Up, GridDirection.Right, false),
                    TileDefinition.Conveyor(3, 4, GridDirection.Up),
                    TileDefinition.Blocker(3, 5),
                    TileDefinition.Teleporter(4, 3, 1, GridDirection.Right),
                    TileDefinition.Teleporter(6, 1, 1, GridDirection.Up),
                    TileDefinition.Conveyor(6, 2, GridDirection.Up),
                    TileDefinition.Switch(6, 3, GridDirection.Left, GridDirection.Right, false),
                    TileDefinition.End(5, 3, ParcelColor.Red),
                    TileDefinition.Conveyor(7, 3, GridDirection.Right),
                    TileDefinition.End(8, 3, ParcelColor.Green)
                },
                new[]
                {
                    new ParcelSpawn(0, 3, ParcelColor.Green, 0f)
                },
                "Chain two switches through the teleporter and avoid the red gate.",
                42)
        };

        public static IReadOnlyList<PuzzleLevel> Levels
        {
            get { return BuiltLevels; }
        }
    }
}
