using UnityEngine;

namespace ConveyorPuzzle
{
    public sealed class TileRuntime
    {
        public TileDefinition Definition;
        public bool Toggled;
        public GameObject Root;
        public Renderer SurfaceRenderer;
        public Transform MarkerRoot;
    }

    public sealed class ParcelRuntime
    {
        public ParcelSpawn Spawn;
        public ParcelColor Color;
        public Vector2Int Cell;
        public Vector2Int TargetCell;
        public GridDirection Direction;
        public GameObject Root;
        public Vector3 From;
        public Vector3 To;
        public float Progress;
        public float DelayRemaining;
        public int Steps;
        public bool Moving;
        public bool Delivered;
    }
}
