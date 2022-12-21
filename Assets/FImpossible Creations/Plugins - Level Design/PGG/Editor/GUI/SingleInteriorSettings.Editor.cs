using FIMSpace.FEditor;
using FIMSpace.Generating.Planning;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.Generating
{

    public static class SingleInteriorSettingsEditor
    {
        public static void DrawGUI(SingleInteriorSettings settingsPreset, SerializedObject so, SerializedProperty prp, bool drawField, int id = -1, bool multiRectNotSupp = false)
        {
            if (settingsPreset == null) return;
            if (so == null) return;
            if (prp == null) return;

            EditorGUI.BeginChangeCheck();
            SerializedProperty sp = prp.Copy();

            SingleInteriorSettings sett = settingsPreset;
            prp.Next(true);

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();

            if (id >= 0) EditorGUILayout.LabelField("[" + id + "]", GUILayout.Width(26));

            if (sett.CustomName == "" && sett.FieldSetup != null)
            {
                string fieldText = sett.CustomName;
                fieldText = EditorGUILayout.TextField(sett.FieldSetup.name);
                if (fieldText != "" && fieldText != sett.FieldSetup.name) sett.CustomName = fieldText;
            }
            else
                sett.CustomName = EditorGUILayout.TextField(sett.CustomName);

            if (!drawField)
            {
                GUILayout.Space(4);
                SerializedProperty sp_dupl = sp.FindPropertyRelative("Duplicates");
                EditorGUIUtility.labelWidth = 72;
                prp.NextVisible(false);
                if (sp_dupl.intValue < 1) sp_dupl.intValue = 1;
                EditorGUILayout.PropertyField(sp_dupl, GUILayout.Width(100));
                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUILayout.LabelField(" Settings:", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);

            prp.NextVisible(false);
            prp.NextVisible(false);
            if (drawField)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 100;
                EditorGUILayout.PropertyField(prp); // preset field
                GUILayout.Space(6);
                EditorGUIUtility.labelWidth = 70;
                prp.NextVisible(false);
                if (prp.intValue < 1) prp.intValue = 1;
                EditorGUILayout.PropertyField(prp, GUILayout.Width(100));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //prp.NextVisible(false);
            }

            prp.NextVisible(false); // door hole command

            FieldSetup setp = settingsPreset.FieldSetup;
            if (setp)
            {
                EditorGUILayout.BeginHorizontal();
                GUIContent[] commands = new GUIContent[setp.CellsInstructions.Count];
                int[] commandsI = new int[setp.CellsInstructions.Count];
                for (int i = 0; i < commands.Length; i++) { commands[i] = new GUIContent(setp.CellsInstructions[i].Title); commandsI[i] = i; }
                settingsPreset.DoorHoleCommandID = EditorGUILayout.IntPopup(new GUIContent("Door Hole Command", "Default door hole connection command"), settingsPreset.DoorHoleCommandID, commands, commandsI);
                prp.NextVisible(false);
                GUILayout.Space(7);
                EditorGUIUtility.labelWidth = 65;
                EditorGUILayout.PropertyField(prp, new GUIContent("Centerize: "), GUILayout.Width(90));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
            }
            else
                prp.NextVisible(false);



            GUILayout.Space(4);
            prp.NextVisible(false);

            if (settingsPreset.JustOneDoor)
            {
                EditorGUIUtility.labelWidth = 90;
                EditorGUILayout.PropertyField(prp);
                prp.NextVisible(false);
                prp.NextVisible(false);
            }
            else
            {
                GUILayout.Space(3);
                EditorGUILayout.BeginHorizontal();
                settingsPreset.JustOneDoor = EditorGUILayout.Toggle(settingsPreset.JustOneDoor, GUILayout.Width(20));
                prp.NextVisible(false);
                EditorGUILayout.PropertyField(prp);
                EditorGUILayout.EndHorizontal();
            }


            GUILayout.Space(7);

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            prp.NextVisible(false);

            if (multiRectNotSupp) if (settingsPreset.OptionalShapePreset != null) if (settingsPreset.OptionalShapePreset.Setup.GenerationMode != GenerationShape.EGenerationMode.RandomRectangle && settingsPreset.OptionalShapePreset.Setup.GenerationMode != GenerationShape.EGenerationMode.StaticSizeRectangle)
                    { EditorGUILayout.HelpBox("Building Plan Preview is not supporting multi-rect shapes", MessageType.Info); }

            prp.NextVisible(false);
            if (settingsPreset.OptionalShapePreset == null)
            {
                DrawCompactSetup(settingsPreset.InternalSetup);
            }
            else
                EditorGUILayout.HelpBox("Interior Size is defined by generation shape preset file", MessageType.None);

            GUILayout.Space(2);
            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(2);
            prp.NextVisible(false);

            string fold = settingsPreset._editorAdvancedFoldout ? " ▼" : " ►";
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            if (GUILayout.Button(fold + "  Advanced Settings", EditorStyles.label, GUILayout.Width(200))) settingsPreset._editorAdvancedFoldout = !settingsPreset._editorAdvancedFoldout;
           
            if (settingsPreset._editorAdvancedFoldout)
            {
                GUILayout.Space(15);

                //EditorGUILayout
                SerializedProperty restrPos = sp.FindPropertyRelative("RestrictPosition");
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 110;
                EditorGUILayout.PropertyField(restrPos);

                restrPos.NextVisible(false);
                EditorGUIUtility.labelWidth = 170;
                if (settingsPreset.RestrictPosition) EditorGUILayout.PropertyField(restrPos);
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.EndHorizontal();

                if (settingsPreset.RestrictPosition)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 114;
                    restrPos.NextVisible(false); EditorGUILayout.PropertyField(restrPos);
                    EditorGUIUtility.labelWidth = 50;
                    GUILayout.Space(10);
                    restrPos.NextVisible(false); EditorGUILayout.PropertyField(restrPos, new GUIContent("Than"));
                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = 0;
                }
                else
                {
                    restrPos.NextVisible(false); restrPos.NextVisible(false);
                }


                SerializedProperty sp_shapePres = sp.FindPropertyRelative("OptionalShapePreset");
                GUILayout.Space(8);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sp_shapePres);
                sp_shapePres.NextVisible(false);
                var nShp = ModificatorsPackEditor.DrawNewScriptableCreateButton<GenerationShape>("GenShape_", "Generating new GenerationShape preset in choosed project directory");
                if (nShp != null) settingsPreset.OptionalShapePreset = nShp;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);

                EditorGUILayout.PropertyField(sp_shapePres);
                sp_shapePres.NextVisible(false);
                GUILayout.Space(7);
                EditorGUILayout.PropertyField(sp_shapePres);
                sp_shapePres.NextVisible(false);
                EditorGUILayout.PropertyField(sp_shapePres);
                sp_shapePres.NextVisible(false);
                EditorGUILayout.PropertyField(sp_shapePres);
                GUILayout.Space(7);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            so.ApplyModifiedProperties();

            
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
                if (so != null) EditorUtility.SetDirty(so.targetObject);
            }
        }
        private static bool changed = false;
        public static bool IfChanged() { if (changed) { changed = false; return true; } return false; }


        static void DrawCompactSetup(GenerationShape.GenerationSetup setup)
        {
            EditorGUIUtility.labelWidth = 44;

            if (setup.GenerationMode == GenerationShape.EGenerationMode.RandomTunnels)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Branches Count", GUILayout.Width(120));
                setup.TargetBranches.Min = EditorGUILayout.IntField("From:", setup.TargetBranches.Min);
                if (setup.TargetBranches.Min < 0) setup.TargetBranches.Min = 0;
                GUILayout.Space(10);
                setup.TargetBranches.Max = EditorGUILayout.IntField("Up to:", setup.TargetBranches.Max);
                if (setup.TargetBranches.Max < 0) setup.TargetBranches.Max = 0;
                if (setup.TargetBranches.Min > setup.TargetBranches.Max) setup.TargetBranches.Max = setup.TargetBranches.Min;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Branches Lengths", GUILayout.Width(135));
                setup.BranchLength.Min = EditorGUILayout.IntField("From:", setup.BranchLength.Min);
                if (setup.BranchLength.Min < 0) setup.BranchLength.Min = 0;
                GUILayout.Space(10);
                setup.BranchLength.Max = EditorGUILayout.IntField("Up to:", setup.BranchLength.Max);
                if (setup.BranchLength.Max < 0) setup.BranchLength.Max = 0;
                if (setup.BranchLength.Min > setup.BranchLength.Max) setup.BranchLength.Max = setup.BranchLength.Min;
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Thickness In Cells", GUILayout.Width(130));
                setup.BranchThickness.Min = EditorGUILayout.IntField("From:", setup.BranchThickness.Min);
                if (setup.BranchThickness.Min < 1) setup.BranchThickness.Min = 1;
                GUILayout.Space(10);
                setup.BranchThickness.Max = EditorGUILayout.IntField("Up to:", setup.BranchThickness.Max);
                if (setup.BranchThickness.Max < 1) setup.BranchThickness.Max = 1;
                if (setup.BranchThickness.Min > setup.BranchThickness.Max) setup.BranchThickness.Max = setup.BranchThickness.Min;
                EditorGUILayout.EndHorizontal();

            }
            else
            {
                GUILayout.Space(6);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Field Size: ", GUILayout.Width(80));

                setup.RectSetup.Width.Max = EditorGUILayout.IntField("Width: ", setup.RectSetup.Width.Max, GUILayout.Width(80));
                GUILayout.Space(12);
                setup.RectSetup.Height.Max = EditorGUILayout.IntField("Height: ", setup.RectSetup.Height.Max, GUILayout.Width(80));

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(new GUIContent(FGUI_Resources.Tex_Info, "Use Advanced Tab for more options like unregular shape or random size"), GUILayout.Width(16));

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            EditorGUIUtility.labelWidth = 0;
        }


        public static void DrawGUIChunkSetup(string title, SingleInteriorSettings settingsPreset, SerializedObject so, SerializedProperty prp, ref bool foldout, bool isRootCorridor = false/*, bool drawCenterize = false, bool unlimitedDoorsCount = true, bool drawShapeField = false*/)
        {
            if (settingsPreset == null) return;
            if (so == null) return;
            if (prp == null) return;

            EditorGUI.BeginChangeCheck();

            string fold = foldout ? " ▼" : " ►";
            Color preC = GUI.color;
            EditorGUIUtility.labelWidth = 0;
            GUI.color = new Color(0.4f, 1f, 0.5f, 1f);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);
            GUI.color = preC;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(fold + "  " + title, EditorStyles.label, GUILayout.Width(200))) foldout = !foldout;

            if (foldout)
                if (isRootCorridor)
                {
                    EditorGUIUtility.labelWidth = 60;
                    settingsPreset.CustomName = EditorGUILayout.TextField(new GUIContent("IDName:", "Can be used to not allowing connection with"), settingsPreset.CustomName);
                    EditorGUIUtility.labelWidth = 0;
                }

            EditorGUILayout.EndHorizontal();

            if (foldout)
            {
                GUILayout.Space(4);

                SingleInteriorSettings sett = settingsPreset;
                prp.Next(true);

                prp.NextVisible(false);
                prp.NextVisible(false); EditorGUILayout.PropertyField(prp); // preset field

                prp.NextVisible(false); // door hole command

                FieldSetup setp = settingsPreset.FieldSetup;
                if (setp)
                {
                    GUIContent[] commands = new GUIContent[setp.CellsInstructions.Count];
                    int[] commandsI = new int[setp.CellsInstructions.Count];
                    for (int i = 0; i < commands.Length; i++) { commands[i] = new GUIContent(setp.CellsInstructions[i].Title); commandsI[i] = i; }
                    settingsPreset.DoorHoleCommandID = EditorGUILayout.IntPopup(new GUIContent("Door Hole Command", "Default door hole connection command"), settingsPreset.DoorHoleCommandID, commands, commandsI);
                    prp.NextVisible(false);
                    GUILayout.Space(7);
                }
                else
                    prp.NextVisible(false);


                GUILayout.Space(4);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

                prp.NextVisible(false);

                prp.NextVisible(false);
                settingsPreset.InternalSetup.GenerationMode = GenerationShape.EGenerationMode.RandomTunnels;
                if (settingsPreset.OptionalShapePreset == null)
                {
                    DrawCompactSetup(settingsPreset.InternalSetup);
                }

                GUILayout.Space(2);
                EditorGUILayout.EndVertical();

                GUILayout.Space(4);
                prp.NextVisible(false);

                //if (unlimitedDoorsCount == false)
                //{
                //    if (settingsPreset.JustOneDoor)
                //    {
                //        EditorGUIUtility.labelWidth = 90;
                //        EditorGUILayout.PropertyField(prp);
                //        prp.NextVisible(false);
                //    }
                //    else
                //    {
                //        GUILayout.Space(3);
                //        EditorGUILayout.BeginHorizontal();
                //        settingsPreset.JustOneDoor = EditorGUILayout.Toggle(settingsPreset.JustOneDoor, GUILayout.Width(20));
                //        prp.NextVisible(false);
                //        EditorGUILayout.PropertyField(prp);
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}
                //else
                //{
                //    prp.NextVisible(false);
                //    GUILayout.Space(-6);
                //}

                // Inject mods
                EditorGUIUtility.labelWidth = 0;
                GUILayout.Space(2);
                prp.NextVisible(false);
                prp.NextVisible(false);
                prp.NextVisible(false);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(prp); EditorGUI.indentLevel--;
                GUILayout.Space(2);


                so.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();

            if ( EditorGUI.EndChangeCheck())
            {
                changed = true;
                if ( so != null) EditorUtility.SetDirty(so.targetObject);
                if ( prp != null) EditorUtility.SetDirty(prp.serializedObject.targetObject);
            }

        }



    }
}