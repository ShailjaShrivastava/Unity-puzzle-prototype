using System.Collections.Generic;
using UnityEngine;

namespace ConveyorPuzzle
{
    public sealed class ConveyorPuzzleGame : MonoBehaviour
    {
        private readonly List<ParcelRuntime> parcels = new List<ParcelRuntime>();

        private PuzzleTheme theme;
        private BoardView boardView;
        private ParcelView parcelView;
        private HudView hudView;

        private PuzzleLevel currentLevel;
        private LevelValidationReport validationReport;
        private int currentLevelIndex;
        private bool reverseDrive;
        private GamePhase phase;
        private string statusText;
        private float statusFlashTime;
        private float restartCountdown = -1f;

        private Camera gameCamera;
        private Light sceneLight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<ConveyorPuzzleGame>() != null)
            {
                return;
            }

            GameObject host = new GameObject("Conveyor Puzzle Game");
            host.AddComponent<ConveyorPuzzleGame>();
        }

        private void Awake()
        {
            theme = PuzzleTheme.Create();
            boardView = new BoardView();
            parcelView = new ParcelView();
            hudView = new HudView();

            EnsureSceneEssentials();
            LoadLevel(0);
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();

            if (phase == GamePhase.Running)
            {
                TickParcels(Time.deltaTime);
            }

            if (phase == GamePhase.Lost && restartCountdown > 0f)
            {
                restartCountdown -= Time.deltaTime;
                if (restartCountdown <= 0f)
                {
                    RestartLevel();
                    return;
                }
            }

            if (statusFlashTime > 0f)
            {
                statusFlashTime -= Time.deltaTime;
            }
        }

        private void OnGUI()
        {
            HandleHudCommand(hudView.Draw(CreateHudModel()));
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadLevel(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadLevel(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadLevel(2);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartLevel();
            }

            if (Input.GetKeyDown(KeyCode.N) && (phase == GamePhase.Won || phase == GamePhase.Complete))
            {
                LoadNextLevel();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (phase == GamePhase.Running)
                {
                    phase = GamePhase.Paused;
                    statusText = "Paused.";
                }
                else if (phase == GamePhase.Planning || phase == GamePhase.Paused)
                {
                    BeginRun();
                }
            }
        }

        private HudModel CreateHudModel()
        {
            int deliveredCount = 0;
            int totalSteps = 0;
            for (int i = 0; i < parcels.Count; i++)
            {
                if (parcels[i].Delivered)
                {
                    deliveredCount++;
                }

                totalSteps += parcels[i].Steps;
            }

            return new HudModel
            {
                LevelName = currentLevel.Name,
                GoalText = currentLevel.GoalText,
                StatusText = statusText,
                ValidationText = validationReport != null ? validationReport.ToSummary() : "Validation: pending",
                Phase = phase,
                ReverseDrive = reverseDrive,
                LevelIndex = currentLevelIndex,
                LevelCount = LevelLibrary.Levels.Count,
                DeliveredCount = deliveredCount,
                ParcelCount = parcels.Count,
                TotalSteps = totalSteps,
                StepLimit = currentLevel.StepLimit,
                StatusFlashTime = statusFlashTime
            };
        }

        private void HandleHudCommand(HudCommand command)
        {
            switch (command)
            {
                case HudCommand.Run:
                    BeginRun();
                    break;
                case HudCommand.Pause:
                    phase = GamePhase.Paused;
                    statusText = "Paused.";
                    break;
                case HudCommand.Restart:
                    RestartLevel();
                    break;
                case HudCommand.PreviousLevel:
                    LoadLevel(currentLevelIndex - 1);
                    break;
                case HudCommand.NextLevel:
                    LoadNextLevel();
                    break;
            }
        }

        private void HandleMouseInput()
        {
            if (!Input.GetMouseButtonDown(0) || gameCamera == null)
            {
                return;
            }

            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, 100f))
            {
                return;
            }

            PuzzleTileClickTarget target = hit.collider.GetComponentInParent<PuzzleTileClickTarget>();
            if (target == null)
            {
                return;
            }

            InteractWithTile(target.Coordinate);
        }

        private void InteractWithTile(Vector2Int coordinate)
        {
            TileRuntime tile;
            if (!boardView.TryGetTile(coordinate, out tile))
            {
                return;
            }

            switch (tile.Definition.Kind)
            {
                case TileKind.Switch:
                    tile.Toggled = !tile.Toggled;
                    boardView.UpdateTileAppearance(tile, theme, reverseDrive, GetTileOutput);
                    RefreshPreview();
                    FlashStatus("Switch now routes " + GetTileOutput(tile).ToShortName() + ".");
                    break;
                case TileKind.Button:
                    ToggleReverseDrive();
                    break;
            }
        }

        private void BeginRun()
        {
            if (phase == GamePhase.Won || phase == GamePhase.Lost || phase == GamePhase.Complete)
            {
                return;
            }

            phase = GamePhase.Running;
            statusText = "Parcels moving.";
            boardView.ClearPreview();

            for (int i = 0; i < parcels.Count; i++)
            {
                ParcelRuntime parcel = parcels[i];
                if (!parcel.Delivered && !parcel.Moving)
                {
                    StartSegment(parcel);
                }
            }
        }

        private void TickParcels(float deltaTime)
        {
            for (int i = 0; i < parcels.Count; i++)
            {
                ParcelRuntime parcel = parcels[i];
                if (parcel.Delivered)
                {
                    continue;
                }

                if (parcel.DelayRemaining > 0f)
                {
                    parcel.DelayRemaining -= deltaTime;
                    continue;
                }

                if (!parcel.Moving)
                {
                    StartSegment(parcel);
                    if (phase != GamePhase.Running)
                    {
                        return;
                    }
                }

                parcel.Progress += deltaTime * 1.85f;
                float t = Mathf.Clamp01(parcel.Progress);
                parcel.Root.transform.position = Vector3.Lerp(parcel.From, parcel.To, PuzzleGrid.SmoothStep(t));

                if (parcel.Progress >= 1f)
                {
                    ArriveAtTarget(parcel);
                    if (phase != GamePhase.Running)
                    {
                        return;
                    }
                }
            }
        }

        private void StartSegment(ParcelRuntime parcel)
        {
            parcel.TargetCell = parcel.Cell + parcel.Direction.ToOffset();

            if (!PuzzleGrid.IsInside(currentLevel, parcel.TargetCell))
            {
                Lose("A parcel left the board.");
                return;
            }

            if (!boardView.ContainsTile(parcel.TargetCell))
            {
                Lose("A parcel ran off the conveyor path.");
                return;
            }

            parcel.From = parcel.Root.transform.position;
            parcel.To = PuzzleGrid.CellToWorld(currentLevel, parcel.TargetCell) + Vector3.up * PuzzleGrid.ParcelHeight;
            parcel.Progress = 0f;
            parcel.Moving = true;
        }

        private void ArriveAtTarget(ParcelRuntime parcel)
        {
            parcel.Cell = parcel.TargetCell;
            parcel.Root.transform.position = parcel.To;
            parcel.Moving = false;
            parcel.Steps++;

            if (parcel.Steps > currentLevel.StepLimit)
            {
                Lose("The route looped for too long.");
                return;
            }

            TileRuntime tile;
            if (!boardView.TryGetTile(parcel.Cell, out tile))
            {
                Lose("A parcel reached empty floor.");
                return;
            }

            switch (tile.Definition.Kind)
            {
                case TileKind.End:
                    HandleGateArrival(parcel, tile);
                    break;
                case TileKind.Blocker:
                    parcel.Direction = parcel.Direction.Opposite();
                    FlashStatus("Blocker reversed a parcel.");
                    StartSegment(parcel);
                    break;
                case TileKind.Teleporter:
                    HandleTeleporterArrival(parcel, tile);
                    break;
                default:
                    parcel.Direction = GetTileOutput(tile);
                    StartSegment(parcel);
                    break;
            }
        }

        private void HandleGateArrival(ParcelRuntime parcel, TileRuntime tile)
        {
            if (tile.Definition.GateColor != parcel.Color)
            {
                Lose("Wrong color gate. Restarting level.", true);
                return;
            }

            parcel.Delivered = true;
            parcel.Root.SetActive(false);
            FlashStatus(parcel.Color + " parcel delivered.");

            for (int i = 0; i < parcels.Count; i++)
            {
                if (!parcels[i].Delivered)
                {
                    return;
                }
            }

            Win();
        }

        private void HandleTeleporterArrival(ParcelRuntime parcel, TileRuntime source)
        {
            TileRuntime destination = boardView.FindTeleporterPair(source);
            if (destination == null)
            {
                Lose("Teleporter pair is missing.");
                return;
            }

            parcel.Cell = destination.Definition.Position;
            parcel.Root.transform.position = PuzzleGrid.CellToWorld(currentLevel, parcel.Cell) + Vector3.up * PuzzleGrid.ParcelHeight;
            parcel.Direction = GetTileOutput(destination);
            FlashStatus("Teleporter moved a parcel.");
            StartSegment(parcel);
        }

        private void ToggleReverseDrive()
        {
            reverseDrive = !reverseDrive;

            for (int i = 0; i < parcels.Count; i++)
            {
                ParcelRuntime parcel = parcels[i];
                if (parcel.Delivered)
                {
                    continue;
                }

                parcel.Direction = parcel.Direction.Opposite();
                if (parcel.Moving)
                {
                    parcel.From = parcel.Root.transform.position;
                    parcel.TargetCell = parcel.Cell;
                    parcel.To = PuzzleGrid.CellToWorld(currentLevel, parcel.Cell) + Vector3.up * PuzzleGrid.ParcelHeight;
                    parcel.Progress = 0f;
                }
            }

            boardView.UpdateAllTileAppearances(theme, reverseDrive, GetTileOutput);
            RefreshPreview();
            FlashStatus(reverseDrive ? "Reverse drive enabled." : "Reverse drive disabled.");
        }

        private GridDirection GetTileOutput(TileRuntime tile)
        {
            GridDirection direction = tile.Toggled ? tile.Definition.AlternateDirection : tile.Definition.Direction;

            if (reverseDrive &&
                (tile.Definition.Kind == TileKind.Conveyor ||
                 tile.Definition.Kind == TileKind.Start ||
                 tile.Definition.Kind == TileKind.Button))
            {
                direction = direction.Opposite();
            }

            return direction;
        }

        private void LoadNextLevel()
        {
            if (currentLevelIndex >= LevelLibrary.Levels.Count - 1)
            {
                LoadLevel(0);
                return;
            }

            LoadLevel(currentLevelIndex + 1);
        }

        private void RestartLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        private void LoadLevel(int index)
        {
            currentLevelIndex = Mathf.Clamp(index, 0, LevelLibrary.Levels.Count - 1);
            currentLevel = LevelLibrary.Levels[currentLevelIndex];
            validationReport = LevelValidator.Validate(currentLevel);
            reverseDrive = false;
            phase = GamePhase.Planning;
            statusText = "Plan the route, then run.";
            statusFlashTime = 0f;
            restartCountdown = -1f;

            if (!validationReport.IsValid)
            {
                Debug.LogWarning(validationReport.ToMultilineString(currentLevel.Name));
            }

            boardView.Rebuild(currentLevel, theme, reverseDrive, GetTileOutput);
            SpawnParcels();
            RefreshPreview();
            PositionCamera();
        }

        private void SpawnParcels()
        {
            parcels.Clear();
            parcelView.Clear();

            for (int i = 0; i < currentLevel.Parcels.Length; i++)
            {
                ParcelSpawn spawn = currentLevel.Parcels[i];
                TileRuntime startTile;
                if (!boardView.TryGetTile(spawn.Position, out startTile))
                {
                    Debug.LogWarning("Parcel spawn has no tile at " + spawn.Position);
                    continue;
                }

                ParcelRuntime parcel = parcelView.CreateParcel(currentLevel, spawn, GetTileOutput(startTile), theme);
                parcels.Add(parcel);
            }
        }

        private void RefreshPreview()
        {
            if (phase != GamePhase.Planning)
            {
                return;
            }

            List<Vector2Int> previewPath = new List<Vector2Int>();
            for (int i = 0; i < currentLevel.Parcels.Length; i++)
            {
                AppendPreviewPath(currentLevel.Parcels[i], previewPath);
            }

            boardView.SetPreview(currentLevel, previewPath, theme);
        }

        private void AppendPreviewPath(ParcelSpawn spawn, List<Vector2Int> previewPath)
        {
            TileRuntime startTile;
            if (!boardView.TryGetTile(spawn.Position, out startTile))
            {
                return;
            }

            Vector2Int cell = spawn.Position;
            GridDirection direction = GetTileOutput(startTile);
            previewPath.Add(cell);

            for (int step = 0; step < currentLevel.StepLimit; step++)
            {
                Vector2Int nextCell = cell + direction.ToOffset();
                if (!PuzzleGrid.IsInside(currentLevel, nextCell))
                {
                    return;
                }

                TileRuntime tile;
                if (!boardView.TryGetTile(nextCell, out tile))
                {
                    return;
                }

                cell = nextCell;
                previewPath.Add(cell);

                switch (tile.Definition.Kind)
                {
                    case TileKind.End:
                        return;
                    case TileKind.Blocker:
                        direction = direction.Opposite();
                        break;
                    case TileKind.Teleporter:
                        TileRuntime destination = boardView.FindTeleporterPair(tile);
                        if (destination == null)
                        {
                            return;
                        }

                        cell = destination.Definition.Position;
                        previewPath.Add(cell);
                        direction = GetTileOutput(destination);
                        break;
                    default:
                        direction = GetTileOutput(tile);
                        break;
                }
            }
        }

        private void Win()
        {
            if (currentLevelIndex == LevelLibrary.Levels.Count - 1)
            {
                phase = GamePhase.Complete;
                statusText = "Prototype complete. All levels cleared.";
                return;
            }

            phase = GamePhase.Won;
            statusText = "Level clear. Continue to the next level.";
        }

        private void Lose(string reason)
        {
            Lose(reason, false);
        }

        private void Lose(string reason, bool autoRestart)
        {
            phase = GamePhase.Lost;
            statusText = reason;
            statusFlashTime = 1.5f;
            restartCountdown = autoRestart ? 1.2f : -1f;
        }

        private void FlashStatus(string message)
        {
            statusText = message;
            statusFlashTime = 1.5f;
        }

        private void PositionCamera()
        {
            if (gameCamera == null)
            {
                return;
            }

            gameCamera.orthographic = true;
            gameCamera.orthographicSize = Mathf.Max(currentLevel.Width, currentLevel.Height) * 0.72f;
            gameCamera.transform.position = new Vector3(0f, 10.5f, -0.4f);
            gameCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
        }

        private void EnsureSceneEssentials()
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                gameCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            sceneLight = FindAnyObjectByType<Light>();
            if (sceneLight == null)
            {
                GameObject lightObject = new GameObject("Directional Light");
                sceneLight = lightObject.AddComponent<Light>();
                sceneLight.type = LightType.Directional;
                sceneLight.intensity = 1.2f;
            }

            sceneLight.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        }

    }
}
