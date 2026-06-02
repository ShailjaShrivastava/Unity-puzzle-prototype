using UnityEngine;

namespace ConveyorPuzzle
{
    public enum GridDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    public static class GridDirectionExtensions
    {
        public static Vector2Int ToOffset(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return Vector2Int.up;
                case GridDirection.Right:
                    return Vector2Int.right;
                case GridDirection.Down:
                    return Vector2Int.down;
                default:
                    return Vector2Int.left;
            }
        }

        public static GridDirection Opposite(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return GridDirection.Down;
                case GridDirection.Right:
                    return GridDirection.Left;
                case GridDirection.Down:
                    return GridDirection.Up;
                default:
                    return GridDirection.Right;
            }
        }

        public static Quaternion ToWorldRotation(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return Quaternion.Euler(0f, 0f, 0f);
                case GridDirection.Right:
                    return Quaternion.Euler(0f, 90f, 0f);
                case GridDirection.Down:
                    return Quaternion.Euler(0f, 180f, 0f);
                default:
                    return Quaternion.Euler(0f, 270f, 0f);
            }
        }

        public static string ToShortName(this GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up:
                    return "Up";
                case GridDirection.Right:
                    return "Right";
                case GridDirection.Down:
                    return "Down";
                default:
                    return "Left";
            }
        }
    }
}
