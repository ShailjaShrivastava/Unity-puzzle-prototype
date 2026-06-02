using System;
using System.Collections.Generic;
using UnityEngine;

namespace ConveyorPuzzle
{
    public sealed class BoardView
    {
        private readonly Dictionary<Vector2Int, TileRuntime> tiles = new Dictionary<Vector2Int, TileRuntime>();
        private Transform root;
        private Transform previewRoot;

        public void Rebuild(
            PuzzleLevel level,
            PuzzleTheme theme,
            bool reverseDrive,
            Func<TileRuntime, GridDirection> outputResolver)
        {
            Clear();
            root = new GameObject("Board").transform;
            previewRoot = new GameObject("Route Preview").transform;
            previewRoot.SetParent(root, false);

            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    CreateBaseCell(level, new Vector2Int(x, y), theme);
                }
            }

            for (int i = 0; i < level.Tiles.Length; i++)
            {
                TileDefinition definition = level.Tiles[i];
                TileRuntime tile = CreateTile(level, definition);
                tiles.Add(definition.Position, tile);
                UpdateTileAppearance(tile, theme, reverseDrive, outputResolver);
            }
        }

        public void Clear()
        {
            if (root != null)
            {
                UnityEngine.Object.Destroy(root.gameObject);
                root = null;
            }

            tiles.Clear();
        }

        public void ClearPreview()
        {
            if (previewRoot == null)
            {
                return;
            }

            for (int i = previewRoot.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(previewRoot.GetChild(i).gameObject);
            }
        }

        public void SetPreview(PuzzleLevel level, IReadOnlyList<Vector2Int> path, PuzzleTheme theme)
        {
            ClearPreview();

            if (previewRoot == null)
            {
                return;
            }

            for (int i = 0; i < path.Count; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Preview " + i;
                marker.transform.SetParent(previewRoot, false);
                marker.transform.position = PuzzleGrid.CellToWorld(level, path[i]) + Vector3.up * 0.32f;
                marker.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
                marker.GetComponent<Renderer>().sharedMaterial = theme.PreviewMaterial;
                DestroyCollider(marker);
            }
        }

        public bool ContainsTile(Vector2Int coordinate)
        {
            return tiles.ContainsKey(coordinate);
        }

        public bool TryGetTile(Vector2Int coordinate, out TileRuntime tile)
        {
            return tiles.TryGetValue(coordinate, out tile);
        }

        public TileRuntime FindTeleporterPair(TileRuntime source)
        {
            foreach (TileRuntime candidate in tiles.Values)
            {
                if (candidate == source)
                {
                    continue;
                }

                if (candidate.Definition.Kind == TileKind.Teleporter &&
                    candidate.Definition.TeleporterId == source.Definition.TeleporterId)
                {
                    return candidate;
                }
            }

            return null;
        }

        public void UpdateAllTileAppearances(
            PuzzleTheme theme,
            bool reverseDrive,
            Func<TileRuntime, GridDirection> outputResolver)
        {
            foreach (TileRuntime tile in tiles.Values)
            {
                UpdateTileAppearance(tile, theme, reverseDrive, outputResolver);
            }
        }

        public void UpdateTileAppearance(
            TileRuntime tile,
            PuzzleTheme theme,
            bool reverseDrive,
            Func<TileRuntime, GridDirection> outputResolver)
        {
            tile.SurfaceRenderer.sharedMaterial = theme.TileMaterial(tile, reverseDrive);

            for (int i = tile.MarkerRoot.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(tile.MarkerRoot.GetChild(i).gameObject);
            }

            switch (tile.Definition.Kind)
            {
                case TileKind.Conveyor:
                case TileKind.Start:
                case TileKind.Button:
                case TileKind.Teleporter:
                    CreateArrow(tile.MarkerRoot, outputResolver(tile), theme.ArrowMaterial);
                    break;
                case TileKind.Switch:
                    CreateArrow(tile.MarkerRoot, outputResolver(tile), theme.ArrowMaterial);
                    CreateTinyDirectionMark(tile.MarkerRoot, tile.Toggled ? tile.Definition.Direction : tile.Definition.AlternateDirection, theme);
                    break;
            }
        }

        private TileRuntime CreateTile(PuzzleLevel level, TileDefinition definition)
        {
            TileRuntime tile = new TileRuntime
            {
                Definition = definition,
                Toggled = definition.StartsToggled
            };

            GameObject tileRoot = new GameObject(definition.Kind + " " + definition.Position);
            tileRoot.transform.SetParent(root, false);
            tileRoot.transform.position = PuzzleGrid.CellToWorld(level, definition.Position);
            tile.Root = tileRoot;

            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "Surface";
            surface.transform.SetParent(tileRoot.transform, false);
            surface.transform.localPosition = Vector3.up * (PuzzleGrid.TileHeight * 0.5f + 0.03f);
            surface.transform.localScale = new Vector3(0.92f, PuzzleGrid.TileHeight, 0.92f);
            tile.SurfaceRenderer = surface.GetComponent<Renderer>();

            PuzzleTileClickTarget clickTarget = tileRoot.AddComponent<PuzzleTileClickTarget>();
            clickTarget.Coordinate = definition.Position;

            tile.MarkerRoot = new GameObject("Markers").transform;
            tile.MarkerRoot.SetParent(tileRoot.transform, false);

            if (!string.IsNullOrEmpty(definition.Label))
            {
                CreateLabel(tileRoot.transform, definition.Label, Vector3.up * 0.2f, 0.055f, Color.white);
            }

            return tile;
        }

        private void CreateBaseCell(PuzzleLevel level, Vector2Int coordinate, PuzzleTheme theme)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.name = "Grid " + coordinate.x + "," + coordinate.y;
            cell.transform.SetParent(root, false);
            cell.transform.position = PuzzleGrid.CellToWorld(level, coordinate);
            cell.transform.localScale = new Vector3(1f, 0.035f, 1f);
            cell.GetComponent<Renderer>().sharedMaterial = theme.BaseMaterial;
            DestroyCollider(cell);
        }

        private void CreateArrow(Transform parent, GridDirection direction, Material material)
        {
            GameObject arrow = new GameObject("Arrow " + direction);
            arrow.transform.SetParent(parent, false);
            arrow.transform.localRotation = direction.ToWorldRotation();

            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaft.name = "Shaft";
            shaft.transform.SetParent(arrow.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 0.24f, 0.04f);
            shaft.transform.localScale = new Vector3(0.12f, 0.08f, 0.52f);
            shaft.GetComponent<Renderer>().sharedMaterial = material;
            DestroyCollider(shaft);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Head";
            head.transform.SetParent(arrow.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.24f, 0.32f);
            head.transform.localScale = new Vector3(0.34f, 0.08f, 0.2f);
            head.GetComponent<Renderer>().sharedMaterial = material;
            DestroyCollider(head);
        }

        private void CreateTinyDirectionMark(Transform parent, GridDirection direction, PuzzleTheme theme)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Alternate " + direction;
            marker.transform.SetParent(parent, false);
            marker.transform.localRotation = direction.ToWorldRotation();
            marker.transform.localPosition = Vector3.up * 0.25f;
            marker.transform.localScale = new Vector3(0.07f, 0.06f, 0.42f);
            marker.GetComponent<Renderer>().sharedMaterial = theme.DimArrowMaterial;
            DestroyCollider(marker);
        }

        private void CreateLabel(Transform parent, string text, Vector3 localPosition, float size, Color color)
        {
            GameObject label = new GameObject("Label " + text);
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPosition;
            label.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            TextMesh mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontSize = 64;
            mesh.characterSize = size;
            mesh.color = color;
        }

        private void DestroyCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.Destroy(collider);
            }
        }
    }
}
