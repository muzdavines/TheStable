#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow
    {
        Mesh generatedMesh = null;
        Rect _editorRect = new Rect();

        public bool useScenePreview = false;
        public GameObject sceneModelPreview = null;


        bool _foldout_meshEditPreview = true;
        bool _foldout_bigPreview = false;
        int gridSnapping = 0;


        bool shapeChanged = false;
        bool shapeEndChanging = false;
        bool autoRefresh = true;
        bool autoRefreshFull = true;


        private TileMeshSetup.CurvePoint switchSelPointFlag = null;
        private List<TileMeshSetup.CurvePoint> editingCurve = null;
        private TileMeshSetup.CurvePoint selectedPoint = null;
        private bool isSelectingMultiple = false;
        private int wasSelectingMultiple = -1;
        private Rect selectionBox = new Rect();
        private List<TileMeshSetup.CurvePoint> dragSelectionOn;
        private List<TileMeshSetup.CurvePoint> selectedPoints = new List<TileMeshSetup.CurvePoint>();
        private int selectedPointListOwner = -1;
        TilePreviewWindow tileMeshPreviewEditor = null;
        private Vector2 dragSelectingMultipleStart = Vector2.zero;


        private void DragSelecting(List<TileMeshSetup.CurvePoint> list)
        {
            if (isSelectingMultiple == false)
            {
                dragSelectionOn = list;
                isSelectingMultiple = true;
                dragSelectingMultipleStart = Event.current.mousePosition;
                Event.current.Use();
                wasSelectingMultiple = 20;
            }
            else
            {
                wasSelectingMultiple = 20;
            }
        }

        private void EndDragSelecting()
        {
            if (isSelectingMultiple)
            {
                isSelectingMultiple = false;
                selectionBox = new Rect();
                wasSelectingMultiple = 20;
                Event.current.Use();
            }
        }



        #region Editor Helpers



        #endregion

    }
}
#endif