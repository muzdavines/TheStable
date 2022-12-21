using FIMSpace.FEditor;
using FIMSpace.Generating.Planning;
using UnityEditor;
using UnityEngine;
using FIMSpace.Generating;

namespace FIMSpace.Generating
{

    public static class InstructionDefinitionEditor
    {

#if UNITY_EDITOR
        public static void DrawGUI(InstructionDefinition def, FieldSetup setup)
        {
            if (def.InstructionType == InstructionDefinition.EInstruction.PreventSpawnSelective)
            {
                def.Tags = EditorGUILayout.TextField(new GUIContent("Tags:"), def.Tags);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.DoorHole)
            {
                EditorGUILayout.HelpBox("Door Hole: Prevents spawning and after all mods runs modificator", MessageType.None);
                def.Tags = EditorGUILayout.TextField(new GUIContent("Prevent Spawn Tagged:"), def.Tags);
                def.TargetModification = DrawMod("Door Modificator:", def.TargetModification, setup, false, true, def);
                def.InstructionArgument = EditorGUILayout.TextField(new GUIContent("Optional Cell Data:"), def.InstructionArgument);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.PreRunModificator)
            {
                def.TargetModification = DrawMod("To Run:", def.TargetModification, setup, false, true, def);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.PostRunModificator)
            {
                def.TargetModification = DrawMod("To Run:", def.TargetModification, setup, false, true, def);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.InjectStigma)
            {
                def.Tags = EditorGUILayout.TextField(new GUIContent("On Tags:"), def.Tags);
                def.InstructionArgument = EditorGUILayout.TextField(new GUIContent("Spawn Stigma:"), def.InstructionArgument);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.InjectDataString)
            {
                //def.Tags = EditorGUILayout.TextField(new GUIContent("On Tags:"), def.Tags);
                def.InstructionArgument = EditorGUILayout.TextField(new GUIContent("Cell Data String:"), def.InstructionArgument);
            }
            else if (def.InstructionType == InstructionDefinition.EInstruction.IsolatedGrid)
            {
                EditorGUILayout.HelpBox("IsolatedGrid is not working with scaled grids mode", MessageType.None);
                def.TargetModification = DrawMod("To Run:", def.TargetModification, setup, false, true, def);
            }
        }


        static InstructionDefinition changingDef = null;
        static FieldModification applyingMod = null;

        private static FieldModification DrawMod(string title, FieldModification mod, FieldSetup setup, bool drawExportButtons = true, bool drawParentModsFoldout = true, InstructionDefinition def = null)
        {
            EditorGUILayout.BeginHorizontal();

            Color preBc = GUI.backgroundColor;
            bool sel = false;
            if (SeparatedModWindow.Get) if (SeparatedModWindow.Get.latestMod == mod) sel = true;

            if (sel)
            {
                GUI.backgroundColor = Color.green;
            }

            if ( mod)
            if (mod.ParentPreset)
            {
                if (mod.ParentPreset != setup)
                {
                    if (sel)
                        GUI.backgroundColor = new Color(0.0f, 1f, 0.75f, 1f);
                    else
                        GUI.backgroundColor = new Color(0.1f, 0.5f, 1f, 1f);
                }
            }

            mod = (FieldModification)EditorGUILayout.ObjectField(new GUIContent(title), mod, typeof(FieldModification), false);
            GUI.backgroundColor = preBc;

            if (drawParentModsFoldout)
            {
                if (def != null)
                {
                    if (def == changingDef)
                    {
                        mod = applyingMod;
                        changingDef = null;
                    }
                }

                #region Button to display menu of draft field setup files

                if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_DownFold, "Display quick selection menu for FieldMods contained in the parent FieldSetup"), EditorStyles.label, GUILayout.Width(18), GUILayout.Height(16)))
                {
                    if (setup != null)
                    {
                        GenericMenu draftsMenu = new GenericMenu();
                        var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(setup));

                        draftsMenu.AddItem(new GUIContent("None"), mod == null, () => { mod = null; changingDef = def; applyingMod = null; });
                        draftsMenu.AddItem(new GUIContent(""), false, () => { });

                        for (int i = 0; i < objs.Length; i++)
                        {
                            if (!(objs[i] is FieldModification)) continue;
                            FieldModification fs = (FieldModification)objs[i];
                            if (fs)
                            {
                                string name = fs.name;
                                if (fs.name.Length == 0) name = "Untitled Modificator";
                                draftsMenu.AddItem(new GUIContent(name), mod == fs, () => { mod = fs; changingDef = def; applyingMod = fs; });
                            }
                        }

                        draftsMenu.ShowAsContext();
                    }
                }


                #endregion


            }


            if (mod == null)
            {
                mod = ModificatorsPackEditor.DrawNewScriptableCreateButton<FieldModification>();
                if (setup != null) mod = ModificatorsPackEditor.DrawModInjectButton(null, null, setup);
            }
            else
            {
                if (GUILayout.Button(new GUIContent(FGUI_Resources.TexTargetingIcon, "Select Modificator to display it's settings"), EditorStyles.label, GUILayout.Height(18), GUILayout.Width(18)))
                {
                    SeparatedModWindow.SelectMod(mod);
                }

                FieldModification nMod;

                if (mod != null)
                {
                    if (ModificatorsPackEditor.DrawRenameScriptableButton(mod, "modificator", true))
                    {
                        if (def != null)
                        {
                            // Auto change name on right mouse button
                            string newName = def.Title;

                            if (mod.ParentPreset) newName += " - " + mod.ParentPreset.name.Replace("FS_", "");

                            mod.name = newName;
                            AssetDatabase.SaveAssets();
                        }
                    }
                }

                if (drawExportButtons)
                    if (mod.VariantOf == null)
                    {
                        nMod = ModificatorsPackEditor.DrawVariantExportButton(mod);
                        if (nMod != null) mod = nMod;
                    }

                if (setup != null)
                {
                    if (FGenerators.AssetContainsAsset(mod, setup) == false)
                    {
                        nMod = ModificatorsPackEditor.DrawModInjectButton(mod, null, setup);
                        if (nMod != null)
                        {
                            mod = nMod;
                            if (setup) FieldModificationEditor.CleanupAndGetUnused(setup);
                        }
                    }
                }

                if (drawExportButtons)
                {
                    nMod = ModificatorsPackEditor.DrawExportModButton(mod);
                    if (nMod != null) mod = nMod;
                }
            }

            EditorGUILayout.EndHorizontal();

            return mod;
        }
#endif

    }
}