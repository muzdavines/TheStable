#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.FEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Rules;
using FIMSpace.Graph;

namespace FIMSpace.Generating
{
    public static class CheckCellsSelectorUtils
    {

        public static void CellsSelector_SwitchOnPosition(this List<Vector3Int> offsets, Vector3Int pos)
        {
            if (offsets == null) offsets = new List<Vector3Int>();
            if (!offsets.Contains(pos)) offsets.Add(pos); else offsets.Remove(pos);
        }

        public static Vector3Int CellsSelector_Rotate(this Vector3Int pos, int rotor = 0)
        {
            Vector3Int target = pos;

            if (rotor == 1) target = new Vector3Int(pos.z, pos.y, -pos.x);
            else if (rotor == 2) target = new Vector3Int(-pos.x, pos.y, -pos.z);
            else if (rotor == 3) target = new Vector3Int(-pos.z, pos.y, pos.x);

            return target;
        }
    }


#if UNITY_EDITOR
    public class CheckCellsSelectorWindow : EditorWindow
    {
        public static CheckCellsSelectorWindow Get;
        public static bool OnChange = false;
        public static bool GetChanged() { if (!OnChange) return false; OnChange = false; return true; }

        private Vector2 scroller = Vector2.zero;
        static List<Vector3Int> lastEdited;
        static CheckCellsSelectorSetup lastSetup = null;
        static FieldSpawner spawnerOwner;
        static GUIStyle boxStyle = null;
        static GUIStyle boxStyleSel = null;
        static int depthLevel = 0;

        enum ERotor { ZeroDegrees_BaseSetup, Rot_90_CounterClockwise, Rot_180_CCW, Rot_270_CCW }
        static ERotor rotorPreview = ERotor.ZeroDegrees_BaseSetup;

        enum ESpaceView { XZ_TopDown, XY_Side_2D, ZY_Front }
        static ESpaceView spaceView = ESpaceView.XZ_TopDown;

        static int drawSize = 30;

        public static void Init(List<Vector3Int> placements, FieldSpawner spawner = null, CheckCellsSelectorSetup setup = null)
        {
            if (FGenerators.CheckIfIsNull(placements) && FGenerators.CheckIfIsNull(setup))
            {
                UnityEngine.Debug.Log("Null Placement Reference! ");
                return;
            }

            editedPortValue = null;
            lastEdited = placements;
            lastSetup = setup;
            if (FGenerators.CheckIfExist_NOTNULL(setup)) lastEdited = setup.ToCheck;

            spawnerOwner = spawner;
            depthLevel = 0;

            CheckCellsSelectorWindow window = (CheckCellsSelectorWindow)GetWindow(typeof(CheckCellsSelectorWindow));
            window.titleContent = new GUIContent(" Cells Selector", PGGUtils.Tex_Selector);
            window.Show();
            window.minSize = new Vector2(300, 320);
            Get = window;
            SingleMode = false;
        }

        static PGGVector3Port editedPortValue = null;
        public static void Init(PGGVector3Port port)
        {
            if (FGenerators.CheckIfIsNull(port))
            {
                UnityEngine.Debug.Log("Null Port Reference! ");
                return;
            }

            editedPortValue = port;
            depthLevel = 0;
            lastEdited = null;
            lastSetup = null;

            CheckCellsSelectorWindow window = (CheckCellsSelectorWindow)GetWindow(typeof(CheckCellsSelectorWindow));
            window.titleContent = new GUIContent(" Cells Selector", PGGUtils.Tex_Selector);
            window.Show();
            window.minSize = new Vector2(300, 320);
            Get = window;
            SingleMode = true;
        }

        static bool SingleMode = false;
        public static void Init(List<Vector3Int> selections, bool singleMode)
        {
            Init(selections, null, null);
            if (singleMode) if (selections.Count > 1) selections.RemoveRange(1, selections.Count - 1);
            SingleMode = singleMode;
        }

        private void OnEnable()
        {
            Get = this;
        }

        private void OnGUI()
        {
            Color bc = GUI.backgroundColor;

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(FGUI_Resources.BGInBoxStyle);
                boxStyle.alignment = TextAnchor.MiddleCenter;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.fontSize = Mathf.RoundToInt(boxStyle.fontSize * 1.2f);

                boxStyleSel = new GUIStyle(boxStyle);
                boxStyleSel.normal.background = FGUI_Resources.HeaderBoxStyleH.normal.background;
            }

            GUILayout.Space(5);
            string spName = "";
            if (spawnerOwner != null) spName = " (" + spawnerOwner.Name + ")";
            EditorGUILayout.LabelField("Choose Cells to be Checked" + spName, FGUI_Resources.HeaderStyle);
            FGUI_Inspector.DrawUILine(0.25f, 0.6f, 2, 6);

            drawSize = EditorGUILayout.IntSlider("Cells Draw Size", drawSize, 12, 40);

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 24;

            string yLev = "YLevel";
            if (spaceView == ESpaceView.XY_Side_2D) yLev = "ZLevel";
            else if (spaceView == ESpaceView.ZY_Front) yLev = "XLevel";

            depthLevel = EditorGUILayout.IntField(new GUIContent(yLev, "If you want to check cells placement above or below main cell"), depthLevel);
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
            if (GUILayout.Button("▲", FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18))) depthLevel++;
            if (GUILayout.Button("▼", FGUI_Resources.ButtonStyle, GUILayout.Width(22), GUILayout.Height(18))) depthLevel--;

            GUILayout.Space(6);

            EditorGUIUtility.labelWidth = 35;
            spaceView = (ESpaceView)EditorGUILayout.EnumPopup(new GUIContent("View", "Perspective to view cells, switch to XY for 2D view"), spaceView);
            EditorGUIUtility.labelWidth = 0;

            bool drawRotor = true;
            bool drawCondition = false;
            bool drawRotorCondition = false;
            if (FGenerators.CheckIfExist_NOTNULL(lastSetup))
            {
                if (lastSetup.UseRotor == false) drawRotor = false;
                else
                {
                    if (lastSetup.UseRotor)
                    {
                        drawRotorCondition = true;
                        if (lastSetup.Rotor == CheckCellsSelectorSetup.ERotor.NoRotor) drawRotor = false;
                    }
                }

                if (lastSetup.UseCondition) drawCondition = true;
            }

            if (editedPortValue != null)
            {
                drawRotor = false;
                drawCondition = false;
                drawRotorCondition = false;
            }


            if (drawCondition)
            {
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
                lastSetup.Condition = (ESR_NeightbourCondition)EditorGUILayout.EnumPopup("", lastSetup.Condition, GUILayout.Width(98));
            }

            if (drawRotorCondition)
            {
                GUI.backgroundColor = new Color(0.6f, 1f, 0.6f, 1f);
                lastSetup.Rotor = (CheckCellsSelectorSetup.ERotor)EditorGUILayout.EnumPopup("", lastSetup.Rotor, GUILayout.Width(78));
            }

            GUI.backgroundColor = bc;


            if (drawRotor)
            {
                if (rotorPreview != ERotor.ZeroDegrees_BaseSetup)
                {
                    EditorGUILayout.HelpBox("With rotor preview you can't modify cells!", MessageType.None);
                }

                GUILayout.Space(6);
                EditorGUIUtility.labelWidth = 86;
                rotorPreview = (ERotor)EditorGUILayout.EnumPopup("Rotor Preview", rotorPreview);
            }

            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILine(0.25f, 0.4f, 2, 6);

            if (lastEdited == null)
            {
                GUILayout.Space(5);

                if (SingleMode)
                {
                    DrawCellSelectorGrid(true);
                }
                else
                {
                    EditorGUILayout.HelpBox("No neightbour checker choosed!\nClick again on button inside inspector window of Node", MessageType.Warning);
                }
            }
            else
            {
                DrawCellSelectorGrid(SingleMode);
            }

        }

        void DrawCellSelectorGrid(bool singleMode = false)
        {
            scroller = EditorGUILayout.BeginScrollView(scroller);

            Rect refRect = new Rect(0, 0, drawSize, drawSize);
            float offset = refRect.width / 4f;

            int gridDrawW = Mathf.RoundToInt(position.width / drawSize);
            int gridDrawH = Mathf.RoundToInt(position.height / drawSize);

            int centerCellOnGridX = gridDrawW / 2;
            if (gridDrawW % 2 == 0) centerCellOnGridX -= 1;

            int centerCellOnGridY = gridDrawH / 2 - 1;
            if (gridDrawH % 2 == 0) centerCellOnGridY -= 1;
            EditorGUI.BeginChangeCheck();

            for (int x = 0; x < gridDrawW - 1; x++)
            {
                for (int y = 0; y < gridDrawH - 3; y++)
                {
                    Rect rDraw = new Rect(refRect); rDraw.x = x * drawSize + offset; rDraw.y = y * drawSize + offset;

                    int posX = x - centerCellOnGridX;
                    int posZ = centerCellOnGridY - y;

                    Vector3Int pos = new Vector3Int(posX, depthLevel, posZ);
                    if (spaceView == ESpaceView.XY_Side_2D)
                        pos = new Vector3Int(posX, posZ, depthLevel);
                    else if (spaceView == ESpaceView.ZY_Front)
                        pos = new Vector3Int(depthLevel, posZ, posX);

                    if (rotorPreview != ERotor.ZeroDegrees_BaseSetup) pos = pos.CellsSelector_Rotate((int)rotorPreview);

                    bool contained = false;

                    if (singleMode)
                    {
                        if (editedPortValue != null)
                        { if (pos == editedPortValue.Value.V3toV3Int()) contained = true; }
                        else
                        {
                            if (lastEdited.Count == 0) lastEdited.Add(new Vector3Int());
                            if (lastEdited.Contains(pos)) contained = true;
                        }
                    }
                    else
                    if (lastEdited.Contains(pos)) contained = true;

                    GUIStyle styl = contained ? boxStyleSel : boxStyle;

                    if (centerCellOnGridX == x && centerCellOnGridY == y && depthLevel == 0)
                    {
                        Color preC = GUI.color;
                        Color prebC = GUI.backgroundColor;

                        if (contained)
                            GUI.backgroundColor = new Color(0.7f, 0.7f, 1f, 1f);
                        else
                            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);

                        if (GUI.Button(rDraw, new GUIContent("C", "Center cell - means cell from which checking starts!\nPosition Offset = " + pos), boxStyleSel))
                        {
                            if (rotorPreview == ERotor.ZeroDegrees_BaseSetup)
                            {
                                if (singleMode)
                                {
                                    if (editedPortValue != null)
                                    {
                                        if (!contained) editedPortValue.Value = pos; else editedPortValue.Value = Vector3.zero;
                                    }
                                    else
                                    {
                                        if (lastEdited.Count == 0) lastEdited.Add(new Vector3Int());
                                        if (!contained) lastEdited[0] = pos; else lastEdited[0] = new Vector3Int();
                                    }
                                }
                                else
                                    lastEdited.CellsSelector_SwitchOnPosition(pos);
                            }
                        }

                        GUI.color = preC;
                        GUI.backgroundColor = prebC;
                    }
                    else
                    {
                        if (contained)
                            GUI.backgroundColor = new Color(0.7f, 0.7f, 1f, 1f);
                        else
                            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.4f);

                        if (GUI.Button(rDraw, new GUIContent(" ", "Position Offset = " + pos), styl))
                        {
                            if (rotorPreview == ERotor.ZeroDegrees_BaseSetup)
                            {
                                if (singleMode)
                                {
                                    if (editedPortValue != null)
                                    {
                                        if (!contained) editedPortValue.Value = pos; else editedPortValue.Value = Vector3.zero;
                                    }
                                    else
                                    {
                                        if (lastEdited.Count == 0) lastEdited.Add(new Vector3Int());
                                        if (!contained) lastEdited[0] = pos; else lastEdited[0] = new Vector3Int();
                                    }
                                }
                                else
                                    lastEdited.CellsSelector_SwitchOnPosition(pos);
                            }
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck()) { OnChange = true; }

            EditorGUILayout.EndScrollView();
        }

    }
#endif

}
