#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        public enum EEditorState { Setup, Adjust, Extra, All }
        [HideInInspector] public EEditorState _EditorCategory = EEditorState.Setup;
        public void _EditorSetPreset(PipePreset p) { projectPreset = p; }
        public PipePreset _EditorGetProjectPreset() { return projectPreset; }
    }

}
#endif
