using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FIMSpace.Generating.Planning;
using FIMSpace.FEditor;
using System;
using FIMSpace.Generating.Checker;

namespace FIMSpace.Generating
{
    public partial class BuildPlannerWindow
    {
        public bool DrawWhenFocused = false;
        public float DebugPlayFPS = 12f;

        #region Base preparation for Scene GUI Draw

        void OnFocus()
        {
#if UNITY_2019_4_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
            SceneView.duringSceneGui += this.OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
            FGeneratorsGizmosDrawer.AddEvent(OnDrawGizmos);
        }

        bool isVisible = true;
        void OnBecameVisible()
        {
            isVisible = true;
            SceneView.RepaintAll();
        }

        void OnBecameInvisible()
        {
            FGeneratorsGizmosDrawer.TemporaryRemove();
            isVisible = false;
            SceneView.RepaintAll();
        }


        void OnDestroy()
        {
            //viewed = 0;
            FGeneratorsGizmosDrawer.TemporaryRemove();

#if UNITY_2019_4_OR_NEWER
            SceneView.duringSceneGui -= this.OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif

            if (FGeneratorsGizmosDrawer.Instance)
            {
                FGenerators.DestroyObject(FGeneratorsGizmosDrawer.Instance.gameObject);
                //if (AutoDestroy) ClearAllGeneratedGameObjects();
            }
        }


        public Matrix4x4 gridMatrix = Matrix4x4.identity;
        float playStepTime = -1f;
        GUIStyle infoLabel = null;

        void OnSceneGUI(SceneView sceneView)
        {
            if (SceneView.currentDrawingSceneView == null) return;
            if (SceneView.currentDrawingSceneView.camera == null) return;
            if (!isVisible) return;

            if (DrawWhenFocused)
                if (focusedWindow != Get && (focusedWindow is SceneView == false)) return;


            Handles.BeginGUI();
            PlanGenerationPrint debugDisplay = null;

            float dpiH = FGenerators.EditorUIScale;
            int hOffset = (int)(Screen.height / dpiH);

            if (projectPreset != null)
            {
                Rect sRect = new Rect(6, 2, 280, 24);
                GUI.Label(sRect, "Seed: " + projectPreset.LatestSeed);
            }

            if (FGenerators.CheckIfExist_NOTNULL(PlanGenerationPrint.SelectedPlannerResult))
            {

                var rslt = PlanGenerationPrint.SelectedPlannerResult;
                Rect bRect = new Rect(16, 23, 220, 24);
                string duplStr = "";
                if (rslt.ParentFieldPlanner.IndexOfDuplicate > -1) duplStr = " [" + rslt.ParentFieldPlanner.IndexOfDuplicate + "]";
                GUI.Label(bRect, "[" + rslt.ParentFieldPlanner.IndexOnPreset + "] " + rslt.ParentFieldPlanner.name + duplStr, EditorStyles.boldLabel);

                if (rslt.ParentFieldPlanner.Discarded)
                {
                    Rect dRect = new Rect(bRect);
                    dRect.position += new Vector2(bRect.width + 16, 0);
                    GUI.Label(dRect, "DISCARDED", EditorStyles.boldLabel);
                }

                Rect nRect = new Rect(bRect);
                nRect.position += new Vector2(0, 26);
                GUI.Label(nRect, "Cells: " + rslt.Checker.ChildPositionsCount);
                nRect.position += new Vector2(70, 0);
                GUI.Label(nRect, "Origin Position: " + rslt.Checker.RootPosition);
                nRect.position += new Vector2(198, 0);
                GUI.Label(nRect, "Rotation: " + rslt.Checker.RootRotation.eulerAngles.V3toV3Int());

                nRect.position = new Vector2(bRect.position.x, nRect.position.y + 26);
                GUI.Label(nRect, "Instructions: " + rslt.CellsInstructions.Count);

            }
            else if (GeneratedDebugSteps.Count > 0) // debug generator view
            {
                Rect bRect = new Rect(15, 23, 280, 24);
                GUI.Label(bRect, "Debug Generating Procedures Step-by-step", FGUI_Resources.HeaderStyle);

                bRect = new Rect(20, 49, 30, 24);

                if (GUI.Button(bRect, new GUIContent(FGUI_Resources.Tex_ArrowLeft)))
                {
                    DebugStep -= 1;
                }

                bRect = new Rect(bRect);
                bRect.x += bRect.width + 10;
                bRect.width += 50;

                if (DebugStep == -1)
                    GUI.Label(bRect, "Result", FGUI_Resources.HeaderStyle);
                else
                    GUI.Label(bRect, (1 + DebugStep) + " / " + GeneratedDebugSteps.Count, FGUI_Resources.HeaderStyle);

                bRect = new Rect(bRect);
                bRect.x += bRect.width + 10;
                bRect.width -= 50;

                if (GUI.Button(bRect, new GUIContent(FGUI_Resources.Tex_ArrowRight)))
                {
                    DebugStep += 1;
                }

                bRect = new Rect(bRect);
                bRect.x += bRect.width + 10;

                if (DebugStepPlay) GUI.backgroundColor = Color.gray;

                if (GUI.Button(bRect, new GUIContent(FGUI_Resources.Tex_RightFold)))
                {
                    if (DebugStep > GeneratedDebugSteps.Count - 5) DebugStep = 0;
                    DebugStepPlay = !DebugStepPlay;
                }

                if (DebugStepPlay) GUI.backgroundColor = Color.white;

                Rect sliderR = new Rect(bRect);
                sliderR.position += new Vector2(40, 0);
                sliderR.width = 64;
                GUI.Label(sliderR, "Speed:");
                sliderR.position += new Vector2(50, 3);
                sliderR.width = 90;
                DebugPlayFPS = GUI.HorizontalSlider(sliderR, DebugPlayFPS, 1f, 30f);

                sliderR.position = new Vector2(20, 77);
                sliderR.width = 340;
                DebugStep = Mathf.RoundToInt(GUI.HorizontalSlider(sliderR, DebugStep, -1, GeneratedDebugSteps.Count - 1));


                if (DebugStep <= -1) debugDisplay = GeneratedPreview;
                else
                    if (DebugStep < GeneratedDebugSteps.Count)
                    debugDisplay = GeneratedDebugSteps[DebugStep];

                if (DebugStep > -1)
                    if (debugDisplay != null)
                    {
                        bRect = new Rect(20, 98, 400, 100);
                        if (infoLabel == null) { infoLabel = new GUIStyle(EditorStyles.boldLabel); infoLabel.alignment = TextAnchor.UpperLeft; }
                        GUI.Label(bRect, debugDisplay.DebugInfo, infoLabel);

                    }

                SceneView latestScV = SceneView.lastActiveSceneView;

                if (latestScV)
                {
                    bRect = new Rect(16, hOffset - 46, 200, 20);
                    GUI.Label(bRect, _lastestGenMs + "ms");
                }

            }

            if (projectPreset != null)
            {
                if (!string.IsNullOrEmpty(projectPreset.CustomInfo))
                {
                    GUIContent cont = new GUIContent("Info:\n" + projectPreset.CustomInfo);
                    Vector2 sz = EditorStyles.label.CalcSize(cont); sz = new Vector2(sz.x * 1.04f + 8, sz.y * 1.1f);
                    Rect bRect = new Rect((Screen.width / dpiH) - sz.x, hOffset - 45 - sz.y, sz.x, sz.y);
                    GUI.Label(bRect, cont);
                }
            }

            PlanGenerationPrint.DrawCellsSceneGUI();
            //if (FGenerators.CheckIfExist_NOTNULL(PlanGenerationPrint.SelectedCellOwner))
            //    if (FGenerators.CheckIfExist_NOTNULL(PlanGenerationPrint.SelectedCellRef))
            //    {
            //        PlanGenerationPrint.SelectedPlannerResult = PlanGenerationPrint.SelectedCellOwner;

            //        var cellOwner = PlanGenerationPrint.SelectedCellOwner;
            //        var cell = PlanGenerationPrint.SelectedCellRef;
            //        var id = PlanGenerationPrint.SelectedCellCommanndI;


            //        Rect bRect = new Rect(16, 120, 420, 24);
            //        GUI.Label(bRect, "Cell Local Position: " + cell.Pos + "     Commands: " + PlanGenerationPrint.SelectedCellCommanndsCount);
            //        bRect.width = 220;

            //        if (PlanGenerationPrint.SelectedCellCommanndsCount > 1)
            //        {
            //            Rect sRect = new Rect(16, 140, 160, 24);
            //            GUI.Label(sRect, "Command In Cell: ");
            //            sRect.position += new Vector2(118, 5);
            //            PlanGenerationPrint.SelectedCellCommanndI = Mathf.RoundToInt(GUI.HorizontalSlider(sRect, PlanGenerationPrint.SelectedCellCommanndI, 0, PlanGenerationPrint.SelectedCellCommanndsCount - 1));
            //        }
            //        else
            //            bRect.position -= new Vector2(0, 18);

            //        int cellCounter = 0;
            //        for (int i = 0; i < PlanGenerationPrint.SelectedCellOwner.CellsInstructions.Count; i++)
            //        {
            //            var instr = PlanGenerationPrint.SelectedCellOwner.CellsInstructions[i];
            //            if (instr.HelperCellRef == PlanGenerationPrint.SelectedCellRef)
            //            {
            //                cellCounter += 1;
            //                if (cellCounter == PlanGenerationPrint.SelectedCellCommanndI + 1)
            //                {
            //                    PlanGenerationPrint.DisplayedCellCommand = instr;
            //                    break;
            //                }
            //            }
            //        }

            //        if (FGenerators.CheckIfExist_NOTNULL(PlanGenerationPrint.DisplayedCellCommand))
            //        {
            //            var cmd = PlanGenerationPrint.DisplayedCellCommand;

            //            Rect cRect = new Rect(16, bRect.y+44, 160, 24);

            //            if (PlanGenerationPrint.SelectedCellCommanndsCount > 1)
            //            {
            //                GUI.Label(cRect, cellCounter + "/" + PlanGenerationPrint.SelectedCellCommanndsCount);
            //                cRect.position += new Vector2(32, 0);
            //            }

            //            GUI.Label(cRect, "Command Index: [" + cmd.Id + "]");

            //            if (cmd.UseDirection)
            //            {
            //                cRect.position += new Vector2(120, 0);
            //                GUI.Label(cRect, "Rotation: " + cmd.rot.eulerAngles);
            //            }
            //        }

            //    }


            Handles.EndGUI();

            Handles.BeginGUI();
            Handles.SetCamera(SceneView.currentDrawingSceneView.camera);
            Handles.matrix = gridMatrix;
            //if (grid != null) DrawHandles(SceneView.currentDrawingSceneView.camera);
            Handles.matrix = Matrix4x4.identity;

            if (Get)
                if (Get.GeneratedDebugSteps != null)
                {
                    if (Get.GeneratedDebugSteps.Count > 0 && Get.DebugStep < Get.GeneratedDebugSteps.Count)
                    {
                        if (Get.DebugStep < 0)
                            PlanGenerationPrint.DrawPrintHandles(GeneratedPreview);
                        else
                            PlanGenerationPrint.DrawPrintHandles(Get.GeneratedDebugSteps[Get.DebugStep]);
                    }
                    else
                    {
                        PlanGenerationPrint.DrawPrintHandles(GeneratedPreview);
                    }
                }

            Handles.EndGUI();

            if (DebugStepPlay)
            {
                float fpsTime = 1f / DebugPlayFPS;
                if (EditorApplication.timeSinceStartup - playStepTime > fpsTime)
                {
                    playStepTime = (float)EditorApplication.timeSinceStartup;
                    DebugStep += 1;
                }

                if (DebugStep >= GeneratedDebugSteps.Count)
                {
                    DebugStepPlay = false;
                    DebugStep = -1;
                }

                repaint = true;
            }

            // Auto update support
            if (projectPreset)
                if (projectPreset._Editor_GraphNodesChanged)
                {
                    ForceUpdateView();
                    projectPreset._Editor_GraphNodesChanged = false;
                    repaint = true;
                }
        }

        #endregion


        void OnDrawGizmos()
        {
            if (!isVisible) return;

            if (FieldPlannerWindow.Get)
                if (FieldPlannerWindow.LatestFieldPlanner != null)
                {
                    if (FieldPlannerWindow.LatestFieldPlanner._EditorDrawShape)
                    {
                        if (FocusOnShape)
                        {
                            Handles.matrix = Matrix4x4.identity;
                            Gizmos.matrix = Matrix4x4.identity;
                            DrawShapeAndGuides(FieldPlannerWindow.LatestFieldPlanner.GetInitialChecker(), 1f);
                            //UnityEngine.Debug.Log("Checker cells " + FieldPlannerWindow.LatestFieldPlanner.GetChecker().ChildPos.AllCells.Count);
                        }
                    }
                }

            if (DebugStep > 0) if (DebugStep < GeneratedDebugSteps.Count)
                    if (GeneratedDebugSteps[DebugStep].DebugGizmosAction != null)
                    {
                        GeneratedDebugSteps[DebugStep].DebugGizmosAction.Invoke();
                    }

            DrawGridAndPreview();
        }

        #region Checker Draw

        public static void DrawShapeAndGuides(CheckerField3D checker, float PreviewSize)
        {
            DrawGuidesGUI(checker, PreviewSize);
            DrawShape(checker);
        }

        public static void DrawGuidesGUI(CheckerField3D checker, float PreviewSize)
        {
            if (Get) if (!Get.isVisible) return;
            Color prehC = Handles.color;

            Vector3 cellSize = Vector3.one * PreviewSize;
            int pSz = Mathf.RoundToInt(PreviewSize);

            // Draw grid helper axis
            Vector3Int min = new Vector3Int(-pSz, 0, 0);
            if (FGenerators.CheckIfExist_NOTNULL(checker))
                if (checker.Grid.AllApprovedCells.Count > 0)
                {
                    min = new Vector3Int(Mathf.RoundToInt(checker.GetGridWorldMin().x) * pSz, 0, Mathf.RoundToInt(checker.GetGridWorldMax().y) * pSz);
                }

            Handles.color = new Color(0f, 0f, 1f, 0.5f);
            Vector3 leftSide = new Vector3((min.x - 0.5f) * (cellSize.x), 0, (min.z + 0.25f) * (cellSize.z));
            leftSide += cellSize.x * Vector3.left * 0.3f;

            Handles.Label(leftSide + cellSize.x * Vector3.left * 0.3f + Vector3.back * cellSize.x * 0.2f, new GUIContent("Forward"), EditorStyles.centeredGreyMiniLabel);
            FEditor.FGUI_Handles.DrawArrow(leftSide, Quaternion.identity, cellSize.x * 0.5f);

            Handles.color = new Color(1f, .4f, .4f, 0.5f);
            Vector3 rightSide = new Vector3((min.x + 0.25f) * (cellSize.x), 0, (min.z - 0.5f) * (cellSize.z));
            rightSide += cellSize.z * Vector3.back * 0.3f;
            Handles.Label(rightSide + cellSize.z * Vector3.back * 0.3f, new GUIContent("Right"), EditorStyles.centeredGreyMiniLabel);
            FEditor.FGUI_Handles.DrawArrow(rightSide, Quaternion.Euler(0, 90, 0), cellSize.x * 0.5f);

            Handles.color = prehC;
        }

        public static void DrawShape(CheckerField3D checker)
        {
            if (FGenerators.CheckIfIsNull(checker))
            {
                return;
            }

            Color preC = Gizmos.color;
            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
            checker.DrawFieldGizmos();
            Gizmos.color = preC;
        }

        #endregion



        public static void ForceUpdateView()
        {
            ForceAutoUpdate = true;
            if (Get) Get.OnGUI();
            SceneView.RepaintAll();
        }


        void DrawHandles(Camera c)
        {
            Color cl = GUI.color;
            Matrix4x4 mx = gridMatrix;

            DrawDebugCommandsHandles();

            GUI.color = cl;
            Repaint();
        }


        void DrawGridAndPreview()
        {
            if (Get == null) return;
            if (projectPreset == null) return;
            if (!isVisible) return;
            if (DrawWhenFocused)
                if (focusedWindow != Get && (focusedWindow is SceneView == false)) return;

            Color preC = Gizmos.color;
            Color prehC = Handles.color;

            if (Get.GeneratedPreview == null)
            {
                Get.DebugStep = -1;
            }
            else
            {
                if (Get.GeneratedDebugSteps.Count > 0)
                {
                    if (Get.DebugStep < -1) Get.DebugStep = -1;
                    if (Get.DebugStep >= Get.GeneratedDebugSteps.Count) Get.DebugStep = -1;

                    if (Get.DebugStep < 0)
                        PlanGenerationPrint.DrawPrintGizmos(GeneratedPreview);
                    else
                        PlanGenerationPrint.DrawPrintGizmos(Get.GeneratedDebugSteps[Get.DebugStep]);
                }
                else
                {
                    PlanGenerationPrint.DrawPrintGizmos(GeneratedPreview);
                }
            }

            Handles.color = prehC;
            Gizmos.color = preC;
        }


        #region Debug Draw commands


        List<DrawCommand> drawCommands;
        int drawCommandDuration = 0;
        public bool FocusOnShape = false;

        public void DrawCellPosition(FieldCell cell, int duration, float size, Color col)
        {
            Vector3 pos = new Vector3(cell.Pos.x, cell.Pos.y, cell.Pos.z);
            pos *= 1f;
            DrawPosition(pos, duration, size, col);
        }


        public void DrawPosition(Vector3 position, int duration, float size, Color col)
        {
            if (drawCommands == null) drawCommands = new List<DrawCommand>();
            drawCommands.Add(new DrawCommand() { pos = position, duration = duration, size = size, c = col });
            if (drawCommandDuration <= 0) drawCommandDuration = duration;
        }


        void DrawDebugCommandsHandles()
        {
            if (drawCommands != null)
                if (drawCommands.Count > 0)
                {
                    drawCommandDuration -= 1;
                    Handles.color = drawCommands[0].c;
                    Handles.SphereHandleCap(0, drawCommands[0].pos, Quaternion.identity, 1f * 0.3f * drawCommands[0].size, EventType.Repaint);

                    if (drawCommandDuration <= 0)
                    {
                        drawCommands.RemoveAt(0);
                        if (drawCommands.Count > 0) drawCommandDuration = drawCommands[0].duration + 1;
                    }
                }
        }


        public struct DrawCommand
        {
            public Vector3 pos;
            public int duration;
            public float size;
            public Color c;
        }


        #endregion


    }
}