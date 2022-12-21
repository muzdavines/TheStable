#if UNITY_EDITOR
using FIMSpace.FEditor;
using System;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        [NonSerialized] public bool _EditorConstantPreview = false;
        Color preG;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                if (_EditorConstantPreview)
                {
                    PipePreviewGeneration();
                    DrawGizmos();
                }

                //Handles.SphereHandleCap(0,transform.position, transform.rotation, 0.25f, EventType.Repaint);
                if (_EditorCategory != EEditorState.Setup || !Selection.Contains(gameObject.GetInstanceID()))
                FGUI_Handles.DrawArrow(transform.position, transform.rotation, 0.5f, 0f, 1f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_EditorConstantPreview) return;
            DrawGizmos();
        }

        void DrawGizmos()
        {
            preG = Gizmos.color;
            Color preH = Handles.color;
            Gizmos.matrix = Matrix4x4.identity;


            Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
            Gizmos.DrawRay(transform.position - transform.forward * MaxDistanceToEnding * 0.5f + transform.up, transform.forward * MaxDistanceToEnding);

            if (endAlignHelperHit.transform)
            {
                Gizmos.DrawRay(endAlignHelperHit.point - Vector3.up * 0.5f, Vector3.up);
                Gizmos.DrawRay(endAlignHelperHit.point - Vector3.right * 0.5f, Vector3.right);
                Gizmos.DrawRay(endAlignHelperHit.point - Vector3.forward * 0.5f, Vector3.forward);
                Gizmos.DrawRay(endAlignHelperHit.point, endAlignHelperHit.normal);
            }
            else if (CustomEndingPosition != null)
            {
                Gizmos.DrawRay(CustomEndingPosition.Value - Vector3.up * 0.5f, Vector3.up);
                Gizmos.DrawRay(CustomEndingPosition.Value - Vector3.right * 0.5f, Vector3.right);
                Gizmos.DrawRay(CustomEndingPosition.Value - Vector3.forward * 0.5f, Vector3.forward);
            }

            if (startAlignHelperHit.transform)
            {
                Gizmos.DrawRay(startAlignHelperHit.point - Vector3.up * 0.5f, Vector3.up);
                Gizmos.DrawRay(startAlignHelperHit.point - Vector3.right * 0.5f, Vector3.right);
                Gizmos.DrawRay(startAlignHelperHit.point - Vector3.forward * 0.5f, Vector3.forward);
                Gizmos.DrawRay(startAlignHelperHit.point, startAlignHelperHit.normal);
            }


            if (_EditorCategory != EEditorState.Setup) // Drawing generation preview
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
                Gizmos.DrawLine(transform.position, EndPosition);
                Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
                Gizmos.DrawSphere(EndPosition, 0.1f);


                Gizmos.color = new Color(0.1f, 0.9f, 0.2f, 0.5f);

                for (int i = 0; i < allSpawns.Count; i++)
                {
                    var s = allSpawns[i];
                    if (s == null) { UnityEngine.Debug.Log("End On Null at " + s); break; }
                    if (s.ToCreate == null || s.PreviewMesh == null) continue;
                    Gizmos.DrawMesh(s.PreviewMesh, 0, s.Position, s.Rotation, s.Scale);
                }

                FGUI_Handles.DrawArrow(transform.position, transform.rotation, 0.5f, 0f, 1f);
                FGUI_Handles.DrawArrow(EndPosition, Quaternion.LookRotation(EndDirection), 0.5f, 0f, 1f);

                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * BoxcastScale);
            }
            else // Pipe segments setup
            {
                if (PresetData._editorSelected > PresetData.Segments.Count - 1) PresetData._editorSelected = -1;

                if (PresetData._editorSelected != -1)
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Handles.matrix = transform.localToWorldMatrix;

                    PipeSegmentSetup s;
                    if (PresetData._editorSelected >= 0) s = PresetData.Segments[PresetData._editorSelected];
                    else s = PresetData._editorSelected == -2 ? PresetData.OptionalUnended : PresetData.OptionalEndCap;

                    if (s != null)
                    {
                        //Vector3 drawOrig = Vector3.right * s.ReferenceScale * 0.4f;
                        //Handles.color = new Color(0f, 0f, 1f, 1f);
                        //FGUI_Handles.DrawArrow(drawOrig, Quaternion.LookRotation(s.ModelForward, s.ModelUpAxis), s.ReferenceScale * 0.5f, 0f);
                        //Handles.color = new Color(0f, 1f, 0f, 1f);
                        //FGUI_Handles.DrawArrow(drawOrig, Quaternion.LookRotation(s.ModelUpAxis, s.ModelUpAxis), s.ReferenceScale * 0.5f, 0f);

                        //Handles.color = preH;
                        //Handles.Label(drawOrig + s.ModelForward, new GUIContent("[Model Forward]"));
                        //Handles.Label(drawOrig + s.ModelUpAxis, new GUIContent("[Model Up Axis]"));

                        Gizmos.color = new Color(0.1f, 0.9f, 0.2f, 0.5f);
                        if (s.PreviewMesh != null) Gizmos.DrawMesh(s.PreviewMesh, 0);
                    }
                }
            }

            Handles.matrix = Matrix4x4.identity;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = preG;
        }

    }
}
#endif
