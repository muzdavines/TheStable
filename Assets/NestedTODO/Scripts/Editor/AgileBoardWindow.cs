using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NestedTODO
{
    public class AgileBoardWindow : EditorWindow
    {
        public static AgileBoardWindow window;
        static List<Board> boards = new List<Board>();
        public static List<Board> Boards { get { return boards; } }

        public static NestedList nestedList;

        Card ActiveCard;

        List<string> ViewFilter = new List<string>();
        string[] defaultFilter = new string[] { "All Tasks" };
        static int viewFilter;

        public static void Refresh()
        {
            LoadNestedList();
            if (window != null)
                window.Repaint();
        }

        struct Tooltip
        {
            public Rect rect;
            public string text;

            public Tooltip(Rect r, string t)
            {
                rect = r;
                text = t;
            }
        }
        List<Tooltip> notesTooltips = new List<Tooltip>();

        const float boardBaseHeight = 40;
        const float boardWidth = 200;
        const float cardBaseHeight = 40;
        const float cardWidth = 180;
        const float cardHeigth = 50;
        const float inBetweenBoardSpace = 10;
        const float inBetweenCardSpace = 10;
        static Texture2D bookmarkIcon;

        Vector2 scrollPosition;
        Vector2 smooth;
        Color normalColor;
        Rect dragRect;

        GUIStyle cardtyle;
        Color proCard = new Color(0.25f, 0.25f, 0.25f, .75f);
        Color perCard = new Color(1, 1, 1, .75f);

        int oldBoard;

        [MenuItem(DefaultConfiguration.AgileBoardWindowRoute, false, 1)]
        static void openWindow()
        {
            window = (AgileBoardWindow)EditorWindow.GetWindow<AgileBoardWindow>();
#if UNITY_5_3_OR_NEWER
            window.titleContent.text = "Agile Board";
#else
            window.title = "Agile Board";
#endif
            window.minSize = new Vector2(2 * (boardWidth + inBetweenBoardSpace), 400);
            window.wantsMouseMove = true;
        }

        void OnEnable()
        {
            ActiveCard = null;
            oldBoard = -1;

            if (bookmarkIcon == null)
            {
                MonoScript ms = MonoScript.FromScriptableObject(this);
                var rootFolderPath = AssetDatabase.GetAssetPath(ms).Replace("Scripts/Editor/AgileBoardWindow.cs", "");
                bookmarkIcon = AssetDatabase.LoadAssetAtPath(rootFolderPath + "Icons/bookmark.png", typeof(Texture2D)) as Texture2D;
            }

            if(ChecklistWindow.nestedList != null)
            {
                nestedList = ChecklistWindow.nestedList;
            }
            else if (EditorPrefs.HasKey("NestedList"))
            {
                nestedList = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("NestedList"), typeof(NestedList)) as NestedList;
            }

            Undo.undoRedoPerformed += this.PerdormUndo;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void ReloadNestedListAfterCompilation()
        {
            if (ChecklistWindow.nestedList != null)
            {
                nestedList = ChecklistWindow.nestedList;
            }
            else if (EditorPrefs.HasKey("NestedList"))
            {
                nestedList = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("NestedList"), typeof(NestedList)) as NestedList;
            }
        }

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= this.PerdormUndo;
        }

        void OnFocus()
        {
            if (nestedList != null)
                LoadNestedList();

            NestedList.UpdateViewFilter(ref ViewFilter, defaultFilter, nestedList);
        }

        void PerdormUndo()
        {
            Refresh();
        }

        public static void LoadNestedList()
        {
            if (nestedList == null)
                return;

            boards.Clear();
            int listCount = nestedList.List.Count;
            for (int i = 0; i < nestedList.boards.Count; i++)
            {
                var tmpBoard = new Board(nestedList.boards[i].title, nestedList.boards[i].wip);
                tmpBoard.cards.Clear();
                boards.Add(tmpBoard);
            }

            //pre cache cards count
            //int[] n = new int[boards.Count];
            List<Vector2> cardsPositions = new List<Vector2>();
            for (int i = 1; i < listCount; i++)
            {
                var node = nestedList.List[i];
                if (node.SetToRewrite)
                    continue;

                if (node.ChildIds.Count == 0 && !node.Completed && node.boardPosition.y >= 0)
                {
                    //n[(int)node.boardPosition.x]++;
                    if(node.boardPosition.x >= 0)
                        cardsPositions.Add(node.boardPosition);
                }
            }

            //fill positions with placeholder cards
            for(int i = 0; i < cardsPositions.Count; i++)
            {
                boards[(int)cardsPositions[i].x].cards.Add(new Card());
            }

            //fill cards positions
            for (int i = 1; i < listCount; i++)
            {
                var node = nestedList.List[i];
                if (node.SetToRewrite)
                    continue;

                //only show leaf nodes
                if (node.ChildIds.Count == 0 && !node.Completed)
                {
                    int boardIndex = (int)node.boardPosition.x;
                    int cardPosition = (int)node.boardPosition.y;

                    //check view filter for board 0
                    
                    if(boardIndex == 0 && viewFilter > 0)
                    {
                        bool skip = true;
                        int categoryCount = nestedList.Configuration.categories.Count;
                        int priorityCount = nestedList.Configuration.priorities.Count;
                        if (viewFilter < categoryCount + 1)
                        {
                            if (node.Category == viewFilter - 1)
                            {
                                skip = false;
                            }
                        }
                        else if (viewFilter >= categoryCount + 1 && viewFilter < priorityCount + categoryCount + 1)
                        {
                            if (node.Priority == viewFilter - (categoryCount + 1))
                            {
                                skip = false;
                            }
                        }
                        else
                        {
                            if (node.Tag == viewFilter - (priorityCount + categoryCount + 1))
                            {
                                skip = false;
                            }
                        }
                        if (skip)
                            continue;
                    }


                    if (cardPosition >= 0)
                    {
                        //cannot be placed due to wip limits in the target board, sent to backlog
                        if (boards[boardIndex].wip > 0 && cardPosition >= boards[boardIndex].wip)
                            boards[0].cards.Add(new Card(node));
                        else
                        {
                            //in case more two or more cards share the same position, set the first occurrence 
                            //and send the rest to the backlog
                            if(cardPosition >= boards[boardIndex].cards.Count)
                                boards[0].cards.Add(new Card(node));
                            else if (boards[boardIndex].cards[cardPosition].node == null)
                                boards[boardIndex].cards[cardPosition].node = node;
                            else
                                boards[0].cards.Add(new Card(node));
                        }
                    }
                    else
                    {
                        //cannot be placed due to wip limits in the target board, sent to backlog
                        if (boards[boardIndex].wip > 0 && cardPosition >= boards[boardIndex].wip)
                            boards[0].cards.Add(new Card(node));
                        else
                            boards[boardIndex].cards.Add(new Card(node));
                    }
                }
            }

            //clear empty slots
            for(int b = 0; b < boards.Count; b++)
            {
                int c = 0;
                while(c < boards[b].cards.Count)
                {
                    if (boards[b].cards[c].node == null)
                        boards[b].cards.RemoveAt(c);
                    else
                        c++;
                }
            }
        }

        void Update()
        {
            //keep repainting while moving a card
            if (ActiveCard != null)
                Repaint();
        }

        void OnGUI()
        {
            cardtyle = new GUIStyle(GUI.skin.button);
            cardtyle.alignment = TextAnchor.MiddleLeft;

            EditorGUI.BeginChangeCheck();
            nestedList = (NestedList)EditorGUI.ObjectField(new Rect(0, 0, this.position.width - 130, 16), nestedList, typeof(NestedList), false);
            if (EditorGUI.EndChangeCheck())
            {
                viewFilter = 0;
                LoadNestedList();
                NestedList.UpdateViewFilter(ref ViewFilter, defaultFilter, nestedList);
                ChecklistWindow.nestedList = nestedList;
                ChecklistWindow.Refresh();

                if (nestedList == null)
                    EditorPrefs.DeleteKey("NestedList");
                else
                    EditorPrefs.SetString("NestedList", AssetDatabase.GetAssetPath(nestedList));
            }

            if (nestedList == null)
                GUI.enabled = false;

            GUI.enabled = true;
            if (nestedList == null)
            {
                EditorGUI.HelpBox(new Rect(0, 15, this.position.width, 40), "A NestedList is needed, please select one or create it using the Checklist Window.", MessageType.Info);
                return;
            }
            else
            {
                nestedList.SerializedListObject.Update();
            }

            notesTooltips.Clear();
            normalColor = GUI.backgroundColor;

            if (GUI.Button(new Rect(0, 17, this.position.width, 17), "Manage Columns", EditorStyles.toolbarButton))
                ManageColumnsWindow.openWindow(this.position);

            int scrollHeight = 0;
            for (int i = 0; i < boards.Count; i++)
            {
                if (boards[i].cards.Count > scrollHeight)
                    scrollHeight = boards[i].cards.Count;
            }

            scrollPosition = GUI.BeginScrollView(new Rect(0, boardBaseHeight, this.position.width, this.position.height - boardBaseHeight), scrollPosition, new Rect(0, boardBaseHeight, 10 + boards.Count * (boardWidth + inBetweenBoardSpace), boardBaseHeight + cardBaseHeight + scrollHeight * (cardHeigth + inBetweenCardSpace)));
            for (int i = 0; i < boards.Count; i++)
            {
                DrawBoard(i);
            }

            //draw active card
            if (ActiveCard != null)
            {
                var mousePos = Event.current.mousePosition;

                int boardIndex = -1;
                int cardIndex = 0;

                //get board index
                for (int i = 0; i < boards.Count; i++)
                {
                    if (boards[i].boardRect.Contains(mousePos))
                        boardIndex = i;
                }

                //get card index
                if (boardIndex >= 0)
                {
                    while (cardIndex < boards[boardIndex].cards.Count)
                    {
                        if (mousePos.y < boards[boardIndex].cards[cardIndex].cardRect.center.y)
                            break;
                        else
                            cardIndex++;
                    }
                }

                int activeIndex = -1;
                if (boardIndex == oldBoard)
                    activeIndex = boards[boardIndex].cards.IndexOf(ActiveCard);

                if (Event.current.type == EventType.MouseUp)
                {
                    if (boardIndex >= 0)
                    {
                        if (boards[boardIndex].wip == 0 || boards[boardIndex].cards.Count < boards[boardIndex].wip)
                        {
                            if (activeIndex < 0)
                                boards[oldBoard].cards.Remove(ActiveCard);

                            if (activeIndex >= 0 && activeIndex > cardIndex)
                                boards[oldBoard].cards.Remove(ActiveCard);

                            if (cardIndex < boards[boardIndex].cards.Count)
                                boards[boardIndex].cards.Insert(cardIndex, ActiveCard);
                            else
                                boards[boardIndex].cards.Add(ActiveCard);

                            if (activeIndex >= 0 && activeIndex <= cardIndex)
                                boards[oldBoard].cards.Remove(ActiveCard);

                            if (boardIndex == boards.Count - 1)
                            {
                                var serializedCard = nestedList.SerializedNode(ActiveCard.cardId);
                                serializedCard.FindPropertyRelative("points").floatValue = 0;
                                nestedList.SaveSerializedProperties();
                            }
                        }
                    }
                    ActiveCard = null;
                    oldBoard = -1;
                    Repaint();
                }

                if (ActiveCard != null)
                {
                    var offset = new Vector2(mousePos.x - dragRect.center.x, mousePos.y - dragRect.center.y);
                    dragRect.position = Vector2.SmoothDamp(dragRect.position, mousePos + offset, ref smooth, 1f, 100, Time.deltaTime);
                    DrawCard(dragRect, ActiveCard, cardtyle);
                }
            }
            else
            {
                for (int i = 0; i < notesTooltips.Count; i++)
                {
                    if (notesTooltips[i].rect.Contains(Event.current.mousePosition))
                    {
                        Vector2 p = Event.current.mousePosition;
                        p.y -= 20;

                        Vector2 size = EditorStyles.label.CalcSize(new GUIContent(notesTooltips[i].text));
                        Rect rect = new Rect(p.x, p.y, size.x, size.y);

                        Color c = EditorGUIUtility.isProSkin ? new Color(.25f, .25f, .25f, 1) : new Color(.85f, .85f, .85f, 1);
                        EditorGUI.DrawRect(rect, c);
                        EditorGUI.LabelField(rect, notesTooltips[i].text);
                    }
                }
            }

            GUI.EndScrollView();
        }

        void DrawBoard(int boardIndex)
        {
            var board = boards[boardIndex];

            board.boardRect = new Rect(
                10 + (inBetweenBoardSpace + boardWidth) * boardIndex,
                boardBaseHeight,
                boardWidth,
                100 + ((board.wip > 0 ? board.wip - 1 : board.cards.Count)) * (inBetweenCardSpace + cardHeigth));

            if (ActiveCard != null && board.boardRect.Contains(Event.current.mousePosition))
            {
                if (board.wip == 0 || board.cards.Count < board.wip)
                    GUI.backgroundColor = Color.yellow;
                else
                    GUI.backgroundColor = Color.red;
            }

#if UNITY_2019_3_OR_NEWER
            GUI.Box(board.boardRect, "");
#else
            GUI.Box(board.boardRect, "", EditorStyles.helpBox);
#endif
            GUI.backgroundColor = normalColor;

            var titleRect = new Rect(board.boardRect.x, board.boardRect.y, boardWidth - 50, 20);
            var tmpStyle = new GUIStyle(EditorStyles.largeLabel);
            tmpStyle.fontStyle = FontStyle.Bold;
            tmpStyle.alignment = TextAnchor.MiddleLeft;
            tmpStyle.contentOffset = new Vector2(2, 4);
            GUI.Label(titleRect, board.title, tmpStyle);

            //view filter
            if(boardIndex == 0)
            {
                Rect popupRect = board.boardRect;
                popupRect.width = 70;
                popupRect.height = 20;
                popupRect.x += board.boardRect.width - popupRect.width - 5;
                popupRect.y += 7;
                EditorGUI.BeginChangeCheck();
                viewFilter = EditorGUI.Popup(popupRect, viewFilter, ViewFilter.ToArray());
                if (EditorGUI.EndChangeCheck())
                    Refresh();
            }


            //draw bookmark button
            if (boardIndex == boards.Count - 1 && board.cards.Count > 0)
            {
                GUI.backgroundColor = Color.grey;
                if (GUI.Button(new Rect(board.boardRect.x + boardWidth * 0.825f, boardBaseHeight + 6, 30, 30), bookmarkIcon, EditorStyles.toolbarButton))
                {
                    for (int i = 0; i < board.cards.Count; i++)
                    {
                        nestedList.SerializedNode(board.cards[i].node).FindPropertyRelative("completed").boolValue = true;
                        nestedList.SaveSerializedProperties();
                        if (nestedList.Configuration.AutoComplete)
                        {
                            nestedList.MarkBranch(board.cards[i].node, board.cards[i].node.Completed);
                            nestedList.TellParentToCheckChilds(board.cards[i].node.NodeID);
                        }
                    }
                    LoadNestedList();
                    return;
                }
                GUI.backgroundColor = normalColor;
            }

            float sumPoints = 0;
            for (int i = 0; i < board.cards.Count; i++)
            {
                var card = board.cards[i];
                if (card.node == null)
                    continue;

                sumPoints += card.node.points;

                //check if card is in a diferent position than last time, and save new position
                if (card.node.boardPosition != new Vector2(boardIndex, i))
                {
                    nestedList.SerializedNode(card.cardId).FindPropertyRelative("boardPosition").vector2Value = new Vector2(boardIndex, i);
                    nestedList.SaveSerializedProperties();
                }

                if (card != ActiveCard)
                {
                    Rect tmpRect = new Rect();
                    tmpRect.x = 20 + (inBetweenBoardSpace + boardWidth) * boardIndex;
                    tmpRect.y = boardBaseHeight + cardBaseHeight + (cardHeigth + inBetweenCardSpace) * i;
                    tmpRect.width = cardWidth;
                    tmpRect.height = cardHeigth;

                    card.cardRect = tmpRect;
                    DrawCard(card.cardRect, card, cardtyle);
                }

                if (Event.current.type == EventType.MouseDown && card.cardRect.Contains(Event.current.mousePosition))
                {
                    ActiveCard = card;
                    dragRect = card.cardRect;
                    oldBoard = boardIndex;
                    ChecklistWindow.activeNode = card.node;
                    ChecklistWindow.Refresh();
                }
            }
            var pointsRect = new Rect(board.boardRect.x, boardBaseHeight + 22, boardWidth, 16);
            tmpStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            tmpStyle.contentOffset = new Vector2(4, 0);
            GUI.Label(pointsRect, string.Concat("[", board.wip.ToString(), "]"), tmpStyle);

            tmpStyle.alignment = TextAnchor.MiddleRight;
            tmpStyle.contentOffset = new Vector2(-4, 0);
            GUI.Label(pointsRect, string.Format("{0}/{1}", sumPoints.ToString(), nestedList.SumNodePoints()), tmpStyle);
        }

        void DrawCard(Rect pos, Card card, GUIStyle style)
        {
            //draw card frame
            EditorGUI.DrawRect(pos, Color.black);

            //draw card inner color
            var offset = pos;
            offset.x += 1;
            offset.y += 1;
            offset.width -= 2;
            offset.height -= 2;
            EditorGUI.DrawRect(offset, EditorGUIUtility.isProSkin ? proCard : perCard);

            //draw priority stripe
            var stripeRect = offset;
            stripeRect.width = 4;
            EditorGUI.DrawRect(stripeRect, nestedList.Configuration.priorities[card.cardPriority].priorityColor);

            //draw card text
            offset.x += 8;
            offset.width -= 6;
            offset.height = 30;
            var tmpStyle = new GUIStyle(EditorStyles.label);
            tmpStyle.wordWrap = true;
            EditorGUI.LabelField(offset, card.text, tmpStyle);

            ////draw card note
            if (card.tooltip.Length > 0)
            {
                var tooltipRect = stripeRect;
                tooltipRect.x += 10;
                tooltipRect.y += 30;
                tooltipRect.width = 15;
                tooltipRect.height = 15;

                notesTooltips.Add(new Tooltip(tooltipRect, card.tooltip));

                EditorGUI.DrawRect(tooltipRect, nestedList.Configuration.getColor(6));
            }

            //draw parent
            if (card.node.ParentID > 0)
            {
                var parentRect = stripeRect;
                parentRect.x += card.tooltip.Length > 0 ? 25 : 10;
                parentRect.y += 30;
                parentRect.width = card.tooltip.Length > 0 ? 125 : 150;
                parentRect.height = 15;
                tmpStyle = new GUIStyle(EditorStyles.miniLabel);

                tmpStyle.wordWrap = false;
                tmpStyle.clipping = TextClipping.Clip;
                EditorGUI.LabelField(parentRect, nestedList.getParent(card.node).Title, tmpStyle);
            }

            //draw card points
            offset.x += 130;
            offset.y += 29;
            offset.width = 38;
            offset.height = 20;

#if UNITY_2019_3_OR_NEWER
            if (GUI.Button(offset, card.Points, EditorStyles.miniButton))
#else
            if (GUI.Button(offset, card.Points, EditorStyles.toolbarButton))
#endif
            {
                PointsWindow.openWindow(card.node, this.position);
            }

        }

        [System.Serializable]
        public class Board
        {
            public string title;
            public List<Card> cards;
            public Rect boardRect;
            public int wip;

            public Board(string t, int w = 0)
            {
                title = t;
                cards = new List<Card>();
                boardRect = new Rect();
                wip = w;
            }
        }

        [System.Serializable]
        public class Card
        {
            public NestedNode node;
            public Rect cardRect;

            public string text { get { return node.Title; } }
            public string tooltip { get { return node.Note; } }
            public string Points { get { return node.points.ToString(); } }
            public int cardId { get { return node.NodeID; } }
            public int cardPriority { get { return node.Priority; } }

            public Card()
            {
                node = null;
                cardRect = new Rect();
            }

            public Card(NestedNode n)
            {
                node = n;
                cardRect = new Rect();
            }
        }
    }

    public class ManageColumnsWindow : EditorWindow
    {
        const int buttonWidth = 20;
        const int textWidth = 50;
        Vector2 scrollPosition;
        SerializedProperty column;

        public static void openWindow(Rect parentWindow)
        {
            var window = ScriptableObject.CreateInstance<ManageColumnsWindow>();

            window.position = new Rect(parentWindow.x + parentWindow.width / 2 - 160, parentWindow.y + parentWindow.height / 2 - 150, 320, 300);
            window.ShowPopup();
            window.Focus();
        }

        void OnLostFocus()
        {
            this.Close();
        }

        void OnGUI()
        {
            if (AgileBoardWindow.nestedList == null)
            {
                EditorGUILayout.HelpBox("There is no TODO List selected", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Mandatory Columns", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            column = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(0);
            column.FindPropertyRelative("title").stringValue = EditorGUILayout.TextField("Start Column", column.FindPropertyRelative("title").stringValue, GUILayout.MinWidth(textWidth)).Trim();
            column = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(AgileBoardWindow.nestedList.boards.Count - 1);
            column.FindPropertyRelative("title").stringValue = EditorGUILayout.TextField("Completed Column", column.FindPropertyRelative("title").stringValue, GUILayout.MinWidth(textWidth)).Trim();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Extra Columns", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Column"))
            {
                int boardIndex = AgileBoardWindow.Boards.Count - 1;

                //move all cards in the completed column one space to the right
                for (int i = 0; i < AgileBoardWindow.Boards[boardIndex].cards.Count; i++)
                {
                    var serializedNode = AgileBoardWindow.nestedList.SerializedNode(AgileBoardWindow.Boards[boardIndex].cards[i].cardId);
                    serializedNode.FindPropertyRelative("boardPosition.x").floatValue++;
                }

                //create the new column
                var serializedBoards = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards");
                serializedBoards.arraySize++;
                serializedBoards.GetArrayElementAtIndex(boardIndex).FindPropertyRelative("title").stringValue = "New Column";
                serializedBoards.GetArrayElementAtIndex(boardIndex).FindPropertyRelative("wip").intValue = 0;

                AgileBoardWindow.nestedList.SaveSerializedProperties();
                AgileBoardWindow.Refresh();
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 1; i < AgileBoardWindow.nestedList.boards.Count - 1; i++)
            {
                DrawColumn(i);
            }

            if (EditorGUI.EndChangeCheck())
            {
                AgileBoardWindow.nestedList.SaveSerializedProperties();
            }
            EditorGUILayout.EndScrollView();

        }

        void DrawColumn(int index)
        {
            column = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(index);

            EditorGUILayout.BeginHorizontal();


            if (index <= 1)
                GUI.enabled = false;
            if (GUILayout.Button(@"▲", EditorStyles.miniButtonLeft, GUILayout.Width(buttonWidth)))
            {
                //swap columns
                var columnA = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(index);
                var columnB = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(index - 1);

                string tmpString = columnA.FindPropertyRelative("title").stringValue;
                int tmpWip = columnA.FindPropertyRelative("wip").intValue;

                columnA.FindPropertyRelative("title").stringValue = columnB.FindPropertyRelative("title").stringValue;
                columnA.FindPropertyRelative("wip").intValue = columnB.FindPropertyRelative("wip").intValue;

                columnB.FindPropertyRelative("title").stringValue = tmpString;
                columnB.FindPropertyRelative("wip").intValue = tmpWip;

                //swap nodes
                for (int i = 1; i < AgileBoardWindow.nestedList.List.Count; i++)
                {
                    if (AgileBoardWindow.nestedList.getNode(i).boardPosition.y == -1)
                        continue;

                    var p = AgileBoardWindow.nestedList.SerializedNode(i).FindPropertyRelative("boardPosition.x");
                    if (AgileBoardWindow.nestedList.getNode(i).boardPosition.x == index)
                    {
                        p.floatValue--;
                    }
                    else if (AgileBoardWindow.nestedList.getNode(i).boardPosition.x == index - 1)
                    {
                        p.floatValue++;
                    }
                }
                AgileBoardWindow.nestedList.SaveSerializedProperties();
                AgileBoardWindow.LoadNestedList();
                return;
            }
            GUI.enabled = true;

            if (index >= AgileBoardWindow.nestedList.boards.Count - 2)
                GUI.enabled = false;
            if (GUILayout.Button(@"▼", EditorStyles.miniButtonRight, GUILayout.Width(buttonWidth)))
            {
                //swap columns
                var columnA = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(index);
                var columnB = AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").GetArrayElementAtIndex(index + 1);

                string tmpString = columnA.FindPropertyRelative("title").stringValue;
                int tmpWip = columnA.FindPropertyRelative("wip").intValue;

                columnA.FindPropertyRelative("title").stringValue = columnB.FindPropertyRelative("title").stringValue;
                columnA.FindPropertyRelative("wip").intValue = columnB.FindPropertyRelative("wip").intValue;

                columnB.FindPropertyRelative("title").stringValue = tmpString;
                columnB.FindPropertyRelative("wip").intValue = tmpWip;

                //swap nodes
                for (int i = 1; i < AgileBoardWindow.nestedList.List.Count; i++)
                {
                    if (AgileBoardWindow.nestedList.getNode(i).boardPosition.y == -1)
                        continue;

                    var p = AgileBoardWindow.nestedList.SerializedNode(i).FindPropertyRelative("boardPosition.x");
                    if (AgileBoardWindow.nestedList.getNode(i).boardPosition.x == index)
                    {
                        p.floatValue++;
                    }
                    else if (AgileBoardWindow.nestedList.getNode(i).boardPosition.x == index + 1)
                    {
                        p.floatValue--;
                    }
                }
                AgileBoardWindow.nestedList.SaveSerializedProperties();
                AgileBoardWindow.LoadNestedList();
                return;
            }
            GUI.enabled = true;

            EditorGUILayout.LabelField("Title:", GUILayout.Width(50));
            column.FindPropertyRelative("title").stringValue = EditorGUILayout.TextField(column.FindPropertyRelative("title").stringValue, GUILayout.MinWidth(textWidth)).Trim();
            EditorGUILayout.LabelField("WIP limit:", GUILayout.Width(70));
            column.FindPropertyRelative("wip").intValue = Mathf.Max(EditorGUILayout.IntField(column.FindPropertyRelative("wip").intValue), 0);

            if (GUILayout.Button("x", GUILayout.Width(buttonWidth)))
            {
                bool choice = EditorUtility.DisplayDialog("Delete Column", "Are you sure about deleting the selected column?", "Yes", "Cancel");
                if (choice)
                {
                    for (int i = 1; i < AgileBoardWindow.nestedList.List.Count; i++)
                    {
                        var node = AgileBoardWindow.nestedList.getNode(i);
                        var serializedNode = AgileBoardWindow.nestedList.SerializedNode(i);
                        if (node.boardPosition.x == index)
                        {
                            serializedNode.FindPropertyRelative("boardPosition").vector2Value = new Vector2(0, -1);
                        }
                        else if (node.boardPosition.x > index)
                        {
                            serializedNode.FindPropertyRelative("boardPosition.x").floatValue--;
                        }
                    }

                    AgileBoardWindow.nestedList.SerializedListObject.FindProperty("boards").DeleteArrayElementAtIndex(index);

                    AgileBoardWindow.nestedList.SaveSerializedProperties();
                    AgileBoardWindow.Refresh();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    public class PointsWindow : EditorWindow
    {
        NestedNode node;

        public static void openWindow(NestedNode n, Rect parentWindow)
        {
            var window = ScriptableObject.CreateInstance<PointsWindow>();

            window.node = n;
            window.position = new Rect(parentWindow.x + parentWindow.width / 2 - 150, parentWindow.y + parentWindow.height / 2 - 30, 300, 60);
            window.ShowPopup();
            window.Focus();
        }

        void OnGUI()
        {
            if (AgileBoardWindow.nestedList == null)
            {
                EditorGUILayout.HelpBox("There is no TODO List selected", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(node.Title, EditorStyles.boldLabel);

            var serializedNode = AgileBoardWindow.nestedList.SerializedNode(node);
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < 5; i++)
            {
                if (GUILayout.Button(Mathf.Pow(2, i).ToString()))
                {
                    serializedNode.FindPropertyRelative("points").floatValue = (int)Mathf.Pow(2, i);
                    AgileBoardWindow.nestedList.SaveSerializedProperties();
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            serializedNode.FindPropertyRelative("points").floatValue = Mathf.Max((float)System.Math.Round(EditorGUILayout.FloatField("Card Points", serializedNode.FindPropertyRelative("points").floatValue), 1), 0);
            if (EditorGUI.EndChangeCheck())
                AgileBoardWindow.nestedList.SaveSerializedProperties();
        }

        void OnLostFocus()
        {
            this.Close();
        }
    }

}