#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating.Rules.Modelling
{
    public class SR_CombineMesh : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Shedule Mesh Combine"; }
        public override string Tooltip() { return "Sheduling meshes of the generated object to be combined into single mesh during generation. (single mesh = less draw calls = better performance   BUT you don't want to use it on the dynamic objects!)"; }
        public EProcedureType Type { get { return EProcedureType.Event; } }

        public enum ECombineSet { SetCombined, ForceNotCombine }
        [Tooltip("If you call combine with mod pack, you can force it to not combine this spawner's results")]
        [HideInInspector] public ECombineSet SetCombined = ECombineSet.SetCombined;

        public enum EStaticSet { Auto, ForceStatic, ForceNotStatic }
        [Tooltip("By default it will not set resulting mesh as static, but if mod pack is sheduling meshes to be static it will be set static")]
        [HideInInspector] public EStaticSet SetStatic = EStaticSet.Auto;

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            if (SetCombined == ECombineSet.ForceNotCombine) spawn.CombineMode = SpawnData.ECombineMode.None;
            else
            {
                if (spawn.CombineMode == SpawnData.ECombineMode.None) // Pack not set this spawn as combined 
                {
                    if (SetStatic == EStaticSet.ForceStatic)
                        spawn.CombineMode = SpawnData.ECombineMode.CombineStatic;
                    else
                        spawn.CombineMode = SpawnData.ECombineMode.Combine;
                }
                else
                {
                    if (SetStatic == EStaticSet.ForceStatic)
                        spawn.CombineMode = SpawnData.ECombineMode.CombineStatic;
                    else if (SetStatic == EStaticSet.ForceNotStatic)
                        spawn.CombineMode = SpawnData.ECombineMode.Combine;
                }
            }
        }

        #region Editor GUI

#if UNITY_EDITOR

        SerializedProperty sp_SetCombined = null;
        SerializedProperty sp = null;
        //private bool _advancedFoldout = false;

        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox(" Right now mesh combine is supported only for renderers with single material on !", MessageType.None);
            EditorGUILayout.HelpBox(" Combine all spawned tiles into single mesh. ! Don't use it on the dynamic objects like movable props !", MessageType.Info);

            if (sp_SetCombined == null) sp_SetCombined = so.FindProperty("SetCombined");

            var pack = TryGetParentModPack();

            bool drawSetCombined = true;
            if (pack) if (pack.CombineSpawns == ModificatorsPack.EPackCombine.None) drawSetCombined = false;

            if (drawSetCombined)
            {
                EditorGUILayout.PropertyField(sp_SetCombined);
                GUILayout.Space(3);
            }
            else
            {
                SetCombined = ECombineSet.SetCombined;
            }

            base.NodeBody(so);

            if (sp == null) sp = so.FindProperty("SetStatic");

            if (pack)
            {
                if (pack.CombineSpawns != ModificatorsPack.EPackCombine.None)
                {
                    EditorGUILayout.HelpBox(" The parent pack '" + pack.name + "' is calling combine with " + pack.CombineSpawns.ToString() + " mode.", MessageType.None);

                    if (SetCombined == ECombineSet.ForceNotCombine)
                    {
                        EditorGUILayout.HelpBox(" Force Not Combine : so disabling mesh combine for this tile!", MessageType.None);

                    }
                }
            }

            GUILayout.Space(3);

            if (SetCombined == ECombineSet.SetCombined)
            {
                EditorGUILayout.PropertyField(sp);

                if (SetStatic == EStaticSet.Auto)
                {
                    bool packSet = false;
                    if (pack) if (pack.CombineSpawns != ModificatorsPack.EPackCombine.None) packSet = true;

                    if (packSet)
                    {
                        if (pack.CombineSpawns == ModificatorsPack.EPackCombine.CombineAll)
                        {
                            EditorGUILayout.LabelField("Result: Not Static", EditorStyles.centeredGreyMiniLabel);
                        }
                        else
                        if (pack.CombineSpawns == ModificatorsPack.EPackCombine.CombineAllAndSetStatic)
                        {
                            EditorGUILayout.LabelField("Result: Static", EditorStyles.centeredGreyMiniLabel);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Result: Not Static", EditorStyles.centeredGreyMiniLabel);
                    }
                }

                GUILayout.Space(4);
            }


            //GUILayout.Space(3);
            //FGUI_Inspector.FoldHeaderStart(ref _advancedFoldout, "  Advanced", EditorStyles.helpBox);

            //if ( _advancedFoldout)
            //{
            //    EditorGUILayout.HelpBox("Advanced Randomization per Object is not yet implemented!", MessageType.Info);
            //}

            //GUILayout.EndVertical();

        }

#endif

        #endregion

    }
}