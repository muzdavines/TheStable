using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField3D
    {

        public void DrawFieldGizmos(bool setMatrix = true, bool drawSphere = true)
        {
            Matrix4x4 preMx = Gizmos.matrix;
            if (setMatrix) Gizmos.matrix = preMx * Matrix;

            float scale = 1f;
            Vector3 drawScale = new Vector3(scale, scale * 0.1f, scale) * 0.95f;// * scale * 0.92f;

            Vector3 startPos = Vector3.zero;//RootPosition * scale;
            Color preCol = Gizmos.color;

            #region Bounds Mode Draw
            if (UseBounds && Bounding.Count > 0)
            {
                for (int i = 0; i < Bounding.Count; i++)
                {
                    Vector3 sze = Bounding[i].size;
                    sze.y *= 0.1f;
                    Gizmos.DrawCube(startPos + Bounding[i].center * scale, sze * scale);
                }
            }
            #endregion
            else // Cell Mode Draw
            {
                for (int i = 0; i < Grid.AllApprovedCells.Count; i++)
                {
                    //Gizmos.DrawWireCube(startPos + Grid.AllApprovedCells[i].Pos.V3IntToV3() * scale, drawScale);
                    if (Grid.AllApprovedCells[i].IsGhostCell) Gizmos.color = new Color(preCol.r, preCol.g, preCol.b, preCol.a * 0.65f);
                    Gizmos.DrawCube(startPos + Grid.AllApprovedCells[i].Pos.V3IntToV3() * scale, drawScale);
                    if (Grid.AllApprovedCells[i].IsGhostCell) Gizmos.color = preCol;
                }
            }

            if (drawSphere) Gizmos.DrawSphere(startPos, scale * 0.25f);

            if (setMatrix) Gizmos.matrix = preMx;
        }

        public bool DrawFieldHandles(float scaleUp = 1f)
        {
            bool clicked = false;
            float scale = scaleUp;
            //Vector3 drawScale = new Vector3(scale, scale * 0.1f, scale) * 0.92f;
            Vector3 startPos = Vector3.zero;//RootPosition * scale;

#if UNITY_EDITOR

            if (UnityEditor.Handles.Button(Matrix.MultiplyPoint(startPos), RootRotation, scale * 0.5f, scale * 0.3f, UnityEditor.Handles.SphereHandleCap))
            {
                //Event.current.Use();
                clicked = true;
            }
#endif

            return clicked;
        }

        public void DrawFieldGizmosBounding()
        {
            //if (setMatrix) Gizmos.matrix = Matrix;

            Bounds b = GetFullBoundsWorldSpace();
            Gizmos.DrawWireCube(b.center, b.size);

            //if (setMatrix) Gizmos.matrix = Matrix4x4.identity;
        }
    }
}