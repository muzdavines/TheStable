#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
#endif


namespace FIMSpace.Generating
{
    [System.Serializable]
    public class QuarterRotationCheck
    {
        /// <summary> 0° to 90° </summary>
        public bool q1 = true;
        /// <summary> 90° to 180° </summary>
        public bool q2 = false;
        /// <summary> 180° to 270° </summary>
        public bool q3 = false;
        /// <summary> 270° to 360° </summary>
        public bool q4 = false;


        #region Methods

        /// <param name="quarter">0,90,180 or 270</param>
        public bool ISDegrees(int degree)
        {
            if (degree == 0 || degree == 360) return q1;
            if (degree == 90 || degree == -270) return q2;
            if (degree == 180 || degree == -180) return q3;
            if (degree == 270 || degree == -90) return q4;

            return false;
        }

        /// <param name="quarter">1,2,3 or 4</param>
        public bool ISQuarter(int quarter = 1)
        {
            if (quarter == 1) return q1;
            if (quarter == 2) return q2;
            if (quarter == 3) return q3;
            if (quarter == 4) return q4;

            return false;
        }

        public void SetOnDegrees(int degree, bool value)
        {
            if (degree == 0 || degree == 360) q1 = value;
            if (degree == 90 || degree == -270) q2 = value;
            if (degree == 180 || degree == -180) q3 = value;
            if (degree == 270 || degree == -90) q4 = value;
        }

        public void SetOnQuarter(int quarter, bool value)
        {
            if (quarter == 1) q1 = value;
            if (quarter == 2) q2 = value;
            if (quarter == 3) q3 = value;
            if (quarter == 4) q4 = value;
        }

        internal bool AllChecked()
        {
            return q1 && q2 && q3 && q4;
        }

        internal bool OnlyOneChecked()
        {
            return CountChecked() == 1;
        }

        internal bool AnyChecked()
        {
            return CountChecked() > 0;
        }

        internal int CountChecked()
        {
            int checkedq = 0;
            if (q1) checkedq++;
            if (q2) checkedq++;
            if (q3) checkedq++;
            if (q4) checkedq++;
            return checkedq;
        }

        internal bool Only180Checked()
        {
            if ( q1 && q3)
            {
                if (q2 == false && q4 == false) return true;
            }

            if (q2 && q4)
            {
                if (q1 == false && q3 == false) return true;
            }

            return false;
        }

        #endregion


        #region Editor

#if UNITY_EDITOR
        private static string[] guiSet = new string[] { "0°", "╔", "╚", "╝" };
        //private static string[] guiSet = new string[] { "╗", "╔", "╚", "╝" };

        public static void DrawGUI(QuarterRotationCheck quart, string[] customSet = null, int width = 25, int height = 25)
        {
            if (customSet == null) customSet = guiSet;
            string[] s = customSet;

            Color c = GUI.color;

            EditorGUILayout.BeginVertical();
            GUILayoutOption[] opt = new GUILayoutOption[] { GUILayout.Width(width), GUILayout.Height(height) };

            EditorGUILayout.BeginHorizontal();
            Color unused = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            if (!quart.ISQuarter(2)) GUI.color = unused; if (GUILayout.Button(s[1], opt)) quart.SetOnQuarter(2, !quart.ISQuarter(2)); GUI.color = c;
            if (!quart.ISQuarter(1)) GUI.color = unused; if (GUILayout.Button(s[0], opt)) quart.SetOnQuarter(1, !quart.ISQuarter(1)); GUI.color = c;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (!quart.ISQuarter(3)) GUI.color = unused; if (GUILayout.Button(s[2], opt)) quart.SetOnQuarter(3, !quart.ISQuarter(3)); GUI.color = c;
            if (!quart.ISQuarter(4)) GUI.color = unused; if (GUILayout.Button(s[3], opt)) quart.SetOnQuarter(4, !quart.ISQuarter(4)); GUI.color = c;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

#endif

        #endregion
    }

}
