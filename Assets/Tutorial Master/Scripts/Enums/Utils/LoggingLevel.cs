using System;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Specifies the level of logs that should be displayed.
    /// </summary>
    [Serializable]
    public enum LoggingLevel
    {
        None,
        WarningsOnly,
        ErrorsOnly,
        WarningsAndErrors,
        Full
    }
}