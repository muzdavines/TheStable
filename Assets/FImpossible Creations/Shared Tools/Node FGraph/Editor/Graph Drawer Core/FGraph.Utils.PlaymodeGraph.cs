using UnityEngine;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        protected bool isAnimating = false;
        protected bool displayPlaymodeGraph = false;

        protected virtual void PreparePlaymodeNodes()
        {
            // Refresh nodes for playmode debugging
            if (displayedGraphSetup != ProjectFilePreset)
            {
                displayPlaymodeGraph = true;

                if (nodesFrom != displayedGraphSetup)
                {
                    nodesFrom = null;
                    GetAllNodes();
                }
            }
        }

        public ScriptableObject displayedGraphSetup
        {
            get
            {
                // In editor mode display just project file preset file graph
                if (!Application.isPlaying) return ProjectFilePreset;
                // If want to draw null, choose the project file graph
                if (DebugDrawPreset == null) return ProjectFilePreset;
                // If want to draw preset which is instantiation of project file then allow
                if (CanDrawDebugPreset) return DebugDrawPreset;
                return ProjectFilePreset;
            }
        }


    }
}