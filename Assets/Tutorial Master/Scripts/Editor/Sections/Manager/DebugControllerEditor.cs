using UnityEditor;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.EditorUI
{
    public sealed class DebugControllerEditor : Section
    {
        private int _targetTutorialId;

        public DebugControllerEditor(ref TutorialMasterEditor editor, string tooltipContent = "")
            : base(ref editor, "Debug Mode", tooltipContent)
        {
        }

        private bool IsPlaying
        {
            get
            {
                return Editor.TutorialManager.IsPlaying;
            }
        }

        /// <summary>
        /// Gets the tutorial names.
        /// </summary>
        /// <value>
        /// The tutorial names as well as their index numbering alongside.
        /// </value>
        private string[] TutorialNames
        {
            get
            {
                string[] tutorialNames = new string[Editor.TutorialManager.Tutorials.Count];

                for (int i = 0; i < Editor.TutorialManager.Tutorials.Count; i++)
                {
                    tutorialNames[i] = string.Format("{0} - {1}", i, Editor.TutorialManager.Tutorials[i].Name);
                }

                return tutorialNames;
            }
        }

        private float TutorialProgress
        {
            get
            {
                return Editor.TutorialManager.TutorialProgress;
            }
        }

        protected override void OnSectionGUI()
        {
            RenderTutorialControls();
            RenderStageControls();
            RenderTutorialInfo();
            RenderTutorialProgress();
        }

        private void NextStage()
        {
            Editor.TutorialManager.NextStage();
        }

        private void PrevStage()
        {
            Editor.TutorialManager.PrevStage();
        }

        private void RenderStageControls()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = IsPlaying;

            if (GUILayout.Button("Prev Stage"))
            {
                PrevStage();
            }

            if (GUILayout.Button("Next Stage"))
            {
                NextStage();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void RenderTutorialControls()
        {
            _targetTutorialId = EditorGUILayout.Popup("Tutorials", _targetTutorialId, TutorialNames);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !IsPlaying;

            if (GUILayout.Button("Start Tutorial"))
            {
                StartTutorial(_targetTutorialId);
            }

            GUI.enabled = IsPlaying;

            if (GUILayout.Button("Stop Tutorial"))
            {
                StopTutorial();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void RenderTutorialInfo()
        {
            if (!IsPlaying) return;
            Tutorial cTutorial = Editor.TutorialManager.ActiveTutorial;
            if (cTutorial == null) return;

            EditorGUILayout.LabelField(
                string.Format("Active Tutorial: \"{0}\"", cTutorial.Name),
                EditorStyles.boldLabel);
        }

        private void RenderTutorialProgress()
        {
            if (!IsPlaying) return;
            Tutorial cTutorial = Editor.TutorialManager.ActiveTutorial;
            if (cTutorial == null) return;

            int maxStages = cTutorial.Stages.Count;
            int cStageNumber = cTutorial.ActiveStageIndex + 1;

            Rect rectPos = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(
                rectPos,
                TutorialProgress,
                string.Format(
                    "Tutorial Progress: {0}/{1} ({2}%)",
                    cStageNumber,
                    maxStages,
                    (int)(TutorialProgress * 100)));
            GUILayout.Space(18);
            EditorGUILayout.EndVertical();

            if (cTutorial.ActiveStage.Trigger.Type != TriggerType.None)
            {
                string timerInfo;

                if (cTutorial.ActiveStage.Trigger.Type != TriggerType.Timer)
                {
                    float cTimer = cTutorial.ActiveStage.Trigger.TriggerActivationDelay
                                   - cTutorial.ActiveStage.Trigger.CurrentInputDelayTimer;

                    timerInfo = cTimer > 0
                                    ? string.Format("Input Delay Timer: {0}s", cTimer)
                                    : "Input Delay Timer: ELAPSED";
                }
                else
                {
                    float cTimer = cTutorial.ActiveStage.Trigger.TriggerTimerAmount
                                   - cTutorial.ActiveStage.Trigger.CurrentTriggerTimer;

                    timerInfo = cTimer > 0 ? string.Format("Trigger Timer: {0}s", cTimer) : "Trigger Timer: ELAPSED";
                }

                EditorGUILayout.LabelField(timerInfo, EditorStyles.miniLabel);
            }
        }

        private void StartTutorial(int tutorialId)
        {
            Editor.TutorialManager.StartTutorial(tutorialId);
        }

        private void StopTutorial()
        {
            Editor.TutorialManager.StopTutorial();
        }
    }
}