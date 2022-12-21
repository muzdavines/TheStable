using FIMSpace.Generating.Rules;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace FIMSpace.Generating
{

    [System.Serializable]
    public class CheckCellsSelectorSetup
    {
        public CheckCellsSelectorSetup(bool useRotor = false, bool useCondition = false)
        {
            UseRotor = useRotor;
            UseCondition = useCondition;
        }

        public bool UseRotor = false;
        public enum ERotor
        {
            [Tooltip("Example: If upper cells are selected then Forward cells will be checked, if right cells then right, the same way like rectangles below\n(if you checking this, make sure you look at scene with Z forward and X right)")]
            NoRotor,
            [Tooltip("Example: If upper cells are selected and spawned object is rotated by 90 degrees clockwise then right cells will be checked")]
            RotorWithSpawnRotation
        }
        public ERotor Rotor = ERotor.NoRotor;

        public bool UseCondition = false;
        public ESR_NeightbourCondition Condition = ESR_NeightbourCondition.AllNeeded;

        [HideInInspector] public List<Vector3Int> ToCheck = new List<Vector3Int>() { new Vector3Int(0, 0, 0) };
    }


    [System.Serializable]
    public class NeightbourPlacement
    {
        public enum ENeightbour : int
        {
            LeftUp, Up, RightUp,
            Left, Middle, Right,
            LeftDown, Down, RightDown
        }

        #region Vars

        public bool lu = false;
        public bool u = false;
        public bool ru = false;

        public bool l = false;
        public bool m = false;
        public bool r = false;

        public bool ld = false;
        public bool d = false;
        public bool rd = false;

        #endregion


        #region Methods

        public bool Get(int x, int y)
        {
            if (x == 0 && y == 0) return m;

            if (x == -1)
            {
                if (y == 1) return lu;
                if (y == 0) return l;
                if (y == -1) return ld;
            }

            if (x == 0)
            {
                if (y == 1) return u;
                if (y == 0) return m;
                if (y == -1) return d;
            }

            if (x == 1)
            {
                if (y == 1) return ru;
                if (y == 0) return r;
                if (y == -1) return rd;
            }

            return m;
        }

        public void Set(int x, int y, bool value)
        {
            if (x == 0 && y == 0)
            {
                m = value;
                return;
            }

            if (x == -1)
            {
                if (y == 1) lu = value;
                if (y == 0) l = value;
                if (y == -1) ld = value;
            }

            if (x == 0)
            {
                if (y == 1) u = value;
                if (y == 0) m = value;
                if (y == -1) d = value;
            }

            if (x == 1)
            {
                if (y == 1) ru = value;
                if (y == 0) r = value;
                if (y == -1) rd = value;
            }
        }

        public void Setall(bool value, bool ignoreMiddle = false)
        {
            lu = value;
            u = value;
            ru = value;

            l = value;
            if (!ignoreMiddle) m = value;
            r = value;

            ld = value;
            d = value;
            rd = value;
        }

        #endregion


        #region Editor

#if UNITY_EDITOR
        private static string[] guiSet = new string[] { "╔", "╦", "╗", "╠", "╬", "╣", "╚", "╩", "╝" };

        private static void ButtonPressed(NeightbourPlacement placement, bool allowOnlyOne = false, bool ignoreMiddle = true)
        {
            if (allowOnlyOne) placement.Setall(false, ignoreMiddle);

        }

        public static void DrawGUI(NeightbourPlacement placement, string[] customSet = null, int width = 22, int height = 22, bool allowOnlyOne = false, bool ignoreMiddle = true)
        {
#if UNITY_EDITOR

            if (placement.UseAdvancedSetup)
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Selector")) { CheckCellsSelectorWindow.Init(placement.AdvancedSetup); placement.Advanced_OnSelectorSwitch(); }
                if (GUILayout.Button("Simple Mode")) placement.UseAdvancedSetup = false;
                EditorGUILayout.EndVertical();
            }
            else
            {

                if (customSet == null) customSet = guiSet;
                string[] s = customSet;

                Color c = GUI.color;

                EditorGUILayout.BeginVertical();
                GUILayoutOption[] opt = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };

                EditorGUILayout.BeginHorizontal();
                Color unused = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                if (!placement.Get(-1, 1)) GUI.color = unused; if (GUILayout.Button(s[0], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(-1, 1, !placement.Get(-1, 1)); }
                GUI.color = c;
                if (!placement.Get(0, 1)) GUI.color = unused; if (GUILayout.Button(s[1], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(0, 1, !placement.Get(0, 1)); }
                GUI.color = c;
                if (!placement.Get(1, 1)) GUI.color = unused; if (GUILayout.Button(s[2], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(1, 1, !placement.Get(1, 1)); }
                GUI.color = c;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (!placement.Get(-1, 0)) GUI.color = unused; if (GUILayout.Button(s[3], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(-1, 0, !placement.Get(-1, 0)); }
                GUI.color = c;


                if (!placement.Get(0, 0)) GUI.color = new Color(0.1f, 0.7f, 0.1f, 0.3f); if (GUILayout.Button(s[4], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(0, 0, !placement.Get(0, 0)); }
                GUI.color = c;
                if (!placement.Get(1, 0)) GUI.color = unused; if (GUILayout.Button(s[5], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(1, 0, !placement.Get(1, 0)); }
                GUI.color = c;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (!placement.Get(-1, -1)) GUI.color = unused; if (GUILayout.Button(s[6], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(-1, -1, !placement.Get(-1, -1)); }
                GUI.color = c;
                if (!placement.Get(0, -1)) GUI.color = unused; if (GUILayout.Button(s[7], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(0, -1, !placement.Get(0, -1)); }
                GUI.color = c;
                if (!placement.Get(1, -1)) GUI.color = unused; if (GUILayout.Button(s[8], opt)) { ButtonPressed(placement, allowOnlyOne, ignoreMiddle); placement.Set(1, -1, !placement.Get(1, -1)); }
                GUI.color = c;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
#endif
        }


#endif

        #endregion


        #region Utilities

        public static List<T> Get3x3<T>(T[] a3x3, NeightbourPlacement p) where T : class
        {
            List<T> result = new List<T>();

            if (p.lu) result.Add(a3x3[0]);
            if (p.u) result.Add(a3x3[1]);
            if (p.ru) result.Add(a3x3[2]);

            if (p.l) result.Add(a3x3[3]);
            if (p.m) result.Add(a3x3[4]);
            if (p.r) result.Add(a3x3[5]);

            if (p.ld) result.Add(a3x3[6]);
            if (p.d) result.Add(a3x3[7]);
            if (p.rd) result.Add(a3x3[8]);

            return result;
        }

        /// <param name="quart">1, 2, 3 or 4</param>
        public static List<T> Get3x3Rotate90Quarts<T>(T[] a3x3, NeightbourPlacement p, int quart) where T : class
        {
            List<T> result = new List<T>();

            int[] off = new int[9];

            if (quart <= 1)
            {
                for (int i = 0; i < off.Length; i++) off[i] = i;
            }
            else
            if (quart == 2)
            {
                off[0] = 6; off[1] = 3; off[2] = 0;
                off[3] = 7; off[4] = 4; off[5] = 1;
                off[6] = 8; off[7] = 5; off[8] = 2;
            }
            else
            if (quart == 3)
            {
                off[0] = 8; off[1] = 7; off[2] = 6;
                off[3] = 5; off[4] = 4; off[5] = 3;
                off[6] = 2; off[7] = 1; off[8] = 0;
            }
            else
            {
                off[0] = 2; off[1] = 5; off[2] = 8;
                off[3] = 1; off[4] = 4; off[5] = 7;
                off[6] = 0; off[7] = 3; off[8] = 6;
            }


            if (p.lu) result.Add(a3x3[off[0]]);
            if (p.u) result.Add(a3x3[off[1]]);
            if (p.ru) result.Add(a3x3[off[2]]);

            if (p.l) result.Add(a3x3[off[3]]);

            if (p.m) result.Add(a3x3[4]);

            if (p.r) result.Add(a3x3[off[5]]);

            if (p.ld) result.Add(a3x3[off[6]]);
            if (p.d) result.Add(a3x3[off[7]]);
            if (p.rd) result.Add(a3x3[off[8]]);

            return result;
        }

        internal int SelectedCount()
        {
            int c = 0;

            if (lu) c++;
            if (u) c++;
            if (ru) c++;

            if (l) c++;
            if (m) c++;
            if (r) c++;

            if (ld) c++;
            if (d) c++;
            if (rd) c++;

            return c;
        }

        public bool IsSideEdge()
        {
            if (l == false || r == false || u == false || d == false) return true; else return false;
        }

        public bool IsDiagonalEdge()
        {
            if (!lu || !ru || !ld || !rd) return true; else return false;
        }

        public bool IsSelected(ENeightbour n)
        {
            switch (n)
            {
                case ENeightbour.LeftUp: return lu;
                case ENeightbour.Up: return u;
                case ENeightbour.RightUp: return ru;
                case ENeightbour.Left: return l;
                case ENeightbour.Middle: return m;
                case ENeightbour.Right: return r;
                case ENeightbour.LeftDown: return ld;
                case ENeightbour.Down: return d;
                case ENeightbour.RightDown: return rd;
            }

            return false;
        }

        internal ENeightbour GetFirstSelectedNeightbourID(float yawOffset)
        {
            yawOffset = yawOffset % 360f;
            int sel = Get360NeightbourID(GetFirstSelectedNeightbourID());
            int off = 0;

            if (yawOffset > -44f && yawOffset < 44f)
            {
                // nothing
            }
            else if (yawOffset > -89f && yawOffset < 89f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset));
            }
            else if (yawOffset > -134f && yawOffset < 134f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset)) * 2;
            }
            else if (yawOffset > -179f && yawOffset < 179f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset)) * 3;
            }
            else if (yawOffset > -224f && yawOffset < 224f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset)) * 4;
            }
            else if (yawOffset > -214f && yawOffset < 214f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset)) * 5;
            }
            else if (yawOffset > -269f && yawOffset < 269f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset)) * 6;
            }
            else if (yawOffset > -314f && yawOffset < 314f)
            {
                off += Mathf.RoundToInt(Mathf.Sign(yawOffset)) * 7;
            }

            sel += off;
            if (sel < 0) sel += 8;
            if (sel > 7) sel -= 8;

            return Get360NeightbourID(sel);
        }

        internal ENeightbour Get360NeightbourID(int rotCC)
        {
            if (rotCC == 0) return ENeightbour.Right;
            if (rotCC == 1) return ENeightbour.RightUp;
            if (rotCC == 2) return ENeightbour.Up;
            if (rotCC == 3) return ENeightbour.LeftUp;
            if (rotCC == 4) return ENeightbour.Left;
            if (rotCC == 5) return ENeightbour.LeftDown;
            if (rotCC == 6) return ENeightbour.Down;
            if (rotCC == 7) return ENeightbour.RightDown;
            return ENeightbour.Right;
        }

        internal int Get360NeightbourID(ENeightbour rotCC)
        {
            if (rotCC == ENeightbour.Right) return 0;
            if (rotCC == ENeightbour.RightUp) return 1;
            if (rotCC == ENeightbour.Up) return 2;
            if (rotCC == ENeightbour.LeftUp) return 3;
            if (rotCC == ENeightbour.Left) return 4;
            if (rotCC == ENeightbour.LeftDown) return 5;
            if (rotCC == ENeightbour.Down) return 6;
            if (rotCC == ENeightbour.RightDown) return 7;
            return 0;
        }

        internal ENeightbour GetFirstSelectedNeightbourID()
        {
            if (lu) return ENeightbour.LeftUp;
            if (u) return ENeightbour.Up;
            if (ru) return ENeightbour.RightUp;
            if (l) return ENeightbour.Left;
            if (r) return ENeightbour.Right;
            if (ld) return ENeightbour.LeftDown;
            if (d) return ENeightbour.Down;
            if (rd) return ENeightbour.RightDown;
            return ENeightbour.Middle;
        }

        internal void Set(ENeightbour n, bool v)
        {
            switch (n)
            {
                case ENeightbour.LeftUp: lu = v; break;
                case ENeightbour.Up: u = v; break;
                case ENeightbour.RightUp: ru = v; break;
                case ENeightbour.Left: l = v; break;
                case ENeightbour.Middle: m = v; break;
                case ENeightbour.Right: r = v; break;
                case ENeightbour.LeftDown: ld = v; break;
                case ENeightbour.Down: d = v; break;
                case ENeightbour.RightDown: rd = v; break;
            }
        }

        public static Vector3 GetDirection(ENeightbour n)
        {
            switch (n)
            {
                case ENeightbour.LeftUp: return new Vector3(-1, 0, 1);
                case ENeightbour.Up: return new Vector3(0, 0, 1);
                case ENeightbour.RightUp: return new Vector3(1, 0, 1);
                case ENeightbour.Left: return new Vector3(-1, 0, 0);
                case ENeightbour.Right: return new Vector3(1, 0, 0);
                case ENeightbour.LeftDown: return new Vector3(-1, 0, -1);
                case ENeightbour.Down: return new Vector3(0, 0, -1);
                case ENeightbour.RightDown: return new Vector3(1, 0, -1);
            }

            return Vector3.zero;
        }

        #endregion


        #region Advanced Setup

        [HideInInspector] public bool UseAdvancedSetup = false;
        [HideInInspector] public List<Vector3Int> AdvancedSetup;

        public void Advanced_OnSelectorSwitch()
        {
            if (UseAdvancedSetup == false)
            {
                if (AdvancedSetup == null) AdvancedSetup = new System.Collections.Generic.List<Vector3Int>();
                AdvancedSetup.Clear();
                if (l) AdvancedSetup.Add(new Vector3Int(-1, 0, 0));
                if (r) AdvancedSetup.Add(new Vector3Int(1, 0, 0));
                if (u) AdvancedSetup.Add(new Vector3Int(0, 0, 1));
                if (d) AdvancedSetup.Add(new Vector3Int(0, 0, -1));
                if (lu) AdvancedSetup.Add(new Vector3Int(-1, 0, 1));
                if (ru) AdvancedSetup.Add(new Vector3Int(1, 0, 1));
                if (ld) AdvancedSetup.Add(new Vector3Int(-1, 0, -1));
                if (rd) AdvancedSetup.Add(new Vector3Int(1, 0, -1));
                if (lu) AdvancedSetup.Add(new Vector3Int(-1, 0, 1));
                if (m) AdvancedSetup.Add(new Vector3Int(0, 0, 0));
            }

            UseAdvancedSetup = true;
        }

        public void Advanced_SwitchOnPosition(Vector3Int pos)
        {
            AdvancedSetup.CellsSelector_SwitchOnPosition(pos);
        }

        public Vector3Int Advanced_Rotate(Vector3Int pos, int rotor = 0)
        {
            return pos.CellsSelector_Rotate(rotor);
        }

        public Vector3Int Advanced_Rotate(Vector3Int pos, Quaternion rotation, int rotor = 0)
        {
            pos = (rotation * pos).V3toV3Int();
            return pos.CellsSelector_Rotate(rotor);
        }

        #endregion
    }

}
