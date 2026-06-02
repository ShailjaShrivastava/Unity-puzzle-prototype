using UnityEngine;

namespace ConveyorPuzzle
{
    public sealed class ParcelView
    {
        private Transform root;

        public ParcelRuntime CreateParcel(
            PuzzleLevel level,
            ParcelSpawn spawn,
            GridDirection initialDirection,
            PuzzleTheme theme)
        {
            EnsureRoot();

            ParcelRuntime parcel = new ParcelRuntime
            {
                Spawn = spawn,
                Color = spawn.Color,
                Cell = spawn.Position,
                Direction = initialDirection,
                DelayRemaining = spawn.Delay
            };

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = spawn.Color + " Parcel";
            visual.transform.SetParent(root, false);
            visual.transform.position = PuzzleGrid.CellToWorld(level, spawn.Position) + Vector3.up * PuzzleGrid.ParcelHeight;
            visual.transform.localScale = Vector3.one * 0.42f;
            visual.GetComponent<Renderer>().sharedMaterial = theme.ParcelMaterial(spawn.Color);

            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.Destroy(collider);
            }

            parcel.Root = visual;
            return parcel;
        }

        public void Clear()
        {
            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
                root = null;
            }
        }

        private void EnsureRoot()
        {
            if (root == null)
            {
                root = new GameObject("Parcels").transform;
            }
        }
    }
}
