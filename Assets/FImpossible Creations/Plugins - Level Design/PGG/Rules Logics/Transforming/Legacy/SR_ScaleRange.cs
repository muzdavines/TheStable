#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Transforming.Legacy
{
    public class SR_ScaleRange : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Random Scale Ranged"; }
        public override string Tooltip() { return "Setting random scale of target spawned prefabs"; }

        public EProcedureType Type { get { return EProcedureType.Event; } }

        [PGG_SingleLineTwoProperties("MultiplyScale", 0, 90, 10, -40)]
        public bool KeepScaleRatio = true;
        [HideInInspector] public float MultiplyScale = 1f;
        [HideInInspector] public Vector2 ScaleXFromTo = new Vector2(0.9f, 1f);
        [HideInInspector] public Vector2 ScaleYFromTo = new Vector2(0.9f, 1f);
        [HideInInspector] public Vector2 ScaleZFromTo = new Vector2(0.9f, 1f);


        #region There you can do custom modifications for inspector view
#if UNITY_EDITOR

        public override void NodeFooter(SerializedObject so, FieldModification mod)
        {

            if (KeepScaleRatio)
            {
                EditorGUILayout.MinMaxSlider("Random Scale", ref ScaleXFromTo.x, ref ScaleXFromTo.y, 0f, 1f);

                GUILayout.Space(-2);
                EditorGUILayout.LabelField("XYZ: " + System.Math.Round(ScaleXFromTo.x * MultiplyScale, 2) + " to " + System.Math.Round(ScaleXFromTo.y * MultiplyScale, 2), EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(1);
            }
            else
            {
                EditorGUILayout.MinMaxSlider("Random Scale X", ref ScaleXFromTo.x, ref ScaleXFromTo.y, 0f, 1f);
                EditorGUILayout.MinMaxSlider("Random Scale Y", ref ScaleYFromTo.x, ref ScaleYFromTo.y, 0f, 1f);
                EditorGUILayout.MinMaxSlider("Random Scale Z", ref ScaleZFromTo.x, ref ScaleZFromTo.y, 0f, 1f);
                GUIIgnore.Clear();

                GUILayout.Space(-2);
                string fullString = "X: " + System.Math.Round(ScaleXFromTo.x * MultiplyScale, 2) + " to " + System.Math.Round(ScaleXFromTo.y * MultiplyScale, 2);
                fullString += "   " + "Y: " + System.Math.Round(ScaleYFromTo.x * MultiplyScale, 2) + " to " + System.Math.Round(ScaleYFromTo.y * MultiplyScale, 2);
                fullString += "   " + "Z: " + System.Math.Round(ScaleZFromTo.x * MultiplyScale, 2) + " to " + System.Math.Round(ScaleZFromTo.y * MultiplyScale, 2);
                EditorGUILayout.LabelField(fullString, EditorStyles.centeredGreyMiniLabel);
                GUILayout.Space(1);
            }


            base.NodeFooter(so, mod);
        }

#endif
        #endregion


        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {

            if (KeepScaleRatio) // All axis same scale
            {
                float scale = FGenerators.GetRandom(ScaleXFromTo.x * MultiplyScale, ScaleXFromTo.y * MultiplyScale);
                spawn.LocalScaleMul = new Vector3(scale, scale, scale);
            }
            else
            {
                Vector3 newScale = new Vector3();
                newScale.x = FGenerators.GetRandom(ScaleXFromTo.x * MultiplyScale, ScaleXFromTo.y * MultiplyScale);
                newScale.y = FGenerators.GetRandom(ScaleYFromTo.x * MultiplyScale, ScaleYFromTo.y * MultiplyScale);
                newScale.z = FGenerators.GetRandom(ScaleZFromTo.x * MultiplyScale, ScaleZFromTo.y * MultiplyScale);
                spawn.LocalScaleMul = newScale;
            }

        }
    }
}