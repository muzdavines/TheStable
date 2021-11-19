using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NestedTODO
{
    public class ChecklistWindow : EditorWindow
    {
        static ChecklistWindow window;
        public static NestedList nestedList;
        public static string rootFolderPath;

        public static ListConfiguration Config
        {
            get
            {
                if (nestedList != null)
                    return nestedList.Configuration;
                else
                    return null;
            }
        }

        public static void Refresh()
        {
            if (window != null)
                window.Repaint();
        }

        List<string> ViewFilter = new List<string>();
        string[] defaultFilter = new string[] { "All Tasks", "Uncompleted", "Completed" };
        int viewFilter;

        public static NestedNode activeNode;
        int lineCounter;

        bool changesMade;
        bool expandAll = false;
        bool editLock = true;
        Vector2 scrollPosition;
        bool scrollToActiveNode = false;

        const float textEditorHeight = 125;
        const float nodeTextWidth = 300;
        const float nodeTextHeigth = 16;
        const float nodeButtonWidth = 25;
        const float indent = 20;

        string tmpCategory = "";
        string tmpPriority = "";
        string tmpTag = "";
        Color tempPriorityColor = Color.black;

        int transferSlotA = -1;
        int transferSlotB = -1;

        [MenuItem(DefaultConfiguration.ChecklistWindowRoute, false, 0)]
        static void openWindow()
        {
            window = (ChecklistWindow)EditorWindow.GetWindow<ChecklistWindow>();
#if UNITY_5_3_OR_NEWER
            window.titleContent.text = "Checklist";
#else
            window.title = "Checklist";
#endif
            window.minSize = new Vector2(275f, 500f);
        }

        void OnEnable()
        {
            window = this;
            MonoScript ms = MonoScript.FromScriptableObject(this);
            rootFolderPath = AssetDatabase.GetAssetPath(ms).Replace("Scripts/Editor/ChecklistWindow.cs", "");

            if (EditorPrefs.HasKey("NestedList"))
            {
                nestedList = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("NestedList"), typeof(NestedList)) as NestedList;
            }
            else
            {
                activeNode = null;
            }

            if (EditorPrefs.HasKey("ChecklistRearrangeMode"))
                editLock = EditorPrefs.GetBool("ChecklistRearrangeMode");

            Undo.undoRedoPerformed += window.PerformUndo;
        }

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= window.PerformUndo;
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        static void ReloadNestedListAfterCompilation()
        {
            if (AgileBoardWindow.nestedList != null)
            {
                nestedList = AgileBoardWindow.nestedList;
            }
            else if (EditorPrefs.HasKey("NestedList"))
            {
                nestedList = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("NestedList"), typeof(NestedList)) as NestedList;
            }
        }

        void PerformUndo()
        {
            activeNode = null;
            Repaint();
        }

        void OnFocus()
        {
            NestedList.UpdateViewFilter(ref ViewFilter, defaultFilter, nestedList);
        }

        void CreateNestedList()
        {
            if (!AssetDatabase.IsValidFolder(rootFolderPath + "NestedLists/"))
                AssetDatabase.CreateFolder(rootFolderPath, "NestedLists");
            
            string listPath = EditorUtility.SaveFilePanelInProject("Create NestedList File", "NewNestedList", "asset", "", rootFolderPath + "NestedLists/");

            if (listPath.Length == 0)
                return;

            NestedList nl = ScriptableObject.CreateInstance<NestedList>();
            AssetDatabase.CreateAsset(nl, listPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            nestedList = nl;
            AgileBoardWindow.nestedList = nl;

            //add a new task at creation
            nestedList.SerializedListObject.Update();
            nestedList.AddTopLevelNode();
            activeNode = nestedList.List[1];

            Refresh();
        }

        void SelectActiveNode(NestedNode node)
        {
            activeNode = node;
            GUI.FocusControl(null);

            if (nestedList.Configuration.ScrollToActiveTask)
                scrollToActiveNode = true;
        }

        void ScrollToNode(NestedNode node)
        {
            var targetScroll = 20 * (node.lineIndex - 1);

            if (Mathf.Abs(scrollPosition.y - targetScroll) > 100)
                scrollPosition.y = targetScroll;
        }

        void OnGUI()
        {
            #region load/create/export
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            nestedList = (NestedList)EditorGUILayout.ObjectField(nestedList, typeof(NestedList), false);
            if (EditorGUI.EndChangeCheck())
            {
                viewFilter = 0;
                NestedList.UpdateViewFilter(ref ViewFilter, defaultFilter, nestedList);
                AgileBoardWindow.nestedList = nestedList;
                AgileBoardWindow.Refresh();

                if (nestedList == null)
                    EditorPrefs.DeleteKey("NestedList");
                else
                    EditorPrefs.SetString("NestedList", AssetDatabase.GetAssetPath(nestedList));
            }
            if (GUILayout.Button("Create", EditorStyles.miniButton))
            {
                CreateNestedList();
            }

            if(nestedList != null)
            {
                if(GUILayout.Button("Export", EditorStyles.miniButton))
                {
                    ExportListToCSV(nestedList);
                }
            }

            if (Config == null)
                GUI.enabled = false;
            if (GUILayout.Button("Configuration", EditorStyles.miniButton, GUILayout.MaxWidth(95)))
            {
                ChecklistConfigurationWindow.OpenWindow();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            #endregion

            #region check that nested list not null
            if (nestedList == null)
            {
                EditorGUILayout.HelpBox("Select a NestedList using the object field or press the create button to make a new NestedList.", MessageType.Info);
                return;
            }
            else
            {
                nestedList.SerializedListObject.Update();
            }
            #endregion

            changesMade = false;

            DrawGUIExtensions.DrawFixedVerticalSpace(5);

            #region tool bar
            EditorGUILayout.BeginHorizontal();

            //draw an empty rect for spacing
            DrawGUIExtensions.DrawFixedHorizontalSpace(1);

            //draw an empty space and then get its rect to draw the progress bar
            DrawGUIExtensions.DrawFixedHorizontalSpace(200);
            if (nestedList.Progress >= 0)
            {
                var rec = GUILayoutUtility.GetLastRect();
                rec.size = new Vector2(rec.size.x, 20);
                EditorGUI.ProgressBar(rec, nestedList.Progress, string.Concat(Mathf.RoundToInt(nestedList.Progress * 100).ToString(), "%"));
            }

            //draw an expandable empty rect for spacing
            DrawGUIExtensions.DrawExpandableHorizontalSpace();

            viewFilter = EditorGUILayout.Popup(viewFilter, ViewFilter.ToArray(), GUILayout.MaxWidth(200));

            EditorGUILayout.EndHorizontal();
            #endregion

            DrawGUIExtensions.DrawFixedVerticalSpace(10);

            #region menu buttons
            DrawToolbarButtons();

            if (transferSlotA > 0 && nestedList.getNode(transferSlotA).Depth != 1)
            {
                if (GUILayout.Button("Transfer to Top Level"))
                {
                    transferSlotB = 0;
                    nestedList.TransferNode(transferSlotA, transferSlotB);
                    transferSlotA = -1;
                    transferSlotB = -1;
                }
            }
            #endregion

            DrawGUIExtensions.DrawFixedVerticalSpace(5);

            #region draw list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            lineCounter = 1;
            DrawList(0);
            EditorGUILayout.Separator();
            EditorGUILayout.EndScrollView();
            #endregion

            #region node editor
            if (activeNode != null && nestedList.SerializedNode(activeNode) != null)
            {
                DrawTaskEditPanel();
            }
            #endregion


            if (changesMade)
            {
                nestedList.SaveSerializedProperties();
            }
        }

        void DrawToolbarButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button((expandAll) ? "Expand" : "Collapse", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                nestedList.ExpandCollapseAllBranches(expandAll);
                expandAll = !expandAll;
            }
            if (!nestedList.Configuration.ShowExtendedToolbar)
            {
                if (GUILayout.Button("+", EditorStyles.toolbarButton))
                {
                    nestedList.SaveSerializedProperties();
                    if (nestedList.Configuration.AutoSelectNewTask)
                        SelectActiveNode(nestedList.AddTopLevelNode());
                    else
                        nestedList.AddTopLevelNode();
                }
            }
            else
            {
                if (GUILayout.Button("+Top Level", EditorStyles.toolbarButton))
                {
                    nestedList.SaveSerializedProperties();
                    if (nestedList.Configuration.AutoSelectNewTask)
                        SelectActiveNode(nestedList.AddTopLevelNode());
                    else
                        nestedList.AddTopLevelNode();
                }

                if (activeNode == null || activeNode.Depth == 1)
                    GUI.enabled = false;

                if (GUILayout.Button("+Uncle", EditorStyles.toolbarButton))
                {
                    nestedList.SaveSerializedProperties();
                    if (nestedList.Configuration.AutoSelectNewTask)
                        SelectActiveNode(nestedList.AddUncleNode(activeNode));
                    else
                        nestedList.AddUncleNode(activeNode);
                }
                GUI.enabled = true;

                if (activeNode == null)
                    GUI.enabled = false;

                if (GUILayout.Button("+Sibling", EditorStyles.toolbarButton))
                {
                    nestedList.SaveSerializedProperties();
                    if (nestedList.Configuration.AutoSelectNewTask)
                        SelectActiveNode(nestedList.AddSiblingNode(activeNode));
                    else
                        nestedList.AddSiblingNode(activeNode);
                }

                if (GUILayout.Button("+Child", EditorStyles.toolbarButton))
                {
                    nestedList.SaveSerializedProperties();
                    if (nestedList.Configuration.AutoSelectNewTask)
                        SelectActiveNode(nestedList.AddChildNode(activeNode));
                    else
                        nestedList.AddChildNode(activeNode);
                }

                GUI.enabled = true;
            }

            Color normalColor = GUI.backgroundColor;
            if (!editLock)
                GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.black : Color.gray;
            if (GUILayout.Button(editLock ? "Rearrange Off" : "Rearrange On", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                editLock = !editLock;
                EditorPrefs.SetBool("ChecklistRearrangeMode", editLock);
                transferSlotA = -1;
                transferSlotB = -1;
            }
            GUI.backgroundColor = normalColor;
            EditorGUILayout.EndHorizontal();
        }

        void DrawTaskEditPanel()
        {
            if (scrollToActiveNode)
            {
                scrollToActiveNode = false;
                ScrollToNode(activeNode);
            }

            if (GUILayout.Button("Close"))
            {
                activeNode = null;
                return;
            }

            var serializedNode = nestedList.SerializedNode(activeNode);

            DrawGUIExtensions.StartHorizontalColored(Color.grey, EditorStyles.helpBox);

            //text area
            EditorGUILayout.BeginVertical();
            var s = new GUIStyle(EditorStyles.textArea);
            s.fontSize = 14;
            float normalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("TaskTitle");
            serializedNode.FindPropertyRelative("title").stringValue = EditorGUILayout.TextField(activeNode.Title, s, GUILayout.MinWidth(100), GUILayout.Height(20)).Trim();
            serializedNode.FindPropertyRelative("notes").stringValue = EditorGUILayout.TextArea(activeNode.Note, s, GUILayout.MinWidth(100), GUILayout.Height(textEditorHeight - 20)).Trim();
            serializedNode.FindPropertyRelative("linkedFile").objectReferenceValue = EditorGUILayout.ObjectField(activeNode.LinkedFile, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
                changesMade = true;

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(200));

            #region Categories
            //create and select categoty
            EditorGUILayout.BeginHorizontal();
            tmpCategory = EditorGUILayout.TextField("Category:", tmpCategory, GUILayout.MinWidth(60));
            if (tmpCategory.Trim().Length == 0)
                GUI.enabled = false;
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(17)))
            {
                changesMade = true;
                nestedList.AddProperty("categories", string.Concat("Categories/", tmpCategory.Trim()));
                serializedNode.FindPropertyRelative("category").intValue = Config.categories.Count - 1;
                tmpCategory = "";
                NestedList.UpdateViewFilter(ref ViewFilter, defaultFilter, nestedList);
                if (Config.AutoCategory)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.SetBranchProperty(activeNode, "category", activeNode.Category);
                }
                GUI.FocusControl(null);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            //select category
            var tempArray = Config.categories.ToArray();
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = tempArray[i].Replace("Categories/", "");
            }

            EditorGUI.BeginChangeCheck();
            serializedNode.FindPropertyRelative("category").intValue = EditorGUILayout.Popup(activeNode.Category, tempArray, GUILayout.MinWidth(60));
            if (EditorGUI.EndChangeCheck())
            {
                changesMade = true;
                if (Config.AutoCategory)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.SetBranchProperty(activeNode, "category", activeNode.Category);
                }
            }
            #endregion

            #region Priorities
            //create and select priority
            EditorGUILayout.BeginHorizontal();
            tmpPriority = EditorGUILayout.TextField("Priority", tmpPriority, GUILayout.MinWidth(60));
            tempPriorityColor = EditorGUILayout.ColorField(tempPriorityColor, GUILayout.MinWidth(30), GUILayout.MaxWidth(30));
            if (tmpPriority.Trim().Length == 0)
                GUI.enabled = false;
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(17)))
            {
                changesMade = true;
                nestedList.AddProperty("priorities", string.Concat("Priorities/", tmpPriority), tempPriorityColor);
                serializedNode.FindPropertyRelative("priority").intValue = Config.priorities.Count - 1;
                tmpPriority = "";
                tempPriorityColor = Color.black;

                if (Config.AutoPriority)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.SetBranchProperty(activeNode, "priority", activeNode.Priority);
                }

                GUI.FocusControl(null);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            //select priority
            tempArray = Config.getPriorities();
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = tempArray[i].Replace("Priorities/", "");
            }

            EditorGUI.BeginChangeCheck();
            serializedNode.FindPropertyRelative("priority").intValue = EditorGUILayout.Popup(activeNode.Priority, tempArray, GUILayout.MinWidth(60));
            if (EditorGUI.EndChangeCheck())
            {
                changesMade = true;
                if (Config.AutoPriority)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.SetBranchProperty(activeNode, "priority", activeNode.Priority);
                }
            }
            #endregion

            #region Tags
            //create and select tag
            EditorGUILayout.BeginHorizontal();
            tmpTag = EditorGUILayout.TextField("Tag:", tmpTag, GUILayout.MinWidth(60));
            if (tmpTag.Trim().Length == 0)
                GUI.enabled = false;
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(17)))
            {
                changesMade = true;
                nestedList.AddProperty("tags", string.Concat("Tags/", tmpTag.Trim()));
                serializedNode.FindPropertyRelative("tag").intValue = Config.tags.Count - 1;
                tmpTag = "";
                NestedList.UpdateViewFilter(ref ViewFilter, defaultFilter, nestedList);
                if (Config.AutoTag)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.SetBranchProperty(activeNode, "tag", activeNode.Tag);
                }
                GUI.FocusControl(null);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            //select tag
            tempArray = Config.tags.ToArray();
            for (int i = 0; i < tempArray.Length; i++)
            {
                tempArray[i] = tempArray[i].Replace("Tags/", "");
            }

            EditorGUI.BeginChangeCheck();
            serializedNode.FindPropertyRelative("tag").intValue = EditorGUILayout.Popup(activeNode.Tag, tempArray, GUILayout.MinWidth(60));
            if (EditorGUI.EndChangeCheck())
            {
                changesMade = true;
                if (Config.AutoTag)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.SetBranchProperty(activeNode, "tag", activeNode.Tag);
                }
            }
            #endregion

            #region Points
            //points area
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            serializedNode.FindPropertyRelative("points").floatValue = Mathf.Max((float)System.Math.Round(EditorGUILayout.FloatField("Points:", serializedNode.FindPropertyRelative("points").floatValue), 1), 0);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                nestedList.SaveSerializedProperties();
            }
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 5; i++)
            {
                var buttonStyle = i == 0 ? EditorStyles.miniButtonLeft : i == 4 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;
                if (GUILayout.Button(Mathf.Pow(2, i).ToString(), buttonStyle))
                {
                    serializedNode.FindPropertyRelative("points").floatValue = (int)Mathf.Pow(2, i);
                    nestedList.SaveSerializedProperties();
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUIUtility.labelWidth = normalLabelWidth;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }

        void DrawList(int nodeID)
        {
            NestedNode node = nestedList.List[nodeID];

            if (node.SetToRewrite)
                return;

            if (node.Depth > 0)
            {
                bool draw = false;
                bool pass = false;

                bool drawNode = nestedList.CheckFilterVisibility(node, viewFilter, ref pass, ref draw);

                if (drawNode && !pass && Config.HideParents)
                    drawNode = false;

                if (drawNode)
                {
                    DrawListElement(node, draw);
                }

                if (node == activeNode && (!draw || !drawNode))
                    activeNode = null;
            }

            for (int i = 0; i < node.ChildIds.Count; i++)
            {
                DrawList(node.ChildIds[i]);
            }

        }

        void DrawListElement(NestedNode node, bool drawActive)
        {
            if (node == null)
                return;

            if (!nestedList.CheckBranchVisibility(node))
                return;

            if (!drawActive)
                GUI.enabled = false;

            var serializedNode = nestedList.SerializedNode(node.NodeID);
            SerializedProperty tmpProperty;

            if (node.lineIndex != lineCounter)
                node.lineIndex = lineCounter;

            var normalColor = GUI.color;
            Color tmpColor;

            Rect tmpRect = EditorGUILayout.BeginHorizontal();

            #region show childs and node indent
            DrawGUIExtensions.DrawFixedHorizontalSpace(node.Depth * indent);
            if (node.ChildIds.Count > 0)
            {
                tmpRect = GUILayoutUtility.GetLastRect();
                tmpRect.size = new Vector2(17, 17);
                tmpRect.position = new Vector2(tmpRect.position.x + ((node.Depth - 1) * indent) + 7, tmpRect.position.y + 7);
                EditorGUI.BeginChangeCheck();
                serializedNode.FindPropertyRelative("showChilds").boolValue = EditorGUI.Foldout(tmpRect, node.ShowChilds, "");
                if (EditorGUI.EndChangeCheck())
                    changesMade = true;

            }
            #endregion

            #region color skin selection
            if (node == activeNode)
                tmpColor = Config.getColor(0);
            else
                tmpColor = lineCounter % 2 != 0 ? Config.getColor(1) : Config.getColor(2);

            if (!drawActive)
                tmpColor.a = 0.1f;

            //draw line container
#if UNITY_5_3_OR_NEWER
            tmpRect = DrawGUIExtensions.StartHorizontalColored(EditorGUIUtility.isProSkin ? Color.black : Color.white, GUI.skin.box);
#else
            tmpRect = DrawGUIExtensions.StartHorizontalColored(EditorGUIUtility.isProSkin ? Color.black : Color.white, EditorStyles.helpBox);
#endif
            int offset = 1;
            tmpRect.Set(tmpRect.position.x + offset, tmpRect.position.y + offset, tmpRect.size.x - offset * 2, tmpRect.size.y - offset * 2);
            //draw line color overlay
            EditorGUI.DrawRect(tmpRect, tmpColor);
            #endregion

            #region node completed
            EditorGUI.BeginChangeCheck();
            bool state = EditorGUILayout.Toggle(node.Completed, GUILayout.Width(15));
            bool oldState = serializedNode.FindPropertyRelative("completed").boolValue;
            string oldDate = serializedNode.FindPropertyRelative("completedAt").stringValue;

            //check for change
            if (oldState != state)
            {

                if (state)
                {
                    //only write date if no date found
                    if (string.IsNullOrEmpty(oldDate))
                    {
                        serializedNode.FindPropertyRelative("completedAt").stringValue = System.DateTime.Now.ToShortDateString();
                    }
                }
                else
                {
                    //empty date if task is marked as uncompleted
                    serializedNode.FindPropertyRelative("completedAt").stringValue = string.Empty;
                }
            }

            serializedNode.FindPropertyRelative("completed").boolValue = state;

            if (EditorGUI.EndChangeCheck())
            {
                changesMade = true;
                if (Config.AutoComplete)
                {
                    nestedList.SaveSerializedProperties();
                    nestedList.MarkBranch(node, node.Completed);
                    nestedList.TellParentToCheckChilds(node.NodeID);
                }
            }

            #endregion

            #region node notes mark
            if (!Config.HideNotes)
            {
                if (GUILayout.Button(new GUIContent("", node.Note), EditorStyles.label, GUILayout.Width(12)))
                    SelectActiveNode(node);

                tmpRect = GUILayoutUtility.GetLastRect();
                tmpRect.Set(tmpRect.position.x, tmpRect.position.y + 2, 12, 12);
                if (node.Note.Trim().Length > 0)
                    GUI.color = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                else
                    GUI.color = new Color(0, 0, 0, 0);
                EditorGUI.LabelField(tmpRect, "", GUI.skin.box);
                GUI.color = normalColor;
                if (node.Note.Trim().Length > 0)
                {
                    tmpRect.Set(tmpRect.position.x + offset, tmpRect.position.y + offset, tmpRect.size.x - offset * 2, tmpRect.size.y - offset * 2);
                    EditorGUI.DrawRect(tmpRect, Config.skinColors[6]);
                }
            }
            #endregion

            #region node text
            if (node.Completed && node != activeNode)
                tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_COMPLETED);
            else if (node == activeNode)
                tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_HIGHLIGHT);
            else
                tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_UNCOMPLETED);

            if (!drawActive)
                tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_COMPLETED);

#if UNITY_5_3_OR_NEWER
            string nodeText = string.Concat("<color=#", ColorUtility.ToHtmlStringRGBA(tmpColor), ">", node.Title, "</color>");
#else
            string hexColor = (Mathf.RoundToInt(tmpColor.r * 255)).ToString("x2") + (Mathf.RoundToInt(tmpColor.g * 255)).ToString("x2") + (Mathf.RoundToInt(tmpColor.b * 255)).ToString("x2") + (Mathf.RoundToInt(tmpColor.a * 255)).ToString("x2");
            string nodeText = string.Concat("<color=#", hexColor.ToUpper(), ">", node.Title, "</color>");
#endif
            if (GUILayout.Button("", EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.MinWidth(40)))
                SelectActiveNode(node);

            tmpRect = GUILayoutUtility.GetLastRect();
            GUIStyle t = new GUIStyle(EditorStyles.label);
            t.richText = true;
            t.wordWrap = false;
            t.clipping = TextClipping.Clip;
            t.fixedWidth = tmpRect.width;
            EditorGUI.LabelField(tmpRect, nodeText, t);
            #endregion

            #region completed at date
            if (!Config.HideCompletedAt)
            {
                tmpProperty = serializedNode.FindPropertyRelative("completedAt");

                if (node.Completed && node != activeNode)
                    tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_COMPLETED);
                else if (node == activeNode)
                    tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_HIGHLIGHT);
                else
                    tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_UNCOMPLETED);

                if (!drawActive)
                    tmpColor = Config.getColor((int)ListConfiguration.SkinColors.TEXT_COMPLETED);

                nodeText = string.Concat("<color=#", ColorUtility.ToHtmlStringRGBA(tmpColor), ">", tmpProperty.stringValue, "</color>");
                EditorGUILayout.LabelField(nodeText, t, GUILayout.MaxWidth(65));
            }
            #endregion

            #region priority levels
            if (!Config.HidePriorityLevels)
            {
                tmpProperty = serializedNode.FindPropertyRelative("priority");
                GUI.color = Config.priorities[node.Priority].priorityColor;
                if (node.Completed)
                    GUI.enabled = false;
                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button(new GUIContent("", null, Config.priorities[node.Priority].priorityText.Replace("Priorities/", "")), EditorStyles.radioButton, GUILayout.Width(15)))
                    tmpProperty.intValue = (tmpProperty.intValue + 1) % Config.priorities.Count;
                if (EditorGUI.EndChangeCheck())
                {
                    changesMade = true;
                    if (Config.AutoPriority)
                    {
                        nestedList.SaveSerializedProperties();
                        nestedList.SetBranchProperty(node, "priority", node.Priority);
                    }
                }
                GUI.enabled = true;
                GUI.color = normalColor;
            }
            #endregion
        
            #region category popup
            if (!Config.HideCategories)
            {
                tmpProperty = serializedNode.FindPropertyRelative("category");
                var categories = Config.categories.ToArray();
                int longestCatergoryLength = 0;
                for (int i = 0; i < categories.Length; i++)
                {
                    categories[i] = categories[i].Replace("Categories/", "");
                    if (categories[i].Length > longestCatergoryLength)
                        longestCatergoryLength = categories[i].Length;
                }
                EditorGUI.BeginChangeCheck();
                int categoryMaxWidth = Mathf.Min(10 * longestCatergoryLength, 150);
                tmpProperty.intValue = EditorGUILayout.Popup(tmpProperty.intValue, categories, GUILayout.MaxWidth(categoryMaxWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    changesMade = true;
                    if (Config.AutoCategory)
                    {
                        nestedList.SaveSerializedProperties();
                        nestedList.SetBranchProperty(node, "category", node.Category);
                    }
                }
            }
            #endregion

            #region tag popup
            if (!Config.HideTags)
            {
                tmpProperty = serializedNode.FindPropertyRelative("tag");
                var tags = Config.tags.ToArray();
                int longestTagLenght = 0;
                for (int i = 0; i < tags.Length; i++)
                {
                    tags[i] = tags[i].Replace("Tags/", "");
                    if (tags[i].Length > longestTagLenght)
                        longestTagLenght = tags[i].Length;
                }
                EditorGUI.BeginChangeCheck();
                int maxTagWidth = Mathf.Min(10 * longestTagLenght, 150);
                tmpProperty.intValue = EditorGUILayout.Popup(tmpProperty.intValue, tags, GUILayout.MaxWidth(maxTagWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    changesMade = true;
                    if (Config.AutoTag)
                    {
                        nestedList.SaveSerializedProperties();
                        nestedList.SetBranchProperty(node, "tag", node.Tag);
                    }
                }
            }
            #endregion

            #region node progress
            if (!Config.HideProgressBars)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.ExpandWidth(false), GUILayout.Width(30), GUILayout.Height(17));
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                float progress = nestedList.GetBranchProgress(node);
                EditorGUI.ProgressBar(GUILayoutUtility.GetLastRect(), progress, string.Concat(Mathf.RoundToInt(progress * 100).ToString(), ""));
            }
            #endregion

            #region buttons
            if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(nodeButtonWidth)))
            {
                if (nestedList.Configuration.AutoSelectNewTask)
                    SelectActiveNode(nestedList.AddChildNode(node));
                else
                    nestedList.AddChildNode(node);
            }

            if (!editLock)
            {

                if (drawActive)
                {
                    if (!nestedList.CanMoveUp(node))
                        GUI.enabled = false;
                    if (GUILayout.Button(@"▲", EditorStyles.miniButtonMid, GUILayout.Width(nodeButtonWidth - 5)))
                    {
                        nestedList.MoveNodeUp(node);
                        return;
                    }
                    GUI.enabled = true;

                    if (!nestedList.CanMoveDown(node))
                        GUI.enabled = false;
                    if (GUILayout.Button(@"▼", EditorStyles.miniButtonMid, GUILayout.Width(nodeButtonWidth - 5)))
                    {
                        nestedList.MoveNodeDown(node);
                        return;
                    }
                    GUI.enabled = true;
                }

                if (node.NodeID == transferSlotA)
                    GUI.color = Color.grey;
                if (GUILayout.Button("T", EditorStyles.miniButtonMid, GUILayout.Width(nodeButtonWidth - 5)))
                {

                    if (transferSlotA < 0)
                        transferSlotA = node.NodeID;
                    else if (transferSlotA > 0 && transferSlotB < 0)
                    {
                        transferSlotB = node.NodeID;
                        nestedList.TransferNode(transferSlotA, transferSlotB);
                        transferSlotA = -1;
                        transferSlotB = -1;
                    }
                    return;
                }
                GUI.color = normalColor;
            }
            if (GUILayout.Button("x", EditorStyles.miniButtonRight, GUILayout.Width(nodeButtonWidth)))
            {
                GUI.FocusControl(null);
                bool choice = true;
                if (node.ChildIds.Count > 0)
                {
                    if (Config.ConfirmParentDelete)
                        choice = EditorUtility.DisplayDialog("Deleting Task", "Deleting this Task will also delete all of the Sub Tasks branching form it.", "Delete", "Cancel");
                    else
                        choice = true;
                }

                if (choice)
                {
                    nestedList.DeleteNode(node);
                    activeNode = null;
                }
            }
            #endregion

            EditorGUILayout.EndHorizontal();
            lineCounter++;
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    
        void ExportListToCSV(NestedList list)
        {
            string savePath = EditorUtility.SaveFilePanel("Export List", "", list.name + ".csv", "csv");
            if (!string.IsNullOrEmpty(savePath))
            {
                //System.IO.StreamWriter streamWriter = System.IO.File.CreateText(savePath);
                ListConfiguration config = list.Configuration;
                string separator = config.CsvSeparator;
                int encodingCode = config.EncodingCode;
                if (separator.Length == 0)
                    separator = ";";
                
                System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(savePath, false, System.Text.Encoding.GetEncoding(encodingCode));

                System.Text.StringBuilder columnBuilder = new System.Text.StringBuilder();


                columnBuilder.Append("Task");
                columnBuilder.Append(separator);
                columnBuilder.Append("Completed");

                if (config.ExportCompletedAt)
                {
                    columnBuilder.Append(separator);
                    columnBuilder.Append("CompletedAt");
                }

                if (config.ExportProgress)
                {
                    columnBuilder.Append(separator);
                    columnBuilder.Append("Progress (%)");
                }

                if (config.ExportCategory)
                {
                    columnBuilder.Append(separator);
                    columnBuilder.Append("Category");
                }

                if (config.ExportPriority)
                {
                    columnBuilder.Append(separator);
                    columnBuilder.Append("Priority");
                }

                if (config.ExportTags)
                {
                    columnBuilder.Append(separator);
                    columnBuilder.Append("Tag");
                }

                streamWriter.WriteLine(columnBuilder.ToString());

                columnBuilder = null;

                ExportListElement(0, ref list, ref streamWriter);

                streamWriter.Close();

                Debug.Log("Export completed");
            }
            else
                Debug.Log("Export canceled");
			
            EditorGUIUtility.ExitGUI();
        }

        void ExportListElement(int nodeID, ref NestedList list, ref System.IO.StreamWriter streamWriter)
        {
            NestedNode node = list.List[nodeID];
            if (node.SetToRewrite)
                return;
            
            if (node.Depth > 0)
            {
                ListConfiguration config = nestedList.Configuration;
                string separator = config.CsvSeparator;
                string depth = string.Empty;
                for (int i = 1; i < node.Depth; i++)
                {
                    depth += "    ";
                }

                string task = depth + node.Title;
                string completed = node.Completed ? "X" : "";

                //default exported info
                System.Text.StringBuilder stringOutput = new System.Text.StringBuilder();
                stringOutput.Append(task);
                stringOutput.Append(separator);
                stringOutput.Append(completed);

                if (config.ExportCompletedAt)
                {
                    string completedAt = node.CompletedAt;
                    stringOutput.Append(separator);
                    stringOutput.Append(completedAt);
                }

                if (config.ExportProgress)
                {
                    float progress = (float)System.Math.Round(nestedList.GetBranchProgress(node),2) * 100;
                    stringOutput.Append(separator);
                    stringOutput.Append(progress);
                }

                if (config.ExportCategory)
                {
                    string category = Config.categories[node.Category].Replace("Categories/", "");
                    stringOutput.Append(separator);
                    stringOutput.Append(category);
                }

                if (config.ExportPriority)
                {
                    string priority = Config.priorities[node.Priority].priorityText.Replace("Priorities/", "");
                    stringOutput.Append(separator);
                    stringOutput.Append(priority);
                }

                if (config.ExportTags)
                {
                    string tag = Config.tags[node.Tag].Replace("Tags/", "");
                    stringOutput.Append(separator);
                    stringOutput.Append(tag);
                }

                streamWriter.WriteLine(stringOutput.ToString());
                stringOutput = null;
            }

            for (int i = 0; i < node.ChildIds.Count; i++)
            {
                ExportListElement(node.ChildIds[i], ref list, ref streamWriter);
            }
        }

    
    }

}