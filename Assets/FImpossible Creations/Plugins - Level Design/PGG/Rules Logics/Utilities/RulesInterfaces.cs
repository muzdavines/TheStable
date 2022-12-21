
using UnityEngine;

namespace FIMSpace.Generating.Rules
{
    /// <summary>
    /// Interface helping indentifying role of spawn rules
    /// </summary>
    public interface ISpawnProcedureType
    {
        SpawnRuleBase.EProcedureType Type { get; }
    }

    /// <summary>
    /// Consmetical interface
    /// </summary>
    public interface ISpawnProceduresDecorator
    {
#if UNITY_EDITOR
        GUIStyle DisplayStyle { get; }
        int DisplayHeight { get; }
        int UpPadding { get; }
        int DownPadding { get; }
        Color DisplayColor { get; }
#endif
    }

    /// <summary>
    /// Interface implementing custom variable output
    /// </summary>
    //public interface ISpawnOutputtable
    //{
    //    bool IsOutputting { get; }
    //    object Out { get; }
    //}
}