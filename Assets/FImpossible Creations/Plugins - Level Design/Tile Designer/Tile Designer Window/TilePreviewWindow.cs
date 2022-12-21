#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace FIMSpace.Generating
{
    public class TilePreviewWindow : UnityEditor.Editor
    {
        private PreviewRenderUtility _previewRenderUtility;
        private Mesh _mesh;
        //private Mesh _subMesh = null;

        [SerializeField, HideInInspector] private Material _customMaterial = null;
        [SerializeField, HideInInspector] private Material _customSubMeshMaterial = null;
        [SerializeField, HideInInspector] private Material _previewMaterial;
        [SerializeField, HideInInspector] private Material _selectionMaterial;

        public Material PreviewMaterial
        {
            get
            {
                if (designPreview != null) if (designPreview.DefaultMaterial != null) return designPreview.DefaultMaterial;
                if (_customMaterial != null) return _customMaterial;
                if (_previewMaterial == null) _previewMaterial = UnityDefaultDiffuseMaterial;
                return _previewMaterial;
            }
        }

        public static Material UnityDefaultDiffuseMaterial { get { return new Material(Shader.Find("Diffuse")); } }
        private static Material _unityWireframeMaterial = null;
        private static Material UnityWireframeMaterial { get { if (_unityWireframeMaterial == null) _unityWireframeMaterial = new Material(Shader.Find("VR/SpatialMapping/Wireframe")); return _unityWireframeMaterial; } }

        [NonSerialized] public bool UseScroll = true;


        #region Editor Window Setup

        public override bool HasPreviewGUI()
        {
            ValidateData();
            return true;
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }


        void OnDestroy()
        {
            if (_previewRenderUtility != null) _previewRenderUtility.Cleanup();
        }

        private void OnDisable()
        {
            if (_previewRenderUtility != null) _previewRenderUtility.Cleanup();
        }


        #endregion

        public void UpdateMesh(Mesh m/*, bool clearSubmesh = true*/)
        {
            //if (clearSubmesh) _customSubMeshMaterial = null;
            //if (clearSubmesh) _subMesh = null;
            designPreview = null;
            _mesh = m;
        }

        //public void UpdateSubMesh(Mesh m) { _subMesh = m; }
        public void SetMaterial(Material m) { _customMaterial = m; }
        public void SetSubMeshMaterial(Material m) { _customSubMeshMaterial = m; }

        [NonSerialized] TileDesign designPreview = null;
        public void UpdateMesh(TileDesign editedDesign)
        {
            if (editedDesign.IsSomethingGenerated == false) return;
            designPreview = editedDesign;
        }


        private void ValidateData()
        {
            if (_previewRenderUtility == null)
            {
                _previewRenderUtility = new PreviewRenderUtility();
                _previewRenderUtility.camera.orthographic = false;
                _previewRenderUtility.camera.fieldOfView = 50f;
                _previewRenderUtility.camera.nearClipPlane = 0.001f;
                _previewRenderUtility.camera.farClipPlane = 1000f;
                _previewRenderUtility.lights[0].transform.rotation *= Quaternion.Euler(40f, 160f, 0f);
                _previewRenderUtility.lights[0].shadows = LightShadows.Hard;
                _previewRenderUtility.lights[0].shadowStrength = 1f;
                _previewRenderUtility.camera.transform.position = new Vector3(0, 1, -7);
                _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(-12, 155, 0);
            }

            if (_mesh == null) _mesh = target as Mesh;
        }

        Vector2 cameraOffset = new Vector2(0, 0);
        Vector2 sphericCamRot = new Vector2(-12, 155);
        float camDistance = 1f;

        [NonSerialized] public TileMeshSetup.TileMeshCombineInstance selectedInstance = null;

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            #region Refresh

            if (_previewRenderUtility == null)
            {
                ValidateData();
                return;
            }

            #endregion

            if (_mesh == null) return;

            camDistance = Mathf.Clamp(camDistance, 0.1f, 2.4f);
            sphericCamRot.x = Mathf.Clamp(sphericCamRot.x, -60f, 60f);

            cameraOffset.x = Mathf.Clamp(cameraOffset.x, -2f, 2f);
            cameraOffset.y = Mathf.Clamp(cameraOffset.y, -2f, 2f);

            Quaternion camRot = Quaternion.Euler(sphericCamRot);

            Vector3 newPos = _mesh.bounds.center;

            if (designPreview != null)
            {
                newPos = designPreview.GetFullBounds().center;
            }

            newPos += camRot * (Vector3.back * (1f + _mesh.bounds.size.magnitude) * camDistance);
            newPos += camRot * new Vector3(cameraOffset.x, cameraOffset.y, 0f);

            _previewRenderUtility.camera.transform.position = newPos;
            _previewRenderUtility.camera.transform.rotation = camRot;


            if (Event.current.type == EventType.Repaint)
            {
                _previewRenderUtility.BeginPreview(r, background);


                if (designPreview != null)
                {
                    for (int i = 0; i < designPreview.LatestGeneratedMeshes.Count; i++)
                    {
                        var mesh = designPreview.LatestGeneratedMeshes[i];
                        _previewRenderUtility.DrawMesh(mesh, Matrix4x4.identity, designPreview.LatestGeneratedMeshesMaterials[i], 0);

                    }
                }
                else if (_mesh)
                {
                    _previewRenderUtility.DrawMesh(_mesh, Matrix4x4.identity, PreviewMaterial, 0);
                }


                if (_selectionMaterial)
                    if (selectedInstance != null)
                        if (selectedInstance._ModMesh != null)
                        {
                            _previewRenderUtility.DrawMesh(selectedInstance._ModMesh, selectedInstance.GenerateMatrix(), _selectionMaterial, 0);
                        }
                        else
                        {
                            selectedInstance.RefreshModMesh();
                        }


                if (_mesh != null)
                    if (_mesh.subMeshCount > 1)
                    {
                        Material m = PreviewMaterial;
                        if (_customSubMeshMaterial != null) m = _customSubMeshMaterial;
                        _previewRenderUtility.DrawMesh(_mesh, Matrix4x4.identity, m, 1);
                    }


                Handles.SetCamera(_previewRenderUtility.camera);

                Handles.color = Color.gray * 0.5f;
                Handles.DrawLine(new Vector3(-3, 0, 0), new Vector3(3, 0, 0));
                Handles.DrawLine(new Vector3(0, 0, -3), new Vector3(0, 0, 3));
                Handles.color = new Color(0.3f, 1f, 0.4f, 0.1f);

                float scaleRef = 0.05f;
                if (_mesh != null) scaleRef = Mathf.Sqrt(_mesh.bounds.size.magnitude) * 0.05f;
                Handles.SphereHandleCap(0, Vector2.zero, Quaternion.identity, scaleRef, EventType.Repaint);

                Handles.color = Color.white;

                _previewRenderUtility.camera.Render();

                Texture resultRender = _previewRenderUtility.EndPreview();
                GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
            }

            #region Input events

            bool mouseContained = r.Contains(Event.current.mousePosition);

            if (UseScroll)
            {
                if (mouseContained)
                    if (Event.current.type == EventType.ScrollWheel)
                    {
                        if (Event.current.delta.y > 0)
                            camDistance += 0.1f;
                        else
                        if (Event.current.delta.y < 0)
                            camDistance -= 0.1f;

                        Event.current.Use();
                    }
            }

            if (Event.current.type == EventType.MouseDrag)
            {
                if (Event.current.button != 2)
                {
                    sphericCamRot.x += Event.current.delta.y;
                    sphericCamRot.y += Event.current.delta.x;
                }
                else
                {
                    cameraOffset.x -= Event.current.delta.x * 0.02f * camDistance;
                    cameraOffset.y += Event.current.delta.y * 0.02f * camDistance;
                }

                Event.current.Use();
            }

            #endregion

        }

    }
}

#endif