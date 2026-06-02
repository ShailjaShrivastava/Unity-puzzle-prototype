using UnityEngine;

namespace ConveyorPuzzle
{
    public enum HudCommand
    {
        None,
        Run,
        Pause,
        Restart,
        PreviousLevel,
        NextLevel
    }

    public struct HudModel
    {
        public string LevelName;
        public string GoalText;
        public string StatusText;
        public string ValidationText;
        public GamePhase Phase;
        public bool ReverseDrive;
        public int LevelIndex;
        public int LevelCount;
        public int DeliveredCount;
        public int ParcelCount;
        public int TotalSteps;
        public int StepLimit;
        public float StatusFlashTime;
    }

    public sealed class HudView
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle statusStyle;
        private GUIStyle smallStyle;

        public HudCommand Draw(HudModel model)
        {
            EnsureStyles();
            statusStyle.normal.textColor = model.StatusFlashTime > 0f ? new Color(1f, 0.78f, 0.32f) : Color.white;

            HudCommand command = HudCommand.None;

            GUILayout.BeginArea(new Rect(14f, 14f, 430f, 246f), GUI.skin.box);
            GUILayout.Label(model.LevelName, titleStyle);
            GUILayout.Label("Goal: " + model.GoalText, bodyStyle);
            GUILayout.Label(BuildStateLine(model), bodyStyle);
            GUILayout.Label(BuildProgressLine(model), bodyStyle);
            GUILayout.Label(model.ValidationText, smallStyle);
            GUILayout.Label(model.StatusText, statusStyle);

            GUILayout.BeginHorizontal();
            if (model.Phase == GamePhase.Planning || model.Phase == GamePhase.Paused)
            {
                if (GUILayout.Button("Run", GUILayout.Height(30f)))
                {
                    command = HudCommand.Run;
                }
            }
            else if (model.Phase == GamePhase.Running)
            {
                if (GUILayout.Button("Pause", GUILayout.Height(30f)))
                {
                    command = HudCommand.Pause;
                }
            }

            if (GUILayout.Button("Restart", GUILayout.Height(30f)))
            {
                command = HudCommand.Restart;
            }

            GUI.enabled = model.LevelIndex > 0;
            if (GUILayout.Button("Previous", GUILayout.Height(30f)))
            {
                command = HudCommand.PreviousLevel;
            }

            GUI.enabled = model.Phase == GamePhase.Won || model.Phase == GamePhase.Complete;
            if (GUILayout.Button("Next", GUILayout.Height(30f)))
            {
                command = HudCommand.NextLevel;
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Label("Click SW forks or REV buttons. Space run/pause, R restart, N next, 1-3 levels.", smallStyle);
            GUILayout.EndArea();

            return command;
        }

        private string BuildStateLine(HudModel model)
        {
            string reverseText = model.ReverseDrive ? "Reverse on" : "Reverse off";
            return "Level " + (model.LevelIndex + 1) + "/" + model.LevelCount + " | " + model.Phase + " | " + reverseText;
        }

        private string BuildProgressLine(HudModel model)
        {
            return "Delivered " + model.DeliveredCount + "/" + model.ParcelCount +
                   " | Steps " + model.TotalSteps + "/" + model.StepLimit;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            titleStyle.normal.textColor = Color.white;

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true
            };
            bodyStyle.normal.textColor = new Color(0.9f, 0.92f, 0.95f);

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true
            };
            smallStyle.normal.textColor = new Color(0.72f, 0.78f, 0.84f);

            statusStyle = new GUIStyle(bodyStyle)
            {
                fontStyle = FontStyle.Bold
            };
            statusStyle.normal.textColor = Color.white;
        }
    }
}
