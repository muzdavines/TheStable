using UnityEngine;
using UnityEditor;

namespace FIMSpace.Generating
{
    public partial class BuildingPlanDesignerWindow
    {
        public bool AutoRefreshPreview = true;
        [Range(0f, 1f)] public float PreviewAlpha = 0.2f;
        public float PreviewSize = 1f;

        #region Preparation

        void OnFocus()
        {
            //SceneView.duringSceneGui -= this.OnSceneGUI;
            //SceneView.duringSceneGui += this.OnSceneGUI;
            FGeneratorsGizmosDrawer.AddEvent(OnDrawGizmos);
        }

        void OnDestroy()
        {
            //SceneView.duringSceneGui -= this.OnSceneGUI;

            if (FGeneratorsGizmosDrawer.Instance)
                FGenerators.DestroyObject(FGeneratorsGizmosDrawer.Instance.gameObject);
        }


        //public Matrix4x4 gridMatrix = Matrix4x4.identity;
        //void OnSceneGUI(SceneView sceneView)
        //{
        //    if (SceneView.currentDrawingSceneView == null) return;
        //    if (SceneView.currentDrawingSceneView.camera == null) return;

        //    Handles.BeginGUI();
        //    Handles.SetCamera(SceneView.currentDrawingSceneView.camera);

        //    //

        //    Handles.EndGUI();
        //}

        #endregion


        void OnDrawGizmos()
        {
            //if (grid == null) return;
            //if (grid.AllCells == null) return;
            if (selectedPreset == null) return;


            Color preC = Gizmos.color;
            Color prehC = Handles.color;

            Vector3 cellSize = new Vector3(PreviewSize, PreviewSize * 0.1f, PreviewSize);
            float modColStep = 1f / (float)selectedPreset.Settings.Count;

            float allWidth = 0;
            float allHeight = 0;
            for (int i = 0; i < selectedPreset.Settings.Count; i++)
            {
                allWidth += selectedPreset.Settings[i].InternalSetup.RectSetup.Width.Max;
                allHeight += selectedPreset.Settings[i].InternalSetup.RectSetup.Height.Max;
            }


            #region Draw Rooms Palette

            float offset = -allWidth / 2f;
            float scale = 1f; // PreviewSize

            if (drawLegend)
                for (int i = 0; i < selectedPreset.Settings.Count; i++)
                {
                    Gizmos.color = Color.HSVToRGB(i * modColStep, 0.5f, 0.5f);
                    Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);

                    Vector3 origin = new Vector3(offset, 0, (allHeight + 1));
                    Vector3 size = new Vector3(selectedPreset.Settings[i].InternalSetup.RectSetup.Width.Max, scale / 4f, selectedPreset.Settings[i].InternalSetup.RectSetup.Height.Max);
                    Vector3 drawPos = (origin + Vector3.right * (size.x / 2)) * scale;

                    Gizmos.DrawCube(drawPos, size * scale);

                    Vector3 lPos = drawPos;

                    if (i % 2 == 0)
                        lPos += Vector3.back * ((float)size.z / 2f * scale);
                    else
                        lPos -= Vector3.back * ((float)(size.z + 1.5f) / 2f * scale);

                    Handles.Label(lPos, selectedPreset.Settings[i].GetName(), EditorStyles.centeredGreyMiniLabel);

                    offset += size.x + 1;
                }

            #endregion


            #region Drawing Margins and helpers

            if (LimitSize)
            {
                Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

                Gizmos.DrawLine(new Vector3(SizeLimitX.x, 0, SizeLimitZ.x) * PreviewSize, new Vector3(SizeLimitX.y, 0, SizeLimitZ.x) * PreviewSize);
                Gizmos.DrawLine(new Vector3(SizeLimitX.y, 0, SizeLimitZ.x) * PreviewSize, new Vector3(SizeLimitX.y, 0, SizeLimitZ.y) * PreviewSize);
                Gizmos.DrawLine(new Vector3(SizeLimitX.y, 0, SizeLimitZ.y) * PreviewSize, new Vector3(SizeLimitX.x, 0, SizeLimitZ.y) * PreviewSize);
                Gizmos.DrawLine(new Vector3(SizeLimitX.x, 0, SizeLimitZ.y) * PreviewSize, new Vector3(SizeLimitX.x, 0, SizeLimitZ.x) * PreviewSize);
            }

            if (GuideCorridors)
            {
                Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                Vector3 pos = new Vector3(StartGuidePos.x, 0, StartGuidePos.y) * PreviewSize - cellSize * 0.5f;
                Vector3 d = StartGuideDirection.GetDirection() * PreviewSize;
                Gizmos.DrawRay(pos, d);
                Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) + Vector3.right * 0.12f * PreviewSize);
                Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) - Vector3.right * 0.12f * PreviewSize);
                Gizmos.DrawCube(pos, cellSize);

                pos = new Vector3(EndGuidePos.x, 0, EndGuidePos.y) * PreviewSize - cellSize * 0.5f;
                d = EndGuideDirection.GetDirection() * PreviewSize;
                Gizmos.DrawRay(pos, d);
                Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) + Vector3.right * 0.12f * PreviewSize);
                Gizmos.DrawLine(pos + d, Vector3.Lerp(pos, pos + d, 0.7f) - Vector3.right * 0.12f * PreviewSize);
                Gizmos.DrawCube(pos, cellSize);
            }

            #endregion


            #region Drawing Plan (rooms)


            if (planHelper != null)
            {
                float ySize = PreviewSize * 0.05f;

                int countInNoCorr = 0;
                for (int i = 0; i < planHelper.InteriorRects.Count; i++)
                {
                    if (planHelper.InteriorRects[i].TypeID == -1)
                    {
                        Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
                    }
                    else
                    {
                        countInNoCorr += 1;
                        Gizmos.color = Color.HSVToRGB(planHelper.InteriorRects[i].IndividualID * modColStep, 0.5f, 0.5f);
                        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
                    }

                    Bounds b = planHelper.InteriorRects[i].Bound;
                    Gizmos.DrawCube(b.center * PreviewSize, new Vector3(b.size.x, ySize, b.size.z) * PreviewSize);

                    //Handles.Label(b.min, new GUIContent(b.min.ToString()), EditorStyles.centeredGreyMiniLabel);
                }

                for (int i = 0; i < planHelper.ConnectionRects.Count; i++)
                {
                    var cr = planHelper.ConnectionRects[i];

                    Bounds b = cr.Bound;
                    b.size = new Vector3(b.size.x, b.size.y / 4f, b.size.z);

                    Vector3 tSize;

                    if ((int)planHelper.ConnectionRects[i].direction >= 2)
                        tSize = new Vector3(b.size.x / 12f, ySize / 16f, b.size.z / 1.5f);
                    else
                        tSize = new Vector3(b.size.x / 1.5f, ySize / 16f, b.size.z / 12f);

                    Gizmos.color = new Color(.1f, .1f, .1f, 1f);

                    //Gizmos.DrawCube(connPos * PreviewSize, (tSize - widthOff) * (PreviewSize));

                    //if ((int)planHelper.ConnectionRects[i].direction >= 2)
                    //    tSize = new Vector3(b.size.x / 8f, ySize / 10f, b.size.z / 1.5f);
                    //else
                    //    tSize = new Vector3(b.size.x / 1.5f, ySize / 10f, b.size.z / 8f);
                    Gizmos.DrawCube(b.center * PreviewSize, tSize * PreviewSize);
                    //Gizmos.color = new Color(.1f, .1f, .1f, .4f);
                }

            }

            //if (minimap != null)
            //{
            //    Handles.Label(new Vector3(10, 4, -10), new GUIContent(minimap));
            //}

            #endregion


            Handles.color = prehC;
            Gizmos.color = preC;
        }


        PlanHelper planHelper;
        void GeneratePlan()
        {
            if (seed != 0) FGenerators.SetSeed(seed);

            planHelper = new PlanHelper(selectedPreset);
            if (LimitSize) planHelper.SetLimits(SizeLimitX, SizeLimitZ);

            if (selectedPreset != null)
                if (selectedPreset.RootChunkSetup.FieldSetup != null)
                {
                    if (GuideCorridors)
                        planHelper.GeneratePathFindedCorridor(StartGuidePos, EndGuidePos, StartGuideDirection.GetDirection2D(), EndGuideDirection.GetDirection2D(), guideFatness, changeDirectionCost);

                    if (selectedPreset.RootChunkSetup.InternalSetup.BranchLength.IsZero == false)
                        planHelper.GenerateCorridors(selectedPreset.RootChunkSetup.InternalSetup.TargetBranches.GetRandom() - 1, Get.WallsSeparation);
                }

            planHelper.GenerateRooms(Get.WallsSeparation);

            //if (PHUI_PlanToLoadingUI.Instance) PHUI_PlanToLoadingUI.Instance.GenerateMap(planHelper);
            repaint = true;
        }

    }
}