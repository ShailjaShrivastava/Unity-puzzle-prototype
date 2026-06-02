using System.Collections.Generic;
using UnityEngine;

namespace ConveyorPuzzle
{
    public sealed class PuzzleTheme
    {
        public Material BaseMaterial { get; private set; }
        public Material ConveyorMaterial { get; private set; }
        public Material ConveyorReverseMaterial { get; private set; }
        public Material StartMaterial { get; private set; }
        public Material SwitchMaterial { get; private set; }
        public Material SwitchOnMaterial { get; private set; }
        public Material ButtonMaterial { get; private set; }
        public Material ButtonOnMaterial { get; private set; }
        public Material BlockerMaterial { get; private set; }
        public Material TeleporterMaterial { get; private set; }
        public Material ArrowMaterial { get; private set; }
        public Material DimArrowMaterial { get; private set; }
        public Material PreviewMaterial { get; private set; }

        private readonly Dictionary<ParcelColor, Material> parcelMaterials = new Dictionary<ParcelColor, Material>();
        private readonly Dictionary<ParcelColor, Material> gateMaterials = new Dictionary<ParcelColor, Material>();

        public static PuzzleTheme Create()
        {
            PuzzleTheme theme = new PuzzleTheme
            {
                BaseMaterial = MakeMaterial(new Color(0.18f, 0.19f, 0.19f)),
                ConveyorMaterial = MakeMaterial(new Color(0.08f, 0.42f, 0.82f)),
                ConveyorReverseMaterial = MakeMaterial(new Color(0.12f, 0.64f, 0.78f)),
                StartMaterial = MakeMaterial(new Color(0.92f, 0.72f, 0.18f)),
                SwitchMaterial = MakeMaterial(new Color(0.95f, 0.48f, 0.18f)),
                SwitchOnMaterial = MakeMaterial(new Color(0.97f, 0.76f, 0.16f)),
                ButtonMaterial = MakeMaterial(new Color(0.44f, 0.25f, 0.72f)),
                ButtonOnMaterial = MakeMaterial(new Color(0.72f, 0.36f, 0.92f)),
                BlockerMaterial = MakeMaterial(new Color(0.07f, 0.07f, 0.08f)),
                TeleporterMaterial = MakeMaterial(new Color(0.78f, 0.2f, 0.7f)),
                ArrowMaterial = MakeMaterial(new Color(0.94f, 0.98f, 1f)),
                DimArrowMaterial = MakeMaterial(new Color(0.55f, 0.6f, 0.66f)),
                PreviewMaterial = MakeMaterial(new Color(0.95f, 0.96f, 0.42f))
            };

            theme.parcelMaterials[ParcelColor.Red] = MakeMaterial(new Color(0.94f, 0.18f, 0.18f));
            theme.parcelMaterials[ParcelColor.Blue] = MakeMaterial(new Color(0.16f, 0.42f, 0.96f));
            theme.parcelMaterials[ParcelColor.Green] = MakeMaterial(new Color(0.1f, 0.76f, 0.34f));
            theme.parcelMaterials[ParcelColor.Yellow] = MakeMaterial(new Color(0.98f, 0.85f, 0.12f));

            theme.gateMaterials[ParcelColor.Red] = MakeMaterial(new Color(0.62f, 0.1f, 0.1f));
            theme.gateMaterials[ParcelColor.Blue] = MakeMaterial(new Color(0.08f, 0.2f, 0.58f));
            theme.gateMaterials[ParcelColor.Green] = MakeMaterial(new Color(0.06f, 0.42f, 0.18f));
            theme.gateMaterials[ParcelColor.Yellow] = MakeMaterial(new Color(0.68f, 0.54f, 0.08f));

            return theme;
        }

        public Material ParcelMaterial(ParcelColor color)
        {
            return parcelMaterials[color];
        }

        public Material TileMaterial(TileRuntime tile, bool reverseDrive)
        {
            switch (tile.Definition.Kind)
            {
                case TileKind.Conveyor:
                    return reverseDrive ? ConveyorReverseMaterial : ConveyorMaterial;
                case TileKind.Start:
                    return StartMaterial;
                case TileKind.End:
                    return gateMaterials[tile.Definition.GateColor];
                case TileKind.Switch:
                    return tile.Toggled ? SwitchOnMaterial : SwitchMaterial;
                case TileKind.Button:
                    return reverseDrive ? ButtonOnMaterial : ButtonMaterial;
                case TileKind.Blocker:
                    return BlockerMaterial;
                case TileKind.Teleporter:
                    return TeleporterMaterial;
                default:
                    return ConveyorMaterial;
            }
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = color;
            return material;
        }
    }
}
