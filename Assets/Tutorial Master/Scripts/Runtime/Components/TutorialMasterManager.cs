using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HardCodeLab.TutorialMaster.Localization;

namespace HardCodeLab.TutorialMaster
{
    /// <inheritdoc />
    /// <summary>
    /// Holds relevant settings and information about tutorials and manages them at runtime.
    /// </summary>
    /// <seealso cref="T:UnityEngine.MonoBehaviour" />
    [DisallowMultipleComponent]
    [HelpURL("https://support.hardcodelab.com/tutorial-master/2.0/")]
    [AddComponentMenu("Tutorial Master/Tutorial Master Manager")]
    [DataValidator(typeof(TutorialMasterManagerValidator))]
    public class TutorialMasterManager : MonoBehaviour
    {
        /// <summary>
        /// Stores all Tutorials.
        /// </summary>
        public List<Tutorial> Tutorials = new List<Tutorial>();

        /// <summary>
        /// Pool data for Arrow Module
        /// </summary>
        public ArrowPool ArrowModulePool = new ArrowPool();

        /// <summary>
        /// Pool data for Highlighter Module
        /// </summary>
        public HighlighterPool HighlighterModulePool = new HighlighterPool();

        /// <summary>
        /// Pool data for Image Module
        /// </summary>
        public ImagePool ImageModulePool = new ImagePool();

        /// <summary>
        /// Pool data for Pop-Up Module
        /// </summary>
        public PopupPool PopupModulePool = new PopupPool();

        /// <summary>
        /// The Localization Data of this Tutorial Manager.
        /// </summary>
        public LocalizationData LocalizationData = new LocalizationData();

        /// <summary>
        /// If true, then a tutorial of specified index will play when Scene starts.
        /// </summary>
        public bool PlayOnStart = false;

        /// <summary>
        /// Index of a tutorial which will play first when the Scene starts.
        /// </summary>
        public int StartingTutorialIndex = 0;

        /// <summary>
        /// Invoked whenever a current language is changed.
        /// </summary>
        public UnityEvent OnLanguageChanged;

        /// <summary>
        /// Gets the stage which is being currently played.
        /// </summary>
        /// <value>
        /// The object representing a currently played stage.
        /// </value>
        public Stage ActiveStage
        {
            get
            {
                return ActiveTutorial != null
                    ? ActiveTutorial.ActiveStage
                    : null;
            }
        }

        /// <summary>
        /// Gets the tutorial which is being currently played.
        /// </summary>
        /// <value>
        /// The object representing a currently played tutorial.
        /// </value>
        public Tutorial ActiveTutorial
        {
            get
            {
                return ActiveTutorialIndex > -1
                  ? Tutorials[ActiveTutorialIndex]
                  : null;
            }
        }

        /// <summary>
        /// Gets the current index of a currently playing tutorial
        /// </summary>
        /// <value>
        /// The Id of a currently playing tutorial.
        /// </value>
        public int ActiveTutorialIndex { get; private set; }

        /// <summary>
        /// Gets the current index of a currently playing stage of a currently playing tutorial.
        /// </summary>
        /// <value>
        /// The index of the active stage.
        /// </value>
        public int ActiveStageIndex
        {
            get
            {
                if (ActiveTutorialIndex < 0)
                    return -1;

                return ActiveTutorial.ActiveStageIndex;
            }
        }

        /// <summary>
        /// Checks whether a current tutorial is being played.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this tutorial is playing; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying
        {
            get { return ActiveTutorial != null && ActiveTutorial.IsPlaying; }
        }

        /// <summary>
        /// Gets the progress of an active tutorial based on current stage number.
        /// </summary>
        /// <value>
        /// The value between 0 and 1 representing the progress.
        /// </value>
        public float TutorialProgress
        {
            get
            {
                if (!IsPlaying)
                    return 0;

                if (ActiveTutorial.Stages.Count == 0)
                    return 0;

                float progress = (float)(ActiveStageIndex + 1) / ActiveTutorial.Stages.Count;
                if (progress <= 0)
                    return 0;

                return progress;
            }
        }

        /// <summary>
        /// Jumps to a specific Stage of the tutorial.
        /// </summary>
        /// <param name="stageId">Id of the stage.</param>
        public void JumpStage(int stageId)
        {
            if (!IsPlaying)
                return;

            DeallocateModulePools();
            ActiveTutorial.SetStage(stageId);
            SetStageChanges();
        }

        /// <summary>
        /// Goes to the next stage of the current tutorial. If there is no next stage, tutorial is stopped.
        /// </summary>
        public void NextStage()
        {
            if (!IsPlaying)
                return;

            DeallocateModulePools();
            ActiveTutorial.NextStage();
            SetStageChanges();
        }

        /// <summary>
        /// Goes to the previous stage of the current tutorial unless there is no previous stage.
        /// </summary>
        public void PrevStage()
        {
            if (!IsPlaying)
                return;

            DeallocateModulePools();
            ActiveTutorial.PrevStage();
            SetStageChanges();
        }

        /// <summary>
        /// Sets the language of the current and future Stages
        /// </summary>
        /// <param name="languageIndex">Index of the language which will be set.</param>
        public void SetLanguage(int languageIndex = -1)
        {
            if (languageIndex > -1)
                LocalizationData.SetLanguage(languageIndex);

            if (!IsPlaying)
                return;

            if (!ActiveTutorial.IsPlaying)
                return;

            ActiveStage.Modules.SetLanguage(LocalizationData.LanguageId);
            OnLanguageChanged.Invoke();
        }

        /// <summary>
        /// Starts the tutorial.
        /// </summary>
        /// <param name="tutorialId">The Id of the tutorial which will start.</param>
        public void StartTutorial(int tutorialId = 0)
        {
            if (tutorialId >= Tutorials.Count)
                return;

            ActiveTutorialIndex = tutorialId;
            ActiveTutorial.Start();
            SetStageChanges();
        }

        /// <summary>
        /// Starts the tutorial at a given stage.
        /// </summary>
        /// <param name="tutorialId">The Id of the tutorial which will start.</param>
        /// <param name="stageId">The index of the stage which will start.</param>
        public void StartTutorial(int tutorialId, int stageId)
        {
            if (tutorialId >= Tutorials.Count)
                return;

            var stages = Tutorials[tutorialId].Stages;
            if (stageId >= stages.Count)
                return;

            ActiveTutorialIndex = tutorialId;
            ActiveTutorial.SetStage(stageId);
            SetStageChanges();
        }

        /// <summary>
        /// Stops a currently running tutorial.
        /// </summary>
        public void StopTutorial()
        {
            if (!IsPlaying)
                return;

            DeallocateModulePools();
            ActiveTutorial.Stop();
            ActiveTutorialIndex = -1;
        }

        protected virtual void Awake()
        {
            ArrowModulePool.Instantiate(ref Tutorials, transform);
            ImageModulePool.Instantiate(ref Tutorials, transform);
            HighlighterModulePool.Instantiate(ref Tutorials, transform);
            PopupModulePool.Instantiate(ref Tutorials, transform);

            ActiveTutorialIndex = -1;
        }

        /// <summary>
        /// Called only once.
        /// </summary>
        protected virtual void Start()
        {
            if (!PlayOnStart)
                return;

            StartTutorial(StartingTutorialIndex);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        protected virtual void Update()
        {
            if (IsPlaying)
            {
                CheckStageTrigger();
            }
        }

        /// <summary>
        /// Checks if the trigger of a current Stage has been activated.
        /// </summary>
        private void CheckStageTrigger()
        {
            if (!ActiveTutorial.IsPlaying)
                return;

            if (ActiveStage.IsPlaying)
            {
                ActiveStage.Trigger.Check(this);
            }
        }

        /// <summary>
        /// De-allocates the module pools by releasing them.
        /// </summary>
        private void DeallocateModulePools()
        {
            ArrowModulePool.FreeModulePool();
            ImageModulePool.FreeModulePool();
            HighlighterModulePool.FreeModulePool();
            PopupModulePool.FreeModulePool();
        }

        /// <summary>
        /// Sets the listeners for modules of a currently active stage.
        /// </summary>
        private void SetListeners()
        {
            if (!IsPlaying)
                return;

            if (!ActiveTutorial.IsPlaying)
                return;

            foreach (var popupConfig in ActiveStage.Modules.Popups)
            {
                if (!popupConfig.Enabled)
                    continue;

                popupConfig.Module.AssignButtonClickEvent(popupConfig.Settings.ButtonClickEvent, NextStage, StopTutorial);
            }
        }

        /// <summary>
        /// Updates a currently active stage.
        /// </summary>
        private void SetStageChanges()
        {
            SetLanguage();
            SetListeners();
        }
    }
}