using UnityEngine;

namespace ConveyorPuzzle
{
    public static class PuzzleGrid
    {
        public const float TileSize = 1.12f;
        public const float TileHeight = 0.12f;
        public const float ParcelHeight = 0.42f;

        public static bool IsInside(PuzzleLevel level, Vector2Int coordinate)
        {
            return coordinate.x >= 0 &&
                   coordinate.y >= 0 &&
                   coordinate.x < level.Width &&
                   coordinate.y < level.Height;
        }

        public static Vector3 CellToWorld(PuzzleLevel level, Vector2Int coordinate)
        {
            float originX = (level.Width - 1) * TileSize * -0.5f;
            float originZ = (level.Height - 1) * TileSize * -0.5f;
            return new Vector3(originX + coordinate.x * TileSize, 0f, originZ + coordinate.y * TileSize);
        }

        public static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
