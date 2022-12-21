using FIMSpace.FEditor;
using FIMSpace.Generating;
using FIMSpace.Generating.Planner.Nodes;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.ModNodes.Transforming;
using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Planning.PlannerNodes.Math.Algebra;
using FIMSpace.Generating.Planning.PlannerNodes.Math.Values;
using FIMSpace.Generating.Planning.PlannerNodes.Math.Vectors;
using FIMSpace.Generating.Planning.PlannerNodes.Utilities;
using FIMSpace.Generating.Rules.QuickSolutions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Graph
{

    // Graph Drawer ---------------------

    public class ModGraphDrawer : FGraphDrawerBase
    {

        #region Preset file to modify setup

        public IPlanNodesContainer currentSetup = null;
        public override ScriptableObject DebugDrawPreset => currentSetup == null ? null : currentSetup.ScrObj;
        #endregion
        public bool DrawedInsideInspector = false;
        public Texture2D AltVignette = null;
        public double _LatestRefreshDisplayFlag = -2;
        SR_ModGraph latestModGraph = null;
        PlannerFunctionNode latestFunction = null;

        public ModGraphDrawer(EditorWindow parent, IPlanNodesContainer preset) : base(parent)
        {
            currentSetup = preset;
            if (currentSetup is SR_ModGraph) latestModGraph = currentSetup as SR_ModGraph;
            else latestFunction = currentSetup as PlannerFunctionNode;
        }

        public List<PGGPlanner_NodeBase> Nodes { get { return currentSetup.Procedures; } }
        protected override int PresetNodesCount => Nodes.Count;
        public override ScriptableObject ProjectFilePreset => currentSetup == null ? null : currentSetup.ScrObj;
        //protected override Texture2D DefaultConnectionTex => PlannerGraphWindow.PlannerGraphStyles.TEX_Gradient1;
        public override Type GetBaseNodeType => typeof(PlannerRuleBase);


        protected override void OnAddNode(FGraph_NodeBase node)
        {
            PlannerRuleBase planner = node as PlannerRuleBase;
            if (planner) planner.ParentPlanner = currentSetup as FieldPlanner;
            base.OnAddNode(node);
            AutoConnectNode(node);

            RefreshNodes();
        }


        void AutoConnectNode(FGraph_NodeBase node)
        {
            if (node.DrawInputConnector == false) return;
            if (PGGPlanner_NodeBase.AutoSnap == false) return;

            FGraph_NodeBase lastConn = LastConnectedWithProceduresStart();

            if (lastConn != null)
            {
                lastConn.CreateConnectionWith(node, true, lastConn.HotOutputConnectionIndex);

                //if (PGGPlanner_NodeBase.AutoSnap)
                //{
                //    PGGPlanner_NodeBase nde = node as PGGPlanner_NodeBase;
                //    PGGPlanner_NodeBase lst = lastConn as PGGPlanner_NodeBase;
                //    if (nde && lst) lst.AlignViewedNodeWith(nde, true);
                //}

                lastConn._E_SetDirty();
                node._E_SetDirty();
            }
        }

        public override void OnGraphStructureChange()
        {
            base.OnGraphStructureChange();

            if (latestModGraph) RequestPlannerRefresh();

            //if (latestPlanner.ParentBuildPlanner)
            //{
            //    latestPlanner.ParentBuildPlanner._Editor_GraphNodesChanged = true;
            //}
        }

        List<FGraph_NodeBase> _loopStackOverflowPreventer = new List<FGraph_NodeBase>();
        FGraph_NodeBase LastConnectedWithProceduresStart()
        {
            FGraph_NodeBase sNode = FindNodeOfType(typeof(PE_Start));
            PE_Start startNode = null;
            if (sNode != null) startNode = sNode as PE_Start; else return null;

            if (sNode.FirstOutputConnection == null) return sNode;
            else
            {
                _loopStackOverflowPreventer.Clear();

                int limit = 0;
                while (sNode.FirstOutputConnection != null && sNode.FirstOutputConnection.DrawOutputConnector)
                {
                    if (_loopStackOverflowPreventer.Contains(sNode)) { Debug.Log("break"); break; }
                    _loopStackOverflowPreventer.Add(sNode);
                    limit += 1;
                    sNode = sNode.FirstOutputConnection;
                    if (limit > 1000) { Debug.Log("something wrong with node connections"); break; }
                }
            }

            return sNode;
        }

        public void RefreshTitle()
        {
            SR_ModGraph planner = latestModGraph;
            if (planner != null && planner.OwnerSpawner != null)
            {
                if (planner.name.StartsWith("SR_"))
                    TopTitle = planner.OwnerSpawner.Name;
                else
                    TopTitle = planner.name;
                //TopTitle = planner.OwnerSpawner.Name + "\n<size=10>Procedures (Layout Focus)</size>";
            }
            else
                TopTitle = "Mod Node Graph";
        }


        //bool wasDrawed = false;
        bool reloadedOnReload = false;
        public override void DrawGraph()
        {
            if (AltVignette != null)
                if (BGVignetteStyle == null || BGVignetteStyle.normal.background != AltVignette)
                {
                    BGVignetteStyle = new GUIStyle();
                    BGVignetteStyle.normal.background = AltVignette;
                }


            if (latestModGraph)
            {
                latestModGraph.RefreshStartGraphNodes();

                if (latestModGraph.ProceduresBegin == null)
                {
                    PE_Start val = ScriptableObject.CreateInstance<PE_Start>();
                    val.NodePosition = new Vector2(690, 430);
                    latestModGraph.AddRuleToPlanner(val);
                    OnAddNode(val);
                }

            }


            bool pre = EditorGUIUtility.wideMode;
            bool preh = EditorGUIUtility.hierarchyMode;

            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.hierarchyMode = true;

            base.DrawGraph();

            EditorGUIUtility.wideMode = pre;
            EditorGUIUtility.hierarchyMode = preh;

            if (!reloadedOnReload)
            {
                reloadedOnReload = true;

                if (currentSetup != null)
                {
                    //if (currentSetup is SR_ModGraph)
                    //{
                    //    SR_ModGraph fp = currentSetup as SR_ModGraph;
                    //    fp.RefreshOnReload();
                    //}
                    //else if (currentSetup is PlannerFunctionNode)
                    //{
                    //    PlannerFunctionNode pf = currentSetup as PlannerFunctionNode;
                    //    pf.RefreshNodeParams();
                    //}
                }
            }
        }


        protected override void NodeModifyMenu(Event e, FGraph_NodeBase node)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("REMOVE Node"), false, () =>
            {
                // Remove code
                ScheduleEditorEvent(() => RemoveNode(node));
            });
            
            //if (node is PlannerFunctionNode)
            //{
            //    PlannerFunctionNode fNode = node as PlannerFunctionNode;

            //    if (fNode.ProjectFileParent)
            //    {
            //        menu.AddItem(new GUIContent("Open Function Node Graph"), false, () =>
            //        {
            //            if (fNode.ProjectFileParent)
            //                PlannerGraphWindow.Init(fNode.ProjectFileParent);
            //        });

            //    }
            //}

            if (!string.IsNullOrEmpty(node.GetNodeTooltipDescription))
                menu.AddItem(new GUIContent("Show node description"), false, () =>
                {
                    EditorUtility.DisplayDialog(node.GetDisplayName() + " Description", node.GetNodeTooltipDescription, "Ok");
                });

            menu.AddItem(GUIContent.none, false, () => { });


            menu.AddItem(new GUIContent("[Debugging] Display Node in the Inspector Window"), false, () =>
            {
                Selection.activeObject = node;
            });


            PlannerRuleBase plNode = node as PlannerRuleBase;
            if (plNode != null)
            {
                menu.AddItem(new GUIContent("[Debugging] Refresh Ports (use only if there are port issues)"), false, () =>
                {
                    plNode.RemoveAllPortConnections();
                    plNode.CheckPortsForNullConnections();
                    plNode.RefreshPorts();
                });
            }


            menu.AddItem(new GUIContent("[Debugging] Switch Debug Variable"), node._EditorDebugMode, () =>
            {
                node._EditorDebugMode = !node._EditorDebugMode;
            });



            //menu.AddItem(GUIContent.none, false, () => { });

            //menu.AddItem(new GUIContent("+ Create Custom Function (f) Node"), false, () =>
            //{
            //    PlannerFunctionNode func = PlannerFunctionNode.CreateInstance<PlannerFunctionNode>();
            //    func.name = "FN_New Custom Function Node";
            //    AssetDatabase.CreateAsset(func, "Assets/FN_New Custom Function Node.asset");
            //    EditorGUIUtility.PingObject(func);
            //    AssetDatabase.OpenAsset(func);
            //});


            DisplayMenuUnscaled(menu);
        }


        #region Searchable Dropdown Nodes List Modifications

        public override string GetNodesNamespace { get { return "FIMSpace.Generating.Planning.ModNodes"; } }

        private static List<NodeRef> modNodesMenuContent = new List<NodeRef>();
        private static List<NodeRef> extraNodesMenuContent = new List<NodeRef>();
        private void MoveNodesTo(List<NodeRef> from, List<NodeRef> to)
        {
            for (int f = 0; f < from.Count; f++) to.Add(from[f]);
        }
        private void RemoveNodeFrom(List<NodeRef> from, Type type)
        {
            for (int i = from.Count - 1; i >= 0; i--) if (from[i].node.GetType() == type) { from.RemoveAt(i); }
        }


        public override List<NodeRef> GetNodesByNamespace()
        {
            modNodesMenuContent.Clear();

            bool refreshWasNeeded = nodesListRequiresRefresh;
            List<NodeRef> assemblyNodes = base.GetNodesByNamespace();

            if (extraNodesMenuContent.Count == 0 || refreshWasNeeded)
            {
                extraNodesMenuContent.Clear();

                List<NodeRef> extraNodes = GatherNodesByNamespace("FIMSpace.Generating.Planning.PlannerNodes", "FIMSpace.Generating.Planning.PlannerNodes.Logic");
                MoveNodesTo(extraNodes, extraNodesMenuContent);

                extraNodes = base.GatherNodesByNamespace("FIMSpace.Generating.Planning.PlannerNodes.Cells", "FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops");
                RemoveNodeFrom(extraNodes, typeof(FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops.PR_IterateCells));
                RemoveNodeFrom(extraNodes, typeof(FIMSpace.Generating.Planning.PlannerNodes.Cells.Loops.PR_IterateCellOutsideDirections));
                MoveNodesTo(extraNodes, extraNodesMenuContent);

                extraNodes = base.GatherNodesByNamespace("FIMSpace.Generating.Planning.PlannerNodes", "FIMSpace.Generating.Planning.PlannerNodes.Math");
                MoveNodesTo(extraNodes, extraNodesMenuContent);

                extraNodes = base.GatherNodesByNamespace("FIMSpace.Generating.Planning.PlannerNodes", "FIMSpace.Generating.Planning.PlannerNodes.Utilities");
                RemoveNodeFrom(extraNodes, typeof(PR_DebugDrawPosition));
                MoveNodesTo(extraNodes, extraNodesMenuContent);

                MoveNodesTo(extraNodesMenuContent, assemblyNodes);
            }

            //SeparatedModWindow parent = Parent as SeparatedModWindow;
            PlannerFunctionNode isFunction = null;// currentSetup as PlannerFunctionNode;
            //string[] functionNodes = new string[0];// AssetDatabase.FindAssets("t:PlannerFunctionNode");

            for (int i = 0; i < assemblyNodes.Count; i++)
            {
                var node = assemblyNodes[i];

                PGGPlanner_NodeBase plNode = node.node as PGGPlanner_NodeBase;

                // Select only wanted nodes
                if (isFunction != null) // Menu for function node graph
                {
                    if (plNode.NodeVisibility == PGGPlanner_NodeBase.EPlannerNodeVisibility.All || plNode.NodeVisibility == PGGPlanner_NodeBase.EPlannerNodeVisibility.JustFunctions)
                    {
                        modNodesMenuContent.Add(node);
                    }
                }
                else // Mod Nodes
                {
                    if (plNode.NodeVisibility != PGGPlanner_NodeBase.EPlannerNodeVisibility.JustFunctions)
                    {
                        modNodesMenuContent.Add(node);
                    }
                }

            }


            #region Backup Code for Function Nodes

            //for (int i = 0; i < functionNodes.Length; i++)
            //{
            //    PlannerFunctionNode projectFunc = AssetDatabase.LoadAssetAtPath<PlannerFunctionNode>(AssetDatabase.GUIDToAssetPath(functionNodes[i]));

            //    if (projectFunc == isFunction) continue;
            //    if (projectFunc == null) continue;

            //    projectFunc.ProjectFileParent = projectFunc;
            //    PlannerFunctionNode newFunc = PlannerFunctionNode.Instantiate(projectFunc);
            //    newFunc.ProjectFileParent = projectFunc;

            //    string nme = newFunc.DisplayName;
            //    if (string.IsNullOrEmpty(nme)) nme = newFunc.name;

            //    if (string.IsNullOrEmpty(newFunc.CustomPath))
            //    {
            //        nme = "Custom Function Nodes/" + nme;
            //    }
            //    else
            //    {
            //        nme = newFunc.CustomPath + "/" + nme;
            //    }

            //    NodeRef r = new NodeRef() { name = nme, node = newFunc as FGraph_NodeBase };

            //    plannerNodes.Add(r);
            //}

            #endregion

            return modNodesMenuContent;
        }


        #endregion


        #region Adding / Removing Nodes implementation

        protected override void FillListWithNodesToDraw(List<FGraph_NodeBase> willBeDrawed)
        {
            // Check for nulls
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                if (Nodes[i] == null) Nodes.RemoveAt(i);
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                willBeDrawed.Add(Nodes[i]);
            }
        }

        bool IsFuncGraph
        {
            get
            {
                if (latestFunction) return true;
                return false;
            }
        }

        public override void AddNewNodeToPreset(FGraph_NodeBase node, bool moveToCursorPos = true)
        {
            if (IsFuncGraph)
            {
                bool containsAlready = false;

                if (node is PE_Start)
                    for (int i = 0; i < latestFunction.Procedures.Count; i++)
                    {
                        if (latestFunction.Procedures[i] is PE_Start) { containsAlready = true; break; }
                    }

                if (containsAlready)
                {
                    UnityEngine.Debug.Log("[PGG] Only one 'Procedures Start' is allowed inside one graph");
                    return;
                }
            }

            if (moveToCursorPos) node.NodePosition = GetNodeCreatePos();

            if (latestModGraph == null) // Function
            {
                Nodes.Add(node as PlannerRuleBase);
                base.AddNewNodeToPreset(node, moveToCursorPos);
                OnAddNode(node);
            }
            else
            {
                latestModGraph.AddRuleToPlanner(node as PlannerRuleBase);
                OnAddNode(node);
                latestModGraph.RefreshStartGraphNodes();
            }
        }

        protected override void RemoveNode(FGraph_NodeBase node)
        {
            Nodes.Remove(node as PlannerRuleBase);
            base.RemoveNode(node);

            if (latestFunction)
            {
                latestFunction.OnNodeRemove(node);
            }
            else if (latestModGraph)
            {
                latestModGraph.OnNodeRemove(node);
            }
        }


        protected override void DoubleClickNode(Event e, FGraph_NodeBase node)
        {
            base.DoubleClickNode(e, node);

            if (node is PlannerFunctionNode)
            {
                PlannerFunctionNode fNode = node as PlannerFunctionNode;

                if (fNode.ProjectFileParent)
                    ModGraphWindow.Init(fNode.ProjectFileParent);
            }
        }

        protected override void DoubleMMBClick(Event e)
        {
            RequestPlannerRefresh();
        }

        protected override void MMBClick()
        {
            if (pressedKeys.Contains(KeyCode.R) || pressedKeys.Contains(KeyCode.N) || pressedKeys.Contains(KeyCode.C) || pressedKeys.Contains(KeyCode.LeftShift))
            {
                if (latestModGraph)
                {
                    RequestPlannerRefresh();
                }
            }
        }

        public void RequestPlannerRefresh()
        {
            if (latestModGraph == null) return;
            if (FieldDesignWindow.Get == null) return;
            if (FieldDesignWindow.Get.AutoRefreshPreview == false) return;

            FieldDesignWindow.Get.TriggerRefresh(false);
            repaintRequest = true;
            SceneView.RepaintAll();
            Event.current.Use();
        }

        protected override void LMBFreeClick()
        {
            if (pressedKeys.Count < 1) return;

            if (pressedKeys.Contains(KeyCode.Alpha1))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Value>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.Alpha2))
            {
                PR_Value val = ScriptableObject.CreateInstance<PR_Value>();
                val.InputType = EType.Vector3;
                AddNewNodeToPreset(val);
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.Alpha3))
            {
                PR_Value val = ScriptableObject.CreateInstance<PR_Value>();
                val._EditorFoldout = false;
                val.InputType = EType.Vector3;
                AddNewNodeToPreset(val);
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.S))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Subtract>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.A))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Add>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.G))
            {
                //AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetLocalVariable>());
                //LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.V) || pressedKeys.Contains(KeyCode.F) || pressedKeys.Contains(KeyCode.P))
            {
                //AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetFieldPlannerVariable>());
                //LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.B))
            {
                //AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_GetBuildVariable>());
                //LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.L))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Lerp>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.D))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Divide>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.I))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Invert>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.H))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Half>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.R))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<MR_SetRotation>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.W))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<MR_SetPosition>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.X))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.C))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Comment>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.X))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.X))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Rewire>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.N))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Normalize>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.B))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Split>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.O))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_OneMinus>());
                LMBUnclickPrevent();
            }
            else if (pressedKeys.Contains(KeyCode.M))
            {
                AddNewNodeToPreset(ScriptableObject.CreateInstance<PR_Multiply>());
                LMBUnclickPrevent();
            }
        }


        #endregion


        #region Override Connection draw style

        protected override Vector3 DrawConnectionWithTangents(Vector2 start, Vector2 end, bool outToInput = true, Color? color = null, float thickness = 5, bool getMiddlePoint = false, Texture2D lineTex = null, FGraph_NodeBase_Drawer.EConnectorsWireMode wireMode = FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, float oXTan = 0.8f, float iXTan = 0.8f, float oYTan = 0.6f, float iYTan = 0.6f, float tanMul = 1f)
        {
            if (wireMode == FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right)
            {
                if (end.y < start.y)
                    return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness * 0.6f, getMiddlePoint, null, wireMode, 0.8f, 0.8f, 0.6f, 0.6f, tanMul);
                else
                    return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness * 0.6f, getMiddlePoint, null, wireMode, 0.9f, 0.5f, 1.05f, 0.6f, tanMul);
            }

            Vector2 a = start, b = end;
            Vector2 toTarget = b - a;
            if (toTarget.sqrMagnitude < 100) return start;

            if (end.y > start.y)
            {
                return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness, getMiddlePoint, ModGraphWindow.PlannerGraphStyles.TEX_Gradient1, wireMode, 0.8f, 0.8f, 0.6f, 0.6f, tanMul);
            }
            else // Direct to above
            {
                if ((end.x > start.x) && _DrawConnection_input && _DrawConnection_output)
                {
                    Vector3 nend = _DrawConnection_input._E_LatestRect.center - new Vector2((_DrawConnection_input.NodeSize.x / 2f - 25), -_DrawConnection_input.NodeSize.y * 0.475f + 18);
                    base.DrawConnectionWithTangents(nend, end, outToInput, new Color(0.7f, 0.7f, 0.7f, 0.35f), thickness, getMiddlePoint, ModGraphWindow.PlannerGraphStyles.TEX_Gradient1, wireMode, 0.4f, 1.0f, -0.25f, -0.1f, tanMul);
                    return base.DrawConnectionWithTangents(start, nend, outToInput, color, thickness, getMiddlePoint, ModGraphWindow.PlannerGraphStyles.TEX_Gradient1, FGraph_NodeBase_Drawer.EConnectorsWireMode.Left_Right, 0.8f, 0.9f, 0.0f, -0.3f, tanMul);
                }
                else
                    return base.DrawConnectionWithTangents(start, end, outToInput, color, thickness, getMiddlePoint, ModGraphWindow.PlannerGraphStyles.TEX_Gradient1, wireMode, 0.8f, 0.8f, 0.6f, 0.6f, tanMul);
            }
        }

        #endregion


        protected override bool IsCursorInGraph()
        {
            if (objRefRect.Contains(eventMousePos)) return false;
            if (refrButtonRect.Contains(eventMousePos)) return false;
            return base.IsCursorInGraph();
        }

        protected override bool IsCursorInAdditionalActionArea()
        {
            if (refrButtonRect.Contains(eventMousePos)) return true;
            return false;
        }


        Event cursorOutOfGraphEventForward = null;
        Rect refrButtonRect = new Rect();
        Rect objRefRect = new Rect();
        public string TopTitle = "";


        protected override void DrawGraphOverlay()
        {
            float lOffset = 0f;
            bool refrButton = false;
            if (latestModGraph != null) if (FieldDesignWindow.Get != null) refrButton = true;
            if (refrButton) lOffset = 28f;

            // Fowarding overlay input to function buttons
            if (cursorOutOfGraphEventForward != null)
            {
                Event.current = cursorOutOfGraphEventForward;
                cursorOutOfGraphEventForward = null;
            }


            Rect up = new Rect(0f, TopMarginOffset - 3, 24, 20);
            Rect r = new Rect(up);

            r.position += new Vector2(10 + lOffset, 10);

            if (GUI.Button(r, new GUIContent(FGUI_Resources.TexTargetingIcon, "Reset view to center"), FGUI_Resources.ButtonStyle))
            {
                ResetGraphPosition();
                if (Event.current != null) Event.current.Use();
            }

            if (refrButton)
            {
                r = new Rect(up);
                r.position += new Vector2(10, 10);

                if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Refresh, "Generating preview objects if using FieldDesigner window'"), FGUI_Resources.ButtonStyle))
                {
                    RequestPlannerRefresh();
                    if (Event.current != null) Event.current.Use();
                }
            }

            #region Hourglass

            //if (latestPlanner)
            //{
            //    if (latestPlanner.ParentBuildPlanner)
            //    {
            //        if (latestPlanner.ParentBuildPlanner.IsGenerating)
            //        {
            //            r = new Rect(up);
            //            //float elaps = (float)EditorApplication.timeSinceStartup;
            //            r.size = new Vector2(22, 22);
            //            r.position += new Vector2(16,38);//ew Vector2(4+Mathf.Cos(elaps * 6f) * 3f, 30);
            //            Color preC = GUI.color;
            //            GUI.color = new Color(1f, 1f, 1f, 0.4f);
            //            GUI.DrawTexture(r, FGUI_Resources.TexWaitIcon);
            //            GUI.color = preC;
            //        }
            //    }
            //}

            #endregion

            r = new Rect(up);
            r.position += new Vector2(38 + lOffset, 10);

            if (!PGGPlanner_NodeBase.AutoSnap) GUI.backgroundColor = Color.gray;
            if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Drag, "Enable/Disable auto-trigger port connection creation with lastest trigger node"), FGUI_Resources.ButtonStyle))
            //if (GUI.Button(r, new GUIContent(FGUI_Resources.Tex_Drag, "Enable/Disable auto-snap node position on connection creation"), FGUI_Resources.ButtonStyle))
            {
                PGGPlanner_NodeBase.AutoSnap = !PGGPlanner_NodeBase.AutoSnap;
                if (Event.current != null) Event.current.Use();
            }
            GUI.backgroundColor = Color.white;

            if (TopTitle != "")
            {
                float centerX = graphDisplayRect.width / 2f;
                Rect titleR = new Rect(centerX - 250f, TopMarginOffset - 3, 500f, 38f);
                GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.2f);
                GUI.Label(titleR, TopTitle, FGUI_Resources.HeaderStyle);
                GUI.color = Color.white;
            }

            r.position -= new Vector2(60, 0);
            r.width += 64;
            r.height += 10;
            refrButtonRect = r;

            if (currentSetup is PlannerFunctionNode)
            {
                PlannerFunctionNode func = currentSetup as PlannerFunctionNode;
                r.position = new Vector2(graphDisplayRect.width - 160, r.position.y);
                r.width = 150;
                r.height = 18;
                EditorGUI.ObjectField(r, func, typeof(PlannerFunctionNode), false);
                r.width -= 20;
                r.height += 6;
                objRefRect = r;
            }
            else if (currentSetup is FieldPlanner)
            {
                if (DrawedInsideInspector == false)
                {
                    FieldPlanner func = currentSetup as FieldPlanner;
                    r.position = new Vector2(graphDisplayRect.width - 160, r.position.y);
                    r.width = 150;
                    r.height = 18;
                    EditorGUI.ObjectField(r, func, typeof(FieldPlanner), false);
                    r.width -= 20;
                    objRefRect = r;
                }
            }
            else
            {
                objRefRect = new Rect();
            }

        }


        public override FGraph_NodeBase_Drawer GetNodeDrawer(FGraph_NodeBase node)
        {
            if (node is PGGPlanner_ExecutionNode)
            {
                return new PlannerExecutionNode_Drawer(node);
            }
            else
                return new PlannerNode_Drawer(node);
        }

        public bool CheckDisplayRepaintRequest(double repaintID)
        {
            if (_LatestRefreshDisplayFlag != repaintID)
            {
                ForceGraphRepaint();
                _LatestRefreshDisplayFlag = repaintID;
                return true;
            }

            return false;
        }
    }


}