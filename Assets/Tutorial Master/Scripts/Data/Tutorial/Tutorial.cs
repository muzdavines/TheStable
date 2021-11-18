using System;
using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores the data and logic of a tutorial.
    /// </summary>
    [Serializable]
    [DataValidator(typeof(TutorialValidator))]
    public class Tutorial
    {
        /// <summary>
        /// The validator which is used to validate this Tutorial.
        /// </summary>
        private static readonly DataValidator Validator 
            = DataValidatorResolver.Resolve<Tutorial>();

        /// <summary>
        /// Tutorial Events.
        /// </summary>
        public TutorialEvents Events;

        /// <summary>
        /// Name of this Tutorial.
        /// </summary>
        public string Name;

        /// <summary>
        /// Specifies whether to run a data validator check before running this Tutorial (enabled by default).
        /// </summary>
        public bool DataValidatorEnabled = true;

        /// <summary>
        /// Stages that this Tutorial.
        /// </summary>
        public List<Stage> Stages;

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
                if (ActiveStageIndex > -1 && ActiveStageIndex < Stages.Count)
                {
                    return Stages[ActiveStageIndex];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current Id of a currently playing stage.
        /// </summary>
        /// <value>
        /// The Id of a currently playing stage.
        /// </value>
        public int ActiveStageIndex { get; private set; }

        /// <summary>
        /// Gets the unique identifier of this Tutorial.
        /// </summary>
        /// <value>
        /// The string representing the Id.
        /// </value>
        public string Id { get; private set; }

        /// <summary>
        /// Checks whether a current stage is being played.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this stage is playing; otherwise, <c>false</c>.
        /// </value>
        public bool IsPlaying
        {
            get
            {
                return ActiveStage != null && ActiveStage.IsPlaying;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tutorial"/> class.
        /// </summary>
        public Tutorial()
        {
            ActiveStageIndex = -1;
            Name = "Untitled Tutorial";
            Stages = new List<Stage>();
            Events = new TutorialEvents();
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Plays the next enabled stage of this tutorial.
        /// If there is none left, the current stage is stopped and tutorial is declared as stopped.
        /// </summary>
        public void NextStage()
        {
            if (!IsPlaying) 
                return;

            for (int newStageId = ActiveStageIndex + 1; newStageId < Stages.Count; newStageId++)
            {
                if (Stages[newStageId].IsEnabled)
                {
                    SetStage(newStageId);
                    return;
                }
            }

            Stop();
        }

        /// <summary>
        /// Goes to the previous stage of this tutorial if there is one.
        /// </summary>
        public void PrevStage()
        {
            if (!IsPlaying) 
                return;

            for (int newStageId = ActiveStageIndex - 1; newStageId > -1; newStageId--)
            {
                if (Stages[newStageId].IsEnabled)
                {
                    SetStage(newStageId);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the stage based on its index and plays it. A currently played stage will be stopped before jumping to different stage.
        /// </summary>
        /// <param name="stageIndex">The Id of a stage which will be used. If it's set to -1, then current stage is stopped.</param>
        public void SetStage(int stageIndex)
        {
            if (stageIndex >= Stages.Count) 
                return;

            if (stageIndex > -1)
            {
                if (IsPlaying)
                {
                    ActiveStage.Stop();
                }

                ActiveStageIndex = stageIndex;
                ActiveStage.Play();
            }
            else
            {
                ActiveStage.Stop();
                ActiveStageIndex = -1;
            }
        }

        /// <summary>
        /// Starts this tutorial.
        /// </summary>
        public void Start()
        {
            if (IsPlaying) 
                return;

            Events.OnTutorialEnter.Invoke(this);

            if (!Validator.Validate(this))
            {
                TMLogger.LogError(string.Format("Failed to execute a tutorial \"{0}\".", Name));
                TMLogger.LogValidationErrors(Validator);
                return;
            }

            for (int newStageId = 0; newStageId < Stages.Count; newStageId++)
            {
                if (Stages[newStageId].IsEnabled)
                {
                    SetStage(newStageId);
                    break;
                }
            }

            Events.OnTutorialStart.Invoke(this);
        }

        /// <summary>
        /// Stops this tutorial
        /// </summary>
        public void Stop()
        {
            if (!IsPlaying) 
                return;

            SetStage(-1);
            Events.OnTutorialEnd.Invoke(this);
        }
    }
}