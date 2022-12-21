using UnityEditor;

namespace FIMSpace.Graph
{
    public abstract partial class FGraphDrawerBase
    {
        // Delta time for window in editor

        protected float dt = 0.1f;
        double lastUpdateTime = 0f;
        protected bool _dtWasUpdating = false;
        protected bool _dtForcingUpdate = false;

        protected virtual void UpdateEditorDeltaTime()
        {
            if (_dtForcingUpdate)
            {
                if (!_dtWasUpdating)
                {
                    lastUpdateTime = EditorApplication.timeSinceStartup;
                    _dtWasUpdating = true;
                }
            }
            else
            {
                _dtWasUpdating = false;
            }

            if (_dtWasUpdating)
            {
                dt = (float)(EditorApplication.timeSinceStartup - lastUpdateTime);
                lastUpdateTime = EditorApplication.timeSinceStartup;
            }

            _dtForcingUpdate = false;
        }

    }
}