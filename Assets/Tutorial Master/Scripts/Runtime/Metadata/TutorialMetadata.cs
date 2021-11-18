namespace HardCodeLab.TutorialMaster.Runtime.Metadata
{
    /// <summary>
    /// Used to store runtime tutorial metadata of Tutorial Master at its current state
    /// </summary>
    public class TutorialMetadata
    {
        /// <summary>
        /// Index of a currently playing tutorial.
        /// </summary>
        public readonly int TutorialIndex;

        /// <summary>
        /// Name of a currently playing tutorial.
        /// </summary>
        public readonly string TutorialName;

        /// <summary>
        /// Index of a currently playing stage.
        /// </summary>
        public readonly int StageIndex;

        /// <summary>
        /// Name of a currently playing stage.
        /// </summary>
        public readonly string StageName;

        /// <summary>
        /// Creates current tutorial snapshot metadata.
        /// </summary>
        /// <param name="manager">The manager from which the metadata will be based upon.</param>
        /// <returns>Returns generated tutorial metadata.</returns>
        public static TutorialMetadata Create(TutorialMasterManager manager)
        {
            if (!manager.IsPlaying)
                return null;

            return new TutorialMetadata(manager.ActiveTutorialIndex,
                manager.ActiveTutorial.Name,
                manager.ActiveTutorial.ActiveStageIndex,
                manager.ActiveStage.Name);
        }

        private TutorialMetadata(int tutorialIndex, string tutorialName, int stageIndex, string stageName)
        {
            TutorialIndex = tutorialIndex;
            TutorialName = tutorialName;
            StageIndex = stageIndex;
            StageName = stageName;
        }

        public override string ToString()
        {
            return string.Format("Tutorial \"{0}\" ({1}) at: \"{2} ({3}\"",
                TutorialName, TutorialIndex, StageName, StageIndex);
        }
    }
}