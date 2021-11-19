using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace NestedTODO
{
    [System.Serializable]
    public class NestedList : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private int idCount;

        [SerializeField, HideInInspector]
        private List<NestedNode> list;
        public List<NestedNode> List { get { return this.list; } }

        [SerializeField, HideInInspector]
        private ListConfiguration listConfig;
        public ListConfiguration Configuration { get { return this.listConfig; } }

        [SerializeField, HideInInspector]
        public List<board> boards = new List<board>() { new board("Backlog"), new board("To Do"), new board("In Progress"), new board("Completed") };

        [System.Serializable]
        public struct board
        {
            public string title;
            public int wip;

            public board(string t, int w = 0)
            {
                title = t;
                wip = w;
            }
        }

        void OnEnable()
        {
            if (list == null)
            {
                list = new List<NestedNode>();
                idCount = 1;
                NestedNode root = new NestedNode(0);
                list.Add(root);
            }

            if (listConfig == null)
            {
                listConfig = new ListConfiguration();
                RefreshSerializableData();
                listConfig.Reset(SerializedListObject);
            }

            if (listConfig.categories == null || listConfig.categories.Count == 0)
                listConfig.Reset(SerializedListObject, 1);
            if (listConfig.priorities == null || listConfig.priorities.Count == 0)
                listConfig.Reset(SerializedListObject, 2);
            if (listConfig.tags == null || listConfig.tags.Count == 0)
                listConfig.Reset(SerializedListObject, 3);
            if (listConfig.skinColors == null || listConfig.skinColors.Length == 0)
                listConfig.Reset(SerializedListObject, 4);


            Undo.undoRedoPerformed += RefreshSerializableData;
        }

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= RefreshSerializableData;
        }

        public override string ToString()
        {
            string text = base.ToString();
            return text.Replace("(NestedList)", "");
        }

        //functions for getting list data as serialized objects/properties
        #region serialized data
        SerializedObject serializedListObject;
        public SerializedObject SerializedListObject
        {
            get
            {
                if (serializedListObject == null)
                    serializedListObject = new SerializedObject(this);

                return serializedListObject;
            }
        }

        public SerializedProperty SerializedList
        {
            get
            {
                return SerializedListObject.FindProperty("list");
            }
        }

        public void SaveSerializedProperties()
        {
            SerializedListObject.ApplyModifiedProperties();
        }

        public SerializedProperty SerializedNode(int id)
        {
            return SerializedList.GetArrayElementAtIndex(id);
        }

        public SerializedProperty SerializedNode(NestedNode node)
        {
            if (SerializedList.arraySize <= node.NodeID)
                return null;

            return SerializedList.GetArrayElementAtIndex(node.NodeID);
        }

        void RefreshSerializableData()
        {
            serializedListObject = null;
        }
        #endregion

        //functions to get a task from the list
        #region nodes getters
        public NestedNode getNode(int index)
        {
            return list[index];
        }

        public NestedNode getParent(int index)
        {
            return list[getNode(index).ParentID];
        }

        public NestedNode getParent(NestedNode node)
        {
            return list[node.ParentID];
        }

        public NestedNode getChild(int node, int index)
        {
            return list[getNode(node).ChildIds[index]];
        }

        public NestedNode getChild(NestedNode node, int index)
        {
            return list[node.ChildIds[index]];
        }
        #endregion

        //functions to calculate the complete rate of the list
        #region complete rate functions
        public float Progress
        {
            get
            {
                float checkCount = 0;
                float completeCount = 0;

                for (int i = 1; i < list.Count; i++)
                {
                    if (!getNode(i).SetToRewrite)
                    {
                        if (getNode(i).Completed)
                            completeCount++;

                        checkCount++;
                    }
                }

                if (checkCount == 0)
                {
                    completeCount = -1;
                    checkCount = 1;
                }

                return Mathf.Clamp01(completeCount / checkCount);
            }
        }

        public float GetBranchProgress(NestedNode node)
        {
            float checkCount = 0;
            float completeCount = 0;

            getBranchProgress(node, ref checkCount, ref completeCount);

            if (node.ChildIds.Count > 0)
                checkCount--;

            return Mathf.Clamp01(completeCount / checkCount);
        }

        void getBranchProgress(NestedNode node, ref float checkCount, ref float completeCount)
        {
            if (node.SetToRewrite)
                return;

            if (node.ChildIds.Count > 0)
            {
                for (int i = 0; i < node.ChildIds.Count; i++)
                {
                    getBranchProgress(getChild(node, i), ref checkCount, ref completeCount);
                }
            }

            if (node.Completed)
                completeCount++;

            checkCount++;
        }
        #endregion

        //functions for adding tasks to the list
        #region add functions
        public NestedNode AddTopLevelNode()
        {
            return AddNestedNode(null);
        }

        public NestedNode AddSiblingNode(NestedNode node)
        {
            return AddNestedNode(getParent(node));
        }

        public NestedNode AddUncleNode(NestedNode node)
        {
            return AddSiblingNode(getParent(node));
        }

        public NestedNode AddChildNode(NestedNode parent)
        {
            return AddNestedNode(parent);
        }

        public NestedNode AddNestedNode(NestedNode parentTask = null)
        {
            if (parentTask == null)
                parentTask = list[0];

            //see if there is an unused id before incrementing the global id counter
            int id = -1;
            for (int i = 1; i < idCount; i++)
            {
                if (list[i].SetToRewrite)
                {
                    id = i;
                    break;
                }
            }

            if (id < 0)
            {
                id = idCount;
                SerializedListObject.FindProperty("idCount").intValue++;
            }

            Vector3 tmpVector = new Vector3(0, 0, 0);
            Object tmpObject = null;
            if (listConfig.InheritCategory)
                tmpVector.x = parentTask.Category;
            if (listConfig.InheritPriority)
                tmpVector.y = parentTask.Priority;
            if (listConfig.InheritTag)
                tmpVector.z = parentTask.Tag;
            if (listConfig.InheritLinkedFile)
                tmpObject = parentTask.LinkedFile;

            NestedNode node = new NestedNode(id, parentTask, (int)tmpVector.x, (int)tmpVector.y, (int)tmpVector.z, ref tmpObject);

            if (id == list.Count)
            {
                SerializedList.arraySize++;
                var cloneNode = SerializedList.GetArrayElementAtIndex(SerializedList.arraySize - 1);
                CloneNode(ref cloneNode, node);
            }
            else
            {
                var cloneNode = SerializedList.GetArrayElementAtIndex(id);
                CloneNode(ref cloneNode, node);
            }

            var parentChilds = SerializedNode(parentTask.NodeID).FindPropertyRelative("childIds");

            parentChilds.arraySize++;
            parentChilds.GetArrayElementAtIndex(parentChilds.arraySize - 1).intValue = id;
            SerializedNode(parentTask.NodeID).FindPropertyRelative("showChilds").boolValue = true;
            SerializedNode(parentTask.NodeID).FindPropertyRelative("boardPosition").vector2Value = new Vector2(0, -1);

            SaveSerializedProperties();

            if (parentTask.NodeID > 0)
            {
                TellParentToCheckChilds(node.NodeID);
            }
            SaveSerializedProperties();

            return getNode(id);
        }

        void CloneNode(ref SerializedProperty cloneNode, NestedNode original)
        {
            cloneNode.FindPropertyRelative("nodeID").intValue = original.NodeID;
            cloneNode.FindPropertyRelative("parentID").intValue = original.ParentID;
            cloneNode.FindPropertyRelative("completed").boolValue = original.Completed;
            cloneNode.FindPropertyRelative("title").stringValue = original.Title;
            cloneNode.FindPropertyRelative("notes").stringValue = original.Note;
            cloneNode.FindPropertyRelative("depth").intValue = original.Depth;
            cloneNode.FindPropertyRelative("category").intValue = original.Category;
            cloneNode.FindPropertyRelative("priority").intValue = original.Priority;
            cloneNode.FindPropertyRelative("tag").intValue = original.Tag;
            cloneNode.FindPropertyRelative("linkedFile").objectReferenceValue = original.LinkedFile;
            cloneNode.FindPropertyRelative("childIds").ClearArray();
            cloneNode.FindPropertyRelative("showChilds").boolValue = original.ShowChilds;
            cloneNode.FindPropertyRelative("setToRewrite").boolValue = original.SetToRewrite;
            cloneNode.FindPropertyRelative("points").floatValue = original.points;
            cloneNode.FindPropertyRelative("boardPosition").vector2Value = original.boardPosition;
        }
        #endregion

        public void DeleteNode(NestedNode node)
        {
            //get to the every child node
            while (node.ChildIds.Count > 0)
            {
                DeleteNode(getChild(node, 0));
            }

            SerializedProperty n = SerializedNode(node.NodeID);


            n.FindPropertyRelative("setToRewrite").boolValue = true;
            n.FindPropertyRelative("boardPosition").vector2Value = new Vector2(0, -1);
            n.FindPropertyRelative("points").floatValue = 1;

            int index = getParent(node).ChildIds.IndexOf(node.NodeID);
            SerializedNode(node.ParentID).FindPropertyRelative("childIds").DeleteArrayElementAtIndex(index);

            n.FindPropertyRelative("childIds").ClearArray();

            SaveSerializedProperties();
        }

        public void MarkBranch(NestedNode node, bool state)
        {
            for (int i = 0; i < node.ChildIds.Count; i++)
            {
                MarkBranch(getChild(node, i), state);
            }

            SetNodeCompletedState(node.NodeID, state);
        }

        public void TellParentToCheckChilds(int id)
        {
            var parent = getParent(id);

            bool tmpBool = true;
            for (int i = 0; i < parent.ChildIds.Count; i++)
            {
                if (!getChild(parent, i).Completed)
                    tmpBool = false;
            }

            SetNodeCompletedState(parent.NodeID, tmpBool);

            if (parent.NodeID > 0)
                TellParentToCheckChilds(parent.NodeID);
        }

        private void SetNodeCompletedState(int nodeID, bool state)
        {
            bool oldState = SerializedNode(nodeID).FindPropertyRelative("completed").boolValue;
            string oldDate = SerializedNode(nodeID).FindPropertyRelative("completedAt").stringValue;

            //check for change
            if (oldState != state)
            {

                if (state)
                {
                    //only write date if no date found
                    if (string.IsNullOrEmpty(oldDate))
                    {
                        SerializedNode(nodeID).FindPropertyRelative("completedAt").stringValue = System.DateTime.Now.ToShortDateString();
                    }
                }
                else
                {
                    //empty date if task is marked as uncompleted
                    SerializedNode(nodeID).FindPropertyRelative("completedAt").stringValue = string.Empty;
                }
            }

            SerializedNode(nodeID).FindPropertyRelative("completed").boolValue = state;
            SaveSerializedProperties();
        }

        //functions for handling the order of the tasks in the list
        #region order functions

        public void SimpleChildSwap(int childA, int childB)
        {
            NestedNode parent = getParent(childA);

            int posA = parent.ChildIds.IndexOf(childA);
            int posB = parent.ChildIds.IndexOf(childB);

            SerializedNode(parent.NodeID).FindPropertyRelative("childIds").GetArrayElementAtIndex(posA).intValue = childB;
            SerializedNode(parent.NodeID).FindPropertyRelative("childIds").GetArrayElementAtIndex(posB).intValue = childA;

            SaveSerializedProperties();
        }

        public bool CanMoveUp(NestedNode node)
        {
            NestedNode parent = getParent(node);
            if (parent.ChildIds.IndexOf(node.NodeID) == 0)
                return false;

            return true;
        }

        public void MoveNodeUp(NestedNode node)
        {
            NestedNode parent = getParent(node);
            int siblingId = getChild(parent, parent.ChildIds.IndexOf(node.NodeID) - 1).NodeID;

            SimpleChildSwap(node.NodeID, siblingId);
        }

        public bool CanMoveDown(NestedNode node)
        {
            NestedNode parent = getParent(node);
            if (parent.ChildIds.IndexOf(node.NodeID) == parent.ChildIds.Count - 1)
                return false;

            return true;
        }

        public void MoveNodeDown(NestedNode node)
        {
            NestedNode parent = getParent(node);
            int siblingId = getChild(parent, parent.ChildIds.IndexOf(node.NodeID) + 1).NodeID;

            SimpleChildSwap(node.NodeID, siblingId);
        }

        public void TransferNode(int nodeToTransfer, int newParentID)
        {
            if (nodeToTransfer == newParentID)
                return;

            if (!IsParentOfBranch(nodeToTransfer, newParentID))
            {
                //remove transfered node from old parent child array
                var oldParent = SerializedList.GetArrayElementAtIndex(getParent(nodeToTransfer).NodeID);
                int childIndex = 0;
                while (getParent(nodeToTransfer).ChildIds[childIndex] != nodeToTransfer)
                    childIndex++;
                oldParent.FindPropertyRelative("childIds").DeleteArrayElementAtIndex(childIndex);
                SaveSerializedProperties();

                //add the transfered node to the new parent child id array
                var newParent = SerializedList.GetArrayElementAtIndex(newParentID);
                var parentArray = newParent.FindPropertyRelative("childIds");
                parentArray.arraySize++;
                parentArray.GetArrayElementAtIndex(parentArray.arraySize - 1).intValue = nodeToTransfer;
                SaveSerializedProperties();

                //update transfered node parent id
                SerializedList.GetArrayElementAtIndex(nodeToTransfer).FindPropertyRelative("parentID").intValue = newParentID;
                SaveSerializedProperties();

                //fix depth levels
                FixDepthLevels(getNode(nodeToTransfer), getNode(newParentID).Depth + 1);

                //fix boards positions
                SerializedList.GetArrayElementAtIndex(nodeToTransfer).FindPropertyRelative("boardPosition").vector2Value = new Vector2(0, -1);
                SerializedList.GetArrayElementAtIndex(newParentID).FindPropertyRelative("boardPosition").vector2Value = new Vector2(0, -1);
                SaveSerializedProperties();
            }

        }

        //check the parents in the branch and see if the tested node is one of them
        bool IsParentOfBranch(int testedNodeID, int node)
        {
            while (node != 0)
            {
                if (node == testedNodeID)
                {
                    return true;
                }
                else
                {
                    node = getParent(node).NodeID;
                }

            }
            return false;
        }

        void FixDepthLevels(NestedNode node, int depth)
        {
            SerializedList.GetArrayElementAtIndex(node.NodeID).FindPropertyRelative("depth").intValue = depth;
            SaveSerializedProperties();
            for (int i = 0; i < node.ChildIds.Count; i++)
            {
                FixDepthLevels(getChild(node, i), depth + 1);
            }
        }





        #endregion

        //functions for handling a task properties (categories, priorities and tags)
        #region properties functions
        public void AddProperty(string propertyPath, string textValue)
        {
            var property = SerializedListObject.FindProperty("listConfig." + propertyPath);
            property.arraySize++;
            property.GetArrayElementAtIndex(property.arraySize - 1).stringValue = textValue;
            SaveSerializedProperties();
        }

        public void AddProperty(string propertyPath, string textValue, Color colorValue)
        {
            var property = SerializedListObject.FindProperty("listConfig." + propertyPath);
            property.arraySize++;
            var priority = property.GetArrayElementAtIndex(property.arraySize - 1);
            priority.FindPropertyRelative("priorityText").stringValue = textValue;
            priority.FindPropertyRelative("priorityColor").colorValue = colorValue;
            SaveSerializedProperties();
        }

        public void DeleteProperty(string propertyPath, int index)
        {
            var property = SerializedListObject.FindProperty("listConfig." + propertyPath);
            property.DeleteArrayElementAtIndex(index);
            SaveSerializedProperties();
        }

        public void FixProperty(string property, int deletedProperty)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var tmpProperty = SerializedNode(i).FindPropertyRelative(property);

                if (tmpProperty.intValue > deletedProperty)
                    tmpProperty.intValue--;
                else if (tmpProperty.intValue == deletedProperty)
                    tmpProperty.intValue = 0;
            }

            SaveSerializedProperties();
        }

        public void ResetProperty(string property)
        {
            for (int i = 1; i < list.Count; i++)
            {
                SerializedNode(i).FindPropertyRelative(property).intValue = 0;
            }

            SaveSerializedProperties();
        }

        public void SetBranchProperty(NestedNode node, string property, int value)
        {
            for (int i = 0; i < node.ChildIds.Count; i++)
            {
                SetBranchProperty(getChild(node, i), property, value);
            }

            SerializedNode(node.NodeID).FindPropertyRelative(property).intValue = value;

            SaveSerializedProperties();
        }

        public void SwapProperties(string property, int propertyA, int propertyB)
        {
            SerializedProperty tmpProperty = null;

            for (int i = 1; i < list.Count; i++)
            {
                tmpProperty = SerializedNode(i).FindPropertyRelative(property);

                if (tmpProperty.intValue == propertyA)
                    tmpProperty.intValue = propertyB;
                else if (tmpProperty.intValue == propertyB)
                    tmpProperty.intValue = propertyA;
            }

            switch (property)
            {
                case "category": tmpProperty = SerializedListObject.FindProperty("listConfig.categories"); break;
                case "priority": tmpProperty = SerializedListObject.FindProperty("listConfig.priorities"); break;
                case "tag": tmpProperty = SerializedListObject.FindProperty("listConfig.tags"); break;
            }

            if (property.Equals("priority"))
            {
                var tmpString = tmpProperty.GetArrayElementAtIndex(propertyA).FindPropertyRelative("priorityText").stringValue;
                var tmpColor = tmpProperty.GetArrayElementAtIndex(propertyA).FindPropertyRelative("priorityColor").colorValue;

                tmpProperty.GetArrayElementAtIndex(propertyA).FindPropertyRelative("priorityText").stringValue = tmpProperty.GetArrayElementAtIndex(propertyB).FindPropertyRelative("priorityText").stringValue;
                tmpProperty.GetArrayElementAtIndex(propertyA).FindPropertyRelative("priorityColor").colorValue = tmpProperty.GetArrayElementAtIndex(propertyB).FindPropertyRelative("priorityColor").colorValue;

                tmpProperty.GetArrayElementAtIndex(propertyB).FindPropertyRelative("priorityText").stringValue = tmpString;
                tmpProperty.GetArrayElementAtIndex(propertyB).FindPropertyRelative("priorityColor").colorValue = tmpColor;
            }
            else
            {
                string tmp = tmpProperty.GetArrayElementAtIndex(propertyA).stringValue;
                tmpProperty.GetArrayElementAtIndex(propertyA).stringValue = tmpProperty.GetArrayElementAtIndex(propertyB).stringValue;
                tmpProperty.GetArrayElementAtIndex(propertyB).stringValue = tmp;
            }

            SaveSerializedProperties();
        }
        #endregion

        //functions to handle tasks visibility
        #region visibility functions
        public void ExpandCollapseAllBranches(bool state)
        {
            for (int i = 1; i < list.Count; i++)
            {
                SerializedNode(i).FindPropertyRelative("showChilds").boolValue = state;
            }

            SaveSerializedProperties();
        }

        public bool CheckBranchVisibility(NestedNode node)
        {
            while (node.ParentID > 0)
            {
                if (!getParent(node).ShowChilds)
                    return false;

                node = getParent(node);
            }

            return true;
        }

        public bool CheckFilterVisibility(NestedNode node, int viewFilter, ref bool pass, ref bool drawActive)
        {
            int categoryCount = listConfig.categories.Count;
            int priorityCount = listConfig.priorities.Count;
            //check own condition validation;
            switch (viewFilter)
            {
                case 0:
                    pass = true;
                    drawActive = true;
                    break;
                case 1:
                    if (!node.Completed)
                    {
                        pass = true;
                        drawActive = true;
                    }
                    break;
                case 2:
                    if (node.Completed)
                    {
                        pass = true;
                        drawActive = true;
                    }
                    break;
                default:
                    if (viewFilter > 2 && viewFilter < categoryCount + 3)
                    {
                        if (node.Category == viewFilter - 3)
                        {
                            pass = true;
                            drawActive = true;
                        }
                    }
                    else if (viewFilter >= categoryCount + 3 && viewFilter < priorityCount + categoryCount + 3)
                    {
                        if (node.Priority == viewFilter - (categoryCount + 3))
                        {
                            pass = true;
                            drawActive = true;
                        }
                    }
                    else
                    {
                        if (node.Tag == viewFilter - (priorityCount + categoryCount + 3))
                        {
                            pass = true;
                            drawActive = true;
                        }
                    }
                    break;
            }
            if (drawActive)
                return true;

            return checkChildVisibility(node, viewFilter);
        }

        public bool checkChildVisibility(NestedNode node, int condition)
        {
            bool parentPass = false;
            switch (condition)
            {
                case 1:
                    if (!node.Completed)
                        parentPass = true;
                    break;
                case 2:
                    if (node.Completed)
                        parentPass = true;
                    break;
                default:
                    if (node.Category == condition - 3)
                        parentPass = true;
                    break;
            }
            if (parentPass)
                return true;
            else
            {
                bool childTest = false;
                for (int i = 0; i < node.ChildIds.Count; i++)
                {
                    childTest = checkChildVisibility(getChild(node, i), condition);
                    if (childTest)
                        return true;
                }
                return false;
            }
        }
        #endregion

        //functions to handle agile methodology methods
        #region agile functions
        public float SumNodePoints()
        {
            float points = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (getNode(i).ChildIds.Count == 0 && !getNode(i).Completed && !getNode(i).SetToRewrite)
                    points += getNode(i).points;
            }

            return points;
        }
        #endregion

        //used for the view filters in both the checklist and agile board windows
        //not really part of this class, yet its better than having the same code duplicated in multiple places
        public static void UpdateViewFilter(ref List<string> ViewFilter, string[] defaultFilters, NestedList list)
        {
            ViewFilter.Clear();
            for (int i = 0; i < defaultFilters.Length; i++)
                ViewFilter.Add(defaultFilters[i]);

            if (list == null)
                return;

            ViewFilter.AddRange(list.Configuration.categories);
            for (int i = 0; i < list.Configuration.priorities.Count; i++)
            {
                ViewFilter.Add(list.Configuration.priorities[i].priorityText);
            }
            ViewFilter.AddRange(list.Configuration.tags);
        }
    }

    [System.Serializable]
    public class NestedNode
    {
        [SerializeField]
        private string title;
        [SerializeField]
        private string notes;
        [SerializeField]
        private Object linkedFile = null;
        [SerializeField]
        private bool completed = false;

        [SerializeField]
        private bool setToRewrite = false;
        [SerializeField]
        private bool showChilds = true;

        [SerializeField]
        private int category;
        [SerializeField]
        private int priority;
        [SerializeField]
        private int tag;

        [SerializeField]
        private int nodeID;
        [SerializeField]
        private int depth;
        [SerializeField]
        private int parentID;
        [SerializeField]
        private List<int> childIds;

        [SerializeField]
        private string completedAt;

        public int lineIndex;

        public int Category { get { return this.category; } set { this.category = value; } }
        public int Priority { get { return this.priority; } set { this.priority = value; } }
        public int Tag { get { return this.tag; } set { this.tag = value; } }
        public int NodeID { get { return this.nodeID; } set { this.nodeID = value; } }
        public int ParentID { get { return this.parentID; } set { this.parentID = value; } }
        public string Title { get { return this.title; } set { this.title = value; } }
        public string Note { get { return this.notes; } set { this.notes = value; } }
        public UnityEngine.Object LinkedFile { get { return this.linkedFile; } set { this.linkedFile = value; } }
        public int Depth { get { return this.depth; } set { this.depth = value; } }
        public List<int> ChildIds { get { return this.childIds; } set { this.childIds = value; } }
        public bool Completed { get { return this.completed; } set { this.completed = value; } }
        public bool SetToRewrite { get { return this.setToRewrite; } set { this.setToRewrite = value; } }
        public bool ShowChilds { get { return this.showChilds; } set { this.showChilds = value; } }

        public string CompletedAt { get { return completedAt; } set { completedAt = value; } }

        public float points = 1;
        public Vector2 boardPosition = new Vector2(0, -1);

        public NestedNode(int id)
        {
            this.nodeID = id;
            this.parentID = 0;
            this.title = "Root Node";
            this.notes = "";
            this.depth = 0;
            this.category = 0;
            this.priority = 0;
            this.tag = 0;
            this.childIds = new List<int>();
            this.completed = false;
            this.showChilds = true;
            this.setToRewrite = false;
        }

        public NestedNode(int id, NestedNode parentNode, int c, int p, int t, ref Object file)
        {
            this.nodeID = id;
            this.parentID = parentNode.NodeID;
            this.title = "New Task";
            this.notes = "";
            this.depth = parentNode.Depth + 1;
            this.linkedFile = file;
            this.category = c;
            this.priority = p;
            this.tag = t;
            this.childIds = new List<int>();
            this.boardPosition = parentNode.boardPosition;
            this.completed = false;
            this.showChilds = true;
            this.setToRewrite = false;
        }

    }

}