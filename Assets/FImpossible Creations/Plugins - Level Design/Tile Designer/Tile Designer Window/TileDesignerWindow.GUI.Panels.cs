#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FIMSpace.FEditor;
using static FIMSpace.Generating.TileMeshSetup;
using System.Collections.Generic;
using System;

namespace FIMSpace.Generating
{
    public partial class TileDesignerWindow
    {
        List<CurvePoint> _editorPoints = new List<CurvePoint>();
        List<CurvePoint> _editorPoints2 = new List<CurvePoint>();

        List<MeshShapePoint> previewShape { get { return s.previewShape; } }
        List<MeshShapePoint> previewShape2 { get { return s.previewShape2; } }

        int _selectedTileMesh = 0;


        #region Setup GUI

        int _generated_sceneObj = 0;
        int _generated_prefabObj = 0;
        int _generated_meshFile = 0;
        TileDesign _generated_sceneObjParentRef = null;
        GameObject _generated_sceneObjRef = null;

        void DrawTileLeftArrowButton()
        {
            if (EditedDesign.TileMeshes.Count > 1)
                if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22))) { _selectedTileMesh -= 1; RefreshSelectedTileMesh(true); shapeChanged = true; shapeEndChanging = true; repaintRequestCounter = 5; }
        }

        void DrawTileRightArrowButton()
        {
            if (EditedDesign.TileMeshes.Count > 1)
                if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22))) { _selectedTileMesh += 1; RefreshSelectedTileMesh(true); shapeChanged = true; shapeEndChanging = true; repaintRequestCounter = 5; }
        }

        void RefreshSelectedTileMesh(bool setEditedDesign)
        {
            if (_selectedTileMesh >= EditedDesign.TileMeshes.Count) _selectedTileMesh = 0;
            if (_selectedTileMesh < 0) _selectedTileMesh = EditedDesign.TileMeshes.Count - 1;
            if (setEditedDesign) EditedTileSetup = EditedDesign.TileMeshes[_selectedTileMesh];
        }

        void DesignGenericMenu()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Export design setup to the project file"), false, () =>
            {
                TileDesignPreset gen = (TileDesignPreset)FGeneratingUtilities.GenerateScriptable(CreateInstance<TileDesignPreset>(), "Tile Design");

                if (gen)
                {
                    gen.Designs.Add(new TileDesign());
                    gen.Designs[0].PasteEverythingFrom(EditedDesign);
                    Init(gen.Designs[0], gen);
                }
            });

            menu.AddItem(new GUIContent("Copy all design setup"), false, () => { TileDesign._CopyFrom = EditedDesign; });

            if (TileDesign._CopyFrom != null)
                menu.AddItem(new GUIContent("Paste - replace all setup with " + TileDesign._CopyFrom.DesignName + " parameters"), false, () => { EditedDesign.PasteEverythingFrom(TileDesign._CopyFrom); });

            menu.ShowAsContext();
        }

        void GUISetup()
        {
            Color preC = GUI.color;

            if (editingTempDesign || ToDirty == null)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.HelpBox("Editing design which is not saved in the project, save to new file if you want to keep it!", MessageType.None);

                if (GUILayout.Button("Save", GUILayout.Width(48)))
                {
                    TileDesignPreset gen = (TileDesignPreset)FGeneratingUtilities.GenerateScriptable(CreateInstance<TileDesignPreset>(), "Tile Design");

                    if (gen)
                    {
                        gen.Designs.Add(new TileDesign());
                        gen.Designs[0].PasteEverythingFrom(EditedDesign);
                        Init(gen.Designs[0], gen);
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);
            }

            if (ToDirty != null)
                if (ToDirty.GetType() == typeof(TileDesignPreset))
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField("", ToDirty, typeof(TileDesignPreset), false);
                    GUI.enabled = true;
                }

            if (GUILayout.Button("Tile Designer", FGUI_Resources.HeaderStyle)) { DesignGenericMenu(); }
            if (GUILayout.Button("Generate single wall/column/architecture tile for spawning", EditorStyles.centeredGreyMiniLabel)) { DesignGenericMenu(); }
            //GUILayout.Label("Generate single wall/column/architecture tile for spawning", EditorStyles.centeredGreyMiniLabel);

            var d = EditedDesign;

            GUILayout.Space(4);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            d.DesignName = EditorGUILayout.TextField("Whole Design Name:", d.DesignName);
            d.DefaultMaterial = (Material)EditorGUILayout.ObjectField("Default Material:", d.DefaultMaterial, typeof(Material), false);

            EditorGUILayout.EndVertical();


            FGUI_Inspector.DrawUILine(0.3f, 0.5f);

            GUILayout.Space(4);


            #region Meshes Foldout

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            if (d.TileMeshes.Count == 0)
            {
                d.TileMeshes.Add(new TileMeshSetup());
            }


            bool wasFolded = _foldout_meshSetup;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_foldout_meshSetup, true), EditorStyles.label, GUILayout.Height(19), GUILayout.Width(20))) _foldout_meshSetup = !_foldout_meshSetup;
            if (GUILayout.Button(new GUIContent("  Tile Editor Meshes (" + d.TileMeshes.Count + ")", EditorGUIUtility.IconContent("Mesh Icon").image), EditorStyles.label, GUILayout.Height(19))) _foldout_meshSetup = !_foldout_meshSetup;

            if (_foldout_meshSetup)
            {

                EditorGUILayout.BeginHorizontal(GUILayout.Height(19));
                //if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22))) { _selectedTileMesh -= 1; }
                DrawTileLeftArrowButton();

                GUILayout.Label((_selectedTileMesh + 1) + "/" + d.TileMeshes.Count, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(19));

                DrawTileRightArrowButton();
                //if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22))) { _selectedTileMesh += 1; }
                EditorGUILayout.EndHorizontal();



                GUILayout.Space(12);
                if (GUILayout.Button("+", FGUI_Resources.ButtonStyle, GUILayout.Height(18), GUILayout.Width(22))) { d.TileMeshes.Add(new TileMeshSetup("Tile Mesh " + (d.TileMeshes.Count + 1))); _selectedTileMesh += 1; }
                GUILayout.Space(4);
            }
            else
            {
                if (GUILayout.Button(new GUIContent(" Go", FGUI_Resources.Tex_GearSetup), EditorStyles.label, GUILayout.Width(50), GUILayout.Height(19)))
                {
                    EditedTileSetup = d.TileMeshes[_selectedTileMesh];
                    RefreshDefaultCurve(EditedTileSetup.GenTechnique);
                    GenerateTileMeshSetupMesh();

                    EditorMode = EMode.MeshEditor;
                    shapeChanged = true;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (_foldout_meshSetup)
            {
                RefreshSelectedTileMesh(false);
                var sel = d.TileMeshes[_selectedTileMesh];
                int toRemove = -1;

                GUILayout.Space(4);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                GUILayout.BeginHorizontal();
                sel.Name = EditorGUILayout.TextField("Name:", sel.Name);

                GUI.enabled = d.TileMeshes.Count > 1;
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f, 1f);
                if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width(25), GUILayout.Height(19))) { toRemove = _selectedTileMesh; }
                GUI.backgroundColor = Color.white;
                GUI.enabled = true;

                GUILayout.EndHorizontal();

                GUILayout.Space(3);

                if (EditedTileSetup != null)
                    if (EditedTileSetup.GenTechnique == EMeshGenerator.CustomMeshAndExtras)
                    {
                        EditedTileSetup.CustomMesh = (Mesh)EditorGUILayout.ObjectField("Custom Mesh:", EditedTileSetup.CustomMesh, typeof(Mesh), false);
                    }

                GUILayout.BeginHorizontal();
                sel.Material = (Material)EditorGUILayout.ObjectField("Override Material:", sel.Material, typeof(Material), false);
                if (GUILayout.Button(FGUI_Resources.GUIC_Info, EditorStyles.label, GUILayout.Width(17))) { EditorUtility.DisplayDialog("Material Info", "If you override materials, then there will be generated multiple meshes instead of single mesh which is more optimal!\nSo it's better to leave this field empty if it's not neccessary to use multiple materials.", "Ok"); }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.Space(6);
                GUI.backgroundColor = new Color(0.7f, 1f, 0.7f, 1f);

                if (GUILayout.Button(new GUIContent("  Switch to Tile Editor", FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Height(24)))
                {
                    EditedTileSetup = sel;
                    EditorMode = EMode.MeshEditor;
                    shapeChanged = true;
                }

                GUI.backgroundColor = Color.white;
                GUILayout.Space(6);
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                EditorGUILayout.EndVertical();

                if (toRemove != -1)
                {
                    d.TileMeshes.RemoveAt(toRemove);
                    _selectedTileMesh -= 1;
                }

                if (wasFolded != _foldout_meshSetup)
                {
                    if (EditedTileSetup == null) EditedTileSetup = sel;

                    if (EditedTileSetup != null)
                    {
                        if (EditedTileSetup.LatestGeneratedMesh == null)
                        {
                            RefreshDefaultCurve(EditedTileSetup.GenTechnique);
                            GenerateTileMeshSetupMesh();
                        }
                        else
                        {
                            TileMeshUpdatePreview();
                        }
                    }

                    shapeChanged = true;
                }
            }

            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Space(3);
            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);


            #region Object Settings Foldout

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_foldout_gameObjectSetup, true), EditorStyles.label, GUILayout.Height(19), GUILayout.Width(20))) _foldout_gameObjectSetup = !_foldout_gameObjectSetup;
            if (GUILayout.Button(new GUIContent(" Target Parameters", EditorGUIUtility.IconContent("Prefab Icon").image), EditorStyles.label, GUILayout.Height(19))) _foldout_gameObjectSetup = !_foldout_gameObjectSetup;

            if (_foldout_gameObjectSetup)
            {
                #region Backup Code

                //EditorGUILayout.BeginHorizontal(GUILayout.Height(19));
                //if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22))) { }

                //GUILayout.Label("Base", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(19));

                //if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22))) { }
                //EditorGUILayout.EndHorizontal();

                //GUILayout.Space(12);
                //if (GUILayout.Button("+", FGUI_Resources.ButtonStyle, GUILayout.Height(18), GUILayout.Width(22))) { }
                //GUILayout.Space(4);
                #endregion

                if (TileDesign._copyGameObjectSetFrom != null && TileDesign._copyGameObjectSetFrom != EditedDesign)
                {
                    if (DrawPasteButton()) { TileDesign.PasteGameObjectParameters(TileDesign._copyGameObjectSetFrom, EditedDesign); TileDesign._copyGameObjectSetFrom = null; }
                }

                if (DrawCopyButton()) { EditedDesign.CopyGameObjectParameters(); }
            }


            EditorGUILayout.EndHorizontal();

            if (_foldout_gameObjectSetup)
            {
                GUILayout.Space(2);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                EditorGUIUtility.labelWidth = 102;

                d.Static = EditorGUILayout.Toggle("Static:", d.Static);
                Rect paramRect = GUILayoutUtility.GetLastRect();
                paramRect.position += new Vector2(0, EditorGUIUtility.singleLineHeight + 2);
                d.Tag = EditorGUI.TagField(paramRect, "Tag:", d.Tag);
                paramRect.position += new Vector2(0, EditorGUIUtility.singleLineHeight + 2);
                d.Layer = EditorGUI.LayerField(paramRect, "Layer:", d.Layer);
                GUILayout.Space((EditorGUIUtility.singleLineHeight + 2) * 2);

                GUILayout.Space(8);


                EditorGUILayout.LabelField(new GUIContent("  Attach Components:", EditorGUIUtility.IconContent("cs Script Icon").image));

                #region Attach components menu



                EditorGUI.BeginChangeCheck();

                int attachRemove = -1;
                for (int i = 0; i < d._editor_ToAttach.Count; i++)
                {
                    if (GUI_DisplayAttachable(d._editor_ToAttach, i)) attachRemove = i;
                }

                if (EditorGUI.EndChangeCheck()) { d.Editor_SyncToAttach(); _SetDirty(); }

                if (attachRemove >= 0)
                {
                    d._editor_ToAttach.RemoveAt(attachRemove);
                    d.Editor_SyncToAttach();
                }

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(18);
                if (GUILayout.Button(" + Add MonoBehaviour to Attach +", FGUI_Resources.ButtonStyle, GUILayout.Height(16)))
                {
                    d._editor_ToAttach.Add(null);
                    d.Editor_SyncToAttach();
                }
                GUILayout.Space(18);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                #endregion


                GUILayout.Space(8);
                EditorGUILayout.LabelField(new GUIContent("  Send Messages:", EditorGUIUtility.IconContent("EventSystem Icon").image));

                #region Send Messages Menu

                int messageRemove = -1;
                for (int i = 0; i < d.SendMessages.Count; i++)
                {
                    if (GUI_DisplaySendMessageHelper(d.SendMessages, i)) messageRemove = i;
                }

                if (messageRemove >= 0)
                {
                    d.SendMessages.RemoveAt(messageRemove);
                }

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Space(18);
                if (GUILayout.Button(" + Add Message To Send +", FGUI_Resources.ButtonStyle, GUILayout.Height(16)))
                {
                    d.SendMessages.Add(new TileDesign.SendMessageHelper());
                }
                GUILayout.Space(18);
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                #endregion


                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Space(10);


            #region Collider Settings Foldout

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_foldout_colliderSetup, true), EditorStyles.label, GUILayout.Height(19), GUILayout.Width(20))) _foldout_colliderSetup = !_foldout_colliderSetup;
            if (GUILayout.Button(new GUIContent(" Collider Parameters", EditorGUIUtility.IconContent("SphereCollider Icon").image), EditorStyles.label, GUILayout.Height(19))) _foldout_colliderSetup = !_foldout_colliderSetup;

            if (_foldout_colliderSetup)
            {
                #region Backup Code

                //EditorGUILayout.BeginHorizontal(GUILayout.Height(19));
                //if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22))) { }

                //GUILayout.Label("Base", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(19));

                //if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22))) { }
                //EditorGUILayout.EndHorizontal();

                //GUILayout.Space(12);
                //if (GUILayout.Button("+", FGUI_Resources.ButtonStyle, GUILayout.Height(18), GUILayout.Width(22))) { }
                //GUILayout.Space(4);
                #endregion

                if (TileDesign._copyColliderSetFrom != null && TileDesign._copyColliderSetFrom != EditedDesign)
                {
                    if (DrawPasteButton()) { TileDesign.PasteColliderParameters(TileDesign._copyColliderSetFrom, EditedDesign); TileDesign._copyGameObjectSetFrom = null; }
                }

                if (DrawCopyButton()) { EditedDesign.CopyColliderParameters(); }
            }

            EditorGUILayout.EndHorizontal();

            if (_foldout_colliderSetup)
            {
                GUILayout.Space(2);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                d.AddRigidbody = EditorGUILayout.Toggle("Add Rigidbody", d.AddRigidbody);
                if (d.AddRigidbody)
                {
                    EditorGUI.indentLevel++;
                    d.IsKinematic = EditorGUILayout.Toggle("Kinematic", d.IsKinematic);
                    d.RigidbodyMass = EditorGUILayout.FloatField("Rigidbody Mass", d.RigidbodyMass);
                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(8);

                d.ColliderMode = (TileDesign.EColliderMode)EditorGUILayout.EnumPopup("Collision Type:", d.ColliderMode);

                if (d.ColliderMode != TileDesign.EColliderMode.None)
                {
                    d.CollidersMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Colliders Material", d.CollidersMaterial, typeof(PhysicMaterial), false);
                    GUILayout.Space(4);

                    if (d.ColliderMode == TileDesign.EColliderMode.BoundingBox || d.ColliderMode == TileDesign.EColliderMode.MultipleBoundingBoxes || d.ColliderMode == TileDesign.EColliderMode.SphereCollider)
                    {
                        d.ScaleColliders = EditorGUILayout.Slider("Scale Colliders:", d.ScaleColliders, 0.5f, 2f);
                    }
                    else if (d.ColliderMode != TileDesign.EColliderMode.None)
                    {
                        d.ConvexCollider = EditorGUILayout.Toggle("Convex: ", d.ConvexCollider);
                    }

                    if (d.ColliderMode == TileDesign.EColliderMode.BoundingBox)
                    {
                        d.ExpandThinCollider = EditorGUILayout.Vector3Field("Expand if Thin:", d.ExpandThinCollider);
                    }

                }

                GUILayout.Space(4);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Space(10);


            #region Finalization Settings Foldout

            wasFolded = _foldout_finalizeSetup;

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(FGUI_Resources.GetFoldSimbolTex(_foldout_finalizeSetup, true), EditorStyles.label, GUILayout.Height(19), GUILayout.Width(20))) _foldout_finalizeSetup = !_foldout_finalizeSetup;
            if (GUILayout.Button(new GUIContent(" Finalize Tile", FGUI_Resources.Tex_Tweaks), EditorStyles.label, GUILayout.Height(19))) _foldout_finalizeSetup = !_foldout_finalizeSetup;

            if (!_foldout_finalizeSetup)
            {
                if (GUILayout.Button(new GUIContent(" Go", FGUI_Resources.Tex_Extension), EditorStyles.label, GUILayout.Width(50), GUILayout.Height(19))) EditorMode = EMode.Combine;
            }

            EditorGUILayout.EndHorizontal();

            if (_foldout_finalizeSetup)
            {
                GUILayout.Space(2);

                EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

                GUILayout.Space(2);

                EditorGUILayout.HelpBox("In the finalize stage, you can create final model out of multiple meshes prepared in Tile Editor", MessageType.Info);

                GUILayout.Space(4);

                if (GUILayout.Button(new GUIContent("  Switch to Combiner", FGUI_Resources.Tex_Extension), FGUI_Resources.ButtonStyle, GUILayout.Height(22))) { EditorMode = EMode.Combine; }

                GUILayout.Space(8);

                EditorGUILayout.EndVertical();

                if (_foldout_finalizeSetup != wasFolded)
                {
                    if (EditedDesign.IsSomethingGenerated == false || _comb_autoRefresh)
                    {
                        GenerateCombinedMesh();
                    }
                }
            }

            EditorGUILayout.EndVertical();

            #endregion


            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button(new GUIContent("  Re-Generate Meshes", FGUI_Resources.Tex_Expose), GUILayout.Height(30)))
            //{
            //    d.FullGenerateStack();
            //}

            if (GUILayout.Button(new GUIContent("  Generate Prefab", EditorGUIUtility.IconContent("Prefab Icon").image), GUILayout.Height(30), GUILayout.Width(160)))
            {
                if (_generated_prefabObj < 1)
                    EditorUtility.DisplayDialog("Generating Prefab", "Algorithm will generate prefab and place it inside provided project directory, it will also generate unity .mesh file as sub-asset in the prefab file. (mesh needs to be saved in file in order to appear in other game scenes than current one)", "Ok");

                #region Generating Prefab

                string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Prefab File", d.DesignName, "prefab", "Enter name of file");

                if (!string.IsNullOrEmpty(path))
                {

                    GameObject pf = d.GeneratePrefab();

                    bool success;
                    GameObject newPf = PrefabUtility.SaveAsPrefabAsset(pf, path, out success);

                    if (success)
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        for (int i = 0; i < d.LatestGeneratedMeshes.Count; i++)
                        {
                            if (d.LatestGeneratedMeshes[i] == null) continue;
                            AssetDatabase.AddObjectToAsset(d.LatestGeneratedMeshes[i], newPf);
                        }

                        if (d._UsedCombinedCollisionMesh != null)
                        {
                            AssetDatabase.AddObjectToAsset(d._UsedCombinedCollisionMesh, newPf);
                        }

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                        List<MeshFilter> filters = FTransformMethods.FindComponentsInAllChildren<MeshFilter>(newPf.transform, true);

                        for (int f = 0; f < filters.Count; f++)
                        {
                            MeshFilter filt = filters[f];

                            for (int i = 0; i < subAssets.Length; i++)
                            {
                                Mesh m = subAssets[i] as Mesh;

                                if (m)
                                {
                                    if (m.name == d.LatestGeneratedMeshes[f].name)
                                        if (m.vertexCount == d.LatestGeneratedMeshes[f].vertexCount)
                                        {
                                            filt.sharedMesh = m;
                                            break;
                                        }
                                }

                            }
                        }



                        if (d._UsedCombinedCollisionMesh != null)
                        {
                            MeshCollider meshColl = newPf.GetComponent<MeshCollider>();

                            for (int i = 0; i < subAssets.Length; i++)
                            {
                                Mesh msh = subAssets[i] as Mesh;

                                if (msh)
                                {
                                    if (msh.name == d._UsedCombinedCollisionMesh.name)
                                        if (msh.vertexCount == d._UsedCombinedCollisionMesh.vertexCount)
                                        {
                                            meshColl.sharedMesh = msh;
                                            break;
                                        }
                                }
                            }
                        }
                        else
                        {

                            List<MeshCollider> meshColls = FTransformMethods.FindComponentsInAllChildren<MeshCollider>(newPf.transform, true);
                            for (int m = 0; m < meshColls.Count; m++)
                            {
                                MeshCollider mshCols = meshColls[m];

                                for (int i = 0; i < subAssets.Length; i++)
                                {
                                    Mesh msh = subAssets[i] as Mesh;

                                    if (msh)
                                    {
                                        if (msh.name == d.LatestGeneratedMeshes[m].name)
                                            if (msh.vertexCount == d.LatestGeneratedMeshes[m].vertexCount)
                                            {
                                                mshCols.sharedMesh = msh;
                                                break;
                                            }
                                    }

                                }

                            }

                        }



                        EditorUtility.SetDirty(newPf);
                        FGenerators.DestroyObject(pf);
                        EditorGUIUtility.PingObject(newPf);
                    }
                    else
                    {
                        FGenerators.DestroyObject(pf);
                        UnityEngine.Debug.LogError("Something went wrong when generating prefab!");
                    }

                }

                _generated_prefabObj++;

                #endregion

            }

            if (GUILayout.Button(new GUIContent(" Scene Object", EditorGUIUtility.IconContent("SceneAsset Icon").image), GUILayout.Height(30), GUILayout.Width(128)))
            {
                if (_generated_sceneObj < 3)
                    EditorUtility.DisplayDialog("Generating Scene Object", "Algorithm will create tile designer object on the scene, the meshes will be stored inside scene file, so creating prefab will lost meshes references!", "Ok, I will not create prefab from this scene object");

                GenerateSceneObject();

                _generated_sceneObj += 1;
            }

            if (GUILayout.Button(new GUIContent(" Export", EditorGUIUtility.IconContent("Mesh Icon").image, "Export unity mesh file"), GUILayout.Height(30), GUILayout.Width(82)))
            {
                if (_generated_meshFile < 1)
                    EditorUtility.DisplayDialog("Generating Mesh File", "With mesh file you should be able to do further changes to the model with other plugins or use other plugins to export it as .fbx file", "Ok");

                #region Generating Mesh file

                string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Generate Mesh File", d.DesignName, "asset", "Enter name of file");

                if (!string.IsNullOrEmpty(path))
                {
                    d.FullGenerateStack();

                    for (int m = 0; m < d.LatestGeneratedMeshes.Count; m++)
                    {
                        if (m == 0)
                        {
                            AssetDatabase.CreateAsset(d.LatestGeneratedMeshes[0], path);
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(d.LatestGeneratedMeshes[m], path.Replace(".asset", (m + 1).ToString() + ".asset"));
                        }
                    }

                    AssetDatabase.SaveAssets();
                    var toPing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if (toPing) EditorGUIUtility.PingObject(toPing);
                }


                _generated_meshFile++;

                #endregion

            }

            EditorGUILayout.LabelField("If you recompile some scripts,close\ntile designer window and open it again", EditorStyles.centeredGreyMiniLabel, GUILayout.Height(28));

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            DisplayGeneratedMeshInfo(d);
            GUILayout.Space(4);

            EditorGUILayout.EndVertical();


            if (position.height < 430) return;

            Rect rLowRect = position;

            float heightRatio = Mathf.InverseLerp(450, 550, position.height);
            GUI.color = new Color(1f, 1f, 1f, 0.25f + heightRatio * 0.6f);

            float smaller = 40f - Mathf.Min(40f, (heightRatio * 1.65f) * 40f);
            float previewSize = 110f - smaller;
            rLowRect.position = new Vector2(position.width - previewSize, position.height - (previewSize + 4));
            rLowRect.size = new Vector2(previewSize - 3, previewSize);


            if (_foldout_finalizeSetup)
            {
                var selDes = EditedDesign;
                if (selDes != null)
                {
                    if (selDes.IsSomethingGenerated == false)
                    {
                        GenerateCombinedMesh();
                    }

                    if (selDes.IsSomethingGenerated)
                    {
                        RefreshCombinedMeshPreviewDisplayer();

                        if (combinedMeshDisplay != null)
                        {
                            combinedMeshDisplay.UpdateMesh(selDes);
                            CombinePreviewDisplay(rLowRect);
                        }

                        //GUI.Label(rLowRect, "Full Preview");
                    }
                }
            }
            else
            {
                if (_selectedTileMesh >= d.TileMeshes.Count) _selectedTileMesh = 0;

                var selTile = d.TileMeshes[_selectedTileMesh];
                if (selTile != null)
                    if (selTile.LatestGeneratedMesh != null)
                        if (_foldout_meshSetup)
                        {
                            TilePreviewDisplay(rLowRect, selTile.LatestGeneratedMesh, selTile.Material);
                            //GUI.Label(rLowRect, d.TileMeshes[_selectedTileMesh].Name);
                        }
            }


            GUI.color = preC;
        }

        private void DisplayGeneratedMeshInfo(TileDesign d, Rect? r = null)
        {
            bool preEn = GUI.enabled;

            GUI.enabled = false;

            if (d._LatestGen_Meshes == 0)
            {
                if (r == null)
                    EditorGUILayout.LabelField("Nothing Generated Yet");
                else
                    GUI.Label(r.Value, "Nothing Generated Yet");
            }
            else
            {
                if (r == null)
                {
                    EditorGUILayout.LabelField("Separated Meshes: " + d._LatestGen_Meshes);
                    EditorGUILayout.LabelField("Vertices: " + d._LatestGen_Vertices + "    Tris: " + d._LatestGen_Tris);
                    EditorGUILayout.LabelField("Bounds Size: " + d._LatestGen_Bounds.size);
                }
                else
                {
                    r = new Rect(r.Value.position + new Vector2(0, -18 * 2), r.Value.size);
                    GUI.Label(r.Value, "Separated Meshes: " + d._LatestGen_Meshes);
                    r = new Rect(r.Value.position + new Vector2(0, 18), r.Value.size);
                    GUI.Label(r.Value, "Vertices: " + d._LatestGen_Vertices + "    Tris: " + d._LatestGen_Tris);
                    r = new Rect(r.Value.position + new Vector2(0, 18), r.Value.size);
                    GUI.Label(r.Value, "Bounds Size: " + d._LatestGen_Bounds.size);
                }
            }

            GUI.enabled = preEn;
        }

        private void _SetDirty()
        {
            if (ToDirty != null) EditorUtility.SetDirty(ToDirty);
        }

        private void GenerateSceneObject()
        {
            var d = EditedDesign;

            d.FullGenerateStack();

            Vector3 preScenePos = Vector3.zero;

            if (SceneMeshIsSame())
            {
                preScenePos = _generated_sceneObjRef.transform.position;
                FGenerators.DestroyObject(_generated_sceneObjRef);
            }

            _generated_sceneObjRef = d.GeneratePrefab();
            _generated_sceneObjRef.transform.position = preScenePos;
            _generated_sceneObjParentRef = d;

            if (SceneView.lastActiveSceneView)
            {
                Selection.activeObject = _generated_sceneObjRef;
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        bool SceneMeshIsSame()
        {
            if (_generated_sceneObjRef)
                if (_generated_sceneObjParentRef == EditedDesign)
                    return true;

            return false;
        }

        #endregion



        #region Combine GUI

        bool _comb_changed = false;
        int _comb_selectedTileMesh = 0;
        int _comb_selectedMeshCopy = 0;
        bool _comb_autoRefresh = true;
        bool _comb_switchedFromComb = false;

        TileMeshSetup CombRefreshSelTileMeshRef()
        {
            if (_comb_selectedTileMesh < 0) _comb_selectedTileMesh = EditedDesign.TileMeshes.Count - 1;
            if (_comb_selectedTileMesh >= EditedDesign.TileMeshes.Count) _comb_selectedTileMesh = 0;
            return EditedDesign.TileMeshes[_comb_selectedTileMesh];
        }

        TileMeshCombineInstance _lastEditedCombInstance = null;
        GUIStyle _boldCentered = null;

        void GUICombiner()
        {
            _lastEditedCombInstance = null;

            if (_boldCentered == null || _boldCentered.normal.textColor != Color.white)
            {
                _boldCentered = new GUIStyle(EditorStyles.boldLabel);
                _boldCentered.alignment = TextAnchor.MiddleCenter;
            }

            //FGUI_Inspector.DrawUILine(0.3f, 0.5f);
            TileDesign d = EditedDesign;
            if (d.TileMeshes.Count == 0) d.TileMeshes.Add(new TileMeshSetup());

            GUILayout.Label("Tile Combine Mode", FGUI_Resources.HeaderStyle);
            GUILayout.Label("Arrange meshes, remove / join vertices", EditorStyles.centeredGreyMiniLabel);

            FGUI_Inspector.DrawUILine(0.3f, 0.5f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button(new GUIContent("  Back to Setup", FGUI_Resources.Tex_Sliders), FGUI_Resources.ButtonStyle, GUILayout.Height(18))) { EditorMode = EMode.Setup; }
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);

            TileMeshSetup t = CombRefreshSelTileMeshRef();

            //GUILayout.Space(10);
            //EditorGUILayout.LabelField(t.Name, _boldCentered);

            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 24;
            //if (GUILayout.Button(new GUIContent(" Tile Meshes (" + d.TileMeshes.Count + ")", EditorGUIUtility.IconContent("Mesh Icon").image), EditorStyles.boldLabel, GUILayout.Height(24))) _foldout_meshSetup = !_foldout_meshSetup;

            EditorGUI.BeginChangeCheck();
            _comb_selectedTileMesh = EditorGUILayout.IntPopup(new GUIContent(EditorGUIUtility.IconContent("Mesh Icon").image), _comb_selectedTileMesh, GetTMeshNameList(EditedDesign.TileMeshes), GetTMeshIDList(EditedDesign.TileMeshes), GUILayout.MinWidth(170));
            if (EditorGUI.EndChangeCheck()) { repaintRequest = true; _comb_changed = true; }
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.BeginHorizontal(GUILayout.Height(19));
            if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22))) { _comb_selectedTileMesh -= 1; repaintRequest = true; _comb_changed = true; }

            GUILayout.Label((_comb_selectedTileMesh + 1) + "/" + d.TileMeshes.Count, _boldCentered, GUILayout.Height(19));

            if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22))) { _comb_selectedTileMesh += 1; repaintRequest = true; _comb_changed = true; }
            EditorGUILayout.EndHorizontal();


            t = CombRefreshSelTileMeshRef();

            GUILayout.Space(12);
            EditorGUIUtility.labelWidth = 60;

            if (t.Copies <= 0) t.Copies = 1;
            t.Copies = EditorGUILayout.IntField("Copies: ", t.Copies, GUILayout.Width(90));
            t.AdjustCopiesCount();

            if (_comb_selectedMeshCopy < 0) _comb_selectedMeshCopy = t.Instances.Count - 1;
            if (_comb_selectedMeshCopy >= t.Instances.Count) _comb_selectedMeshCopy = 0;

            TileMeshCombineInstance inst = t.Instances[_comb_selectedMeshCopy];
            _lastEditedCombInstance = inst;

            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(4);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(18);

            EditorGUILayout.BeginHorizontal();

            if (t.Copies > 1)
                EditorGUILayout.LabelField("[" + (_comb_selectedTileMesh + 1) + "] Instance Copy (" + (_comb_selectedMeshCopy + 1) + "/" + t.Copies + ") :", GUILayout.Width(140));
            else
                EditorGUILayout.LabelField("[" + (_comb_selectedTileMesh + 1) + "] Instance Copy :", GUILayout.Width(140));
            //EditorGUILayout.LabelField("[" + (_comb_selectedTileMesh + 1) + "] Instance Copy:", GUILayout.Width(140));

            if (GUILayout.Button(FGUI_Resources.Tex_ArrowLeft, GUILayout.Width(22)))
            {
                _comb_selectedMeshCopy -= 1;

                if (_comb_selectedMeshCopy < 0)
                {
                    _comb_selectedTileMesh -= 1;
                    t = CombRefreshSelTileMeshRef();
                    _comb_selectedMeshCopy = t.Copies - 1;
                }
            }

            if (GUILayout.Button(FGUI_Resources.Tex_ArrowRight, GUILayout.Width(22)))
            {
                _comb_selectedMeshCopy += 1;

                if (_comb_selectedMeshCopy >= t.Copies)
                {
                    _comb_selectedTileMesh += 1;
                    t = CombRefreshSelTileMeshRef();
                    _comb_selectedMeshCopy = 0;
                }
            }

            _comb_selectedMeshCopy = EditorGUILayout.IntPopup(_comb_selectedMeshCopy, GetCopiesNameList(t.Instances, t.Name), GetCopiesIDList(t.Instances));


            EditorGUI.BeginChangeCheck();
            inst.Enabled = EditorGUILayout.Toggle(inst.Enabled, GUILayout.Width(16));
            if (EditorGUI.EndChangeCheck()) _comb_changed = true;


            if (GUILayout.Button(new GUIContent(" Go To " + t.Name, FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Width(140), GUILayout.Height(18)))
            {
                EditedTileSetup = t;
                _comb_switchedFromComb = true;
                EditorMode = EMode.MeshEditor;
            }

            if (GUILayout.Button(" + ", FGUI_Resources.ButtonStyle, GUILayout.Height(18), GUILayout.Width(20))) { t.Copies += 1; _comb_changed = true; t.AdjustCopiesCount(); _comb_selectedMeshCopy += 1; }

            GUI.enabled = t.Copies > 1;
            if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Height(18), GUILayout.Width(20))) { t.Copies -= 1; _comb_changed = true; t.AdjustCopiesCount(); }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);

            /*MeshMode = (EMeshMode)*/

            EditorGUI.BeginChangeCheck();

            inst.Position = EditorGUILayout.Vector3Field("Position:", inst.Position);

            EditorGUILayout.BeginHorizontal();
            inst.Rotation = EditorGUILayout.Vector3Field("Rotation:", inst.Rotation);
            if (inst.SepAxisRotMode) GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Rotation, "Separated Axis Rotation Mode Switch"), FGUI_Resources.ButtonStyle, GUILayout.Height(19), GUILayout.Width(24))) { inst.SepAxisRotMode = !inst.SepAxisRotMode; }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            inst.Scale = EditorGUILayout.Vector3Field("Scale:", inst.Scale);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            inst.MeshMode = (TileMeshSetup.TileMeshCombineInstance.EMeshMode)EditorGUILayout.EnumPopup("Mode:", inst.MeshMode);
            GUILayout.Space(14);

            if (inst.MeshMode == TileMeshCombineInstance.EMeshMode.Default)
            {
                if (/*EditedDesign.ColliderMode == TileDesign.EColliderMode.MeshColliders || */EditedDesign.ColliderMode == TileDesign.EColliderMode.CombinedMeshCollider)
                {
                    EditorGUIUtility.labelWidth = 27;
                    inst.UseInCollider = EditorGUILayout.Toggle(new GUIContent(EditorGUIUtility.IconContent("MeshCollider Icon").image, "Include this mesh as collider mesh"), inst.UseInCollider, GUILayout.Width(54));
                }
            }

            EditorGUIUtility.labelWidth = 120;
            inst.OverrideMaterial = (Material)EditorGUILayout.ObjectField("Override Material:", inst.OverrideMaterial, typeof(Material), false);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) { _comb_changed = true; }


            EditorGUILayout.EndVertical();

            FGUI_Inspector.FoldHeaderStart(ref inst.FoldoutAdvanced, " Advanced", EditorStyles.helpBox);

            if (inst.FoldoutAdvanced)
            {
                GUILayout.BeginVertical(FGUI_Resources.BGInBoxBlankStyle);
                GUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 94;
                inst.UVOffset = EditorGUILayout.Vector2Field("UVOffset:", inst.UVOffset);
                inst.UVRotate = EditorGUILayout.FloatField("UVRotate:", inst.UVRotate);
                inst.UVReScale = EditorGUILayout.Vector2Field("UV ReScale:", inst.UVReScale);
                EditorGUIUtility.labelWidth = 0;
                GUILayout.EndHorizontal();

                inst.FlipNormals = EditorGUILayout.Toggle("Flip Face/Normals:", inst.FlipNormals);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            FGUI_Inspector.DrawUILine(0.3f, 0.5f, 1, 6);

            EditorGUILayout.BeginHorizontal(FGUI_Resources.BGInBoxBlankStyle);

            EditorGUIUtility.labelWidth = 100;
            _comb_autoRefresh = EditorGUILayout.Toggle("Auto Refresh:", _comb_autoRefresh, GUILayout.Width(130));
            EditorGUIUtility.labelWidth = 0;

            GUILayout.Space(12);


            if (_comb_autoRefresh)
                if (_comb_changed)
                {
                    _comb_changed = false;
                    GenerateCombinedMesh();
                    if (_generated_sceneObjRef != null) if (SceneMeshIsSame()) GenerateSceneObject();
                }

            if (repaintRequest) TileMeshUpdatePreview();

            if (GUILayout.Button(new GUIContent("  Generate Final Mesh", FGUI_Resources.Tex_Expose), FGUI_Resources.ButtonStyle, GUILayout.Height(20)))
            {
                GenerateCombinedMesh();
                TileMeshUpdatePreview();
            }

            if (GUILayout.Button(new GUIContent("  Scene Mesh", EditorGUIUtility.IconContent("SceneAsset Icon").image), FGUI_Resources.ButtonStyle, GUILayout.Width(120), GUILayout.Height(20)))
            {
                GenerateSceneObject();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            Rect editorRect = GUILayoutUtility.GetLastRect();
            editorRect.position += new Vector2(8, 8);
            editorRect.width = position.width - 16;
            editorRect.height = position.height - (editorRect.y + 8);

            GUI.Box(editorRect, GUIContent.none, FGUI_Resources.BGInBoxStyle);

            if (generatedMesh != null)
                if (generatedMesh.vertexCount > 1)
                {
                    CombinePreviewDisplay(editorRect);
                }

            Rect infoR = editorRect;
            infoR.position += new Vector2(8, editorRect.height - 32);
            infoR.height = 32;
            DisplayGeneratedMeshInfo(EditedDesign, infoR);
        }

        static Mesh combinePreviewMesh = null;
        void CombinePreviewDisplay(Rect? rect)
        {
            RefreshCombinedMeshPreviewDisplayer();

            if (combinedMeshDisplay == null)
            {
                if (rect == null) return;
            }

            if (combinedMeshDisplay != null)
            {
                combinedMeshDisplay.selectedInstance = _lastEditedCombInstance;
                combinedMeshDisplay.UpdateMesh(EditedDesign);
                if (rect == null) return;
                combinedMeshDisplay.OnInteractivePreviewGUI(rect.Value, EditorStyles.textArea);
            }
        }

        void RefreshCombinedMeshPreviewDisplayer()
        {
            if (combinedMeshDisplay == null)
            {
                combinedMeshDisplay = (TilePreviewWindow)Editor.CreateEditor(generatedMesh, typeof(TilePreviewWindow));
            }
        }

        /// <summary> Combine Preview Mesh</summary>
        public static Mesh CPMesh
        {
            get { if (combinePreviewMesh == null) combinePreviewMesh = new Mesh(); return combinePreviewMesh; }
            set { SetMeshFromTo(value, ref combinePreviewMesh); }
        }


        static void SetMeshFromTo(Mesh from, ref Mesh to)
        {
            if (from == null) { return; }
            if (to == null) { to = new Mesh(); return; }

#if UNITY_2019_4_OR_NEWER
            to.SetVertices(from.vertices);
            to.SetNormals(from.normals);
            to.bounds = from.bounds;
            to.SetColors(from.colors);
            to.SetUVs(0, from.uv);
#else
            to.vertices=(from.vertices);
            to.normals=(from.normals);
            to.bounds = from.bounds;
            to.colors =(from.colors);
            to.uv = (from.uv);
#endif
        }

        TilePreviewWindow combinedMeshDisplay = null;

        void GenerateCombinedMesh()
        {
            EditedDesign.FullGenerateStack();

            if (EditedDesign.IsSomethingGenerated)
            {
                CombinePreviewDisplay(null);
            }
        }



        #region Copies GUI popup helper

        private int[] _CopiesIds = null;
        object _CopiesLast = null;
        public int[] GetCopiesIDList<T>(List<T> elems, bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _CopiesIds == null || _CopiesIds.Length != elems.Count)
            {
                _CopiesIds = new int[elems.Count];
                for (int i = 0; i < elems.Count; i++)
                {
                    _CopiesIds[i] = i;
                }
            }

            return _CopiesIds;
        }

        private string[] _CopiesNames = null;
        public string[] GetCopiesNameList<T>(List<T> elems, string predicate = "", bool forceRefresh = false)
        {
            if (elems != _CopiesLast)
            {
                _CopiesLast = elems;
                forceRefresh = true;
            }

            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _CopiesNames == null || _CopiesNames.Length != elems.Count)
            {
                _CopiesNames = new string[elems.Count];
                for (int i = 0; i < elems.Count; i++)
                {
                    _CopiesNames[i] = "Instance - " + predicate + " " + i;
                }
            }
            return _CopiesNames;
        }


        #endregion




        #region Tile Meshes GUI popup helper

        private int[] _TMeshIds = null;
        object _TMeshLast = null;
        public int[] GetTMeshIDList<T>(List<T> elems, bool forceRefresh = false)
        {
            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _TMeshIds == null || _TMeshIds.Length != elems.Count)
            {
                _TMeshIds = new int[elems.Count];
                for (int i = 0; i < elems.Count; i++)
                {
                    _TMeshIds[i] = i;
                }
            }

            return _TMeshIds;
        }

        private GUIContent[] _TMeshNames = null;
        public GUIContent[] GetTMeshNameList(List<TileMeshSetup> elems, string predicate = "", bool forceRefresh = false)
        {
            if (elems != _TMeshLast)
            {
                _TMeshLast = elems;
                forceRefresh = true;
            }

            if (Event.current != null) if (Event.current.type == EventType.MouseDown) forceRefresh = true;

            if (forceRefresh || _TMeshNames == null || _TMeshNames.Length != elems.Count)
            {
                _TMeshNames = new GUIContent[elems.Count];
                for (int i = 0; i < elems.Count; i++)
                {
                    _TMeshNames[i] = new GUIContent("Tile Mesh[" + (i + 1) + "]: " + elems[i].Name);
                }
            }
            return _TMeshNames;
        }


        #endregion





        #endregion



        #region Tile Mesh GUIs


        #region Lathe


        List<CurvePoint> lathe_points { get { return s._lathe_points; } }


        public void GUI_LatheTopPanel()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            width = EditorGUILayout.FloatField("Width:", width);
            GUILayout.Space(16);
            height = EditorGUILayout.FloatField("Height:", height);

            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 68;
            EditorGUI.BeginChangeCheck();
            s._lathe_fillAngle = EditorGUILayout.IntSlider("Fill Angle:", s._lathe_fillAngle, 1, 360);


            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();


            EditorGUIUtility.labelWidth = 112;
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();


            s._lathe_xSubdivCount = EditorGUILayout.IntSlider("Subdivisions (X):", s._lathe_xSubdivCount, 3, 64);

            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            s._lathe_ySubdivLimit = EditorGUILayout.Slider("Isoparm Limit (Y):", s._lathe_ySubdivLimit, 30, 1);
            s.SubdivMode = (ESubdivideCompute)EditorGUILayout.EnumPopup(s.SubdivMode, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 90;

            if (EditorGUI.EndChangeCheck()) { shapeChanged = true; }
        }

        public static List<CurvePoint> _copyCurveRef = null;
        public static List<CurvePoint> _maximizedCurve = null;
        public static float _maximizedZoom = 1f;
        public static Vector2 _maximizedOffset = Vector2.zero;

        void DrawLatheEditor(Rect editorRect, Rect displayArea)
        {
            displayArea.position = Vector2.zero;
            editorRect.position = Vector2.zero;

            Rect lRect = displayArea;
            lRect.width = displayArea.width / 2;

            DrawLatheEditorLeft(lRect);

            DrawAxisInRect(lRect, "Y", "X");


            GUI.Box(lRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);

            Rect rRect = displayArea;
            rRect.width = displayArea.width / 2;
            rRect.position += new Vector2(displayArea.width / 2, 0);

            DrawCurveOptionsButton(rRect, lathe_points);
            //if (DrawCurveButton(rRect, EditorGUIUtility.IconContent("TreeEditor.Duplicate").image)) { _copyCurveRef = lathe_points; }
            //DrawPasteCurveButton(rRect, lathe_points);

            DrawLatheEditorRight(rRect);

            Rect rMenu = editorRect;
            rMenu.position = new Vector2(editorRect.width - 240, 0);
            rMenu.width = 240;
            DrawCurvePointMenu(rMenu);

            Rect previewRect = displayArea;
            previewRect.position += new Vector2(displayArea.width * 1.175f, 0f);
            previewRect.size *= 0.7f;
            previewRect.position += new Vector2(0f, previewRect.height * 0.2f);

            DrawLathePreviewShape(previewRect);

            _latestEditorDisplayRect = _editorDisplayRect1;
            UpdateCurveInputEvents(lRect, displayArea, lathe_points);
        }



        private void DrawLathePreviewShape(Rect previewRect)
        {
            if (previewShape.Count == 0) return;


            Rect headerR = previewRect;
            headerR.height = 32;
            headerR.position -= new Vector2(0, 40);
            GUI.Label(headerR, "Subdivisions Preview", EditorStyles.centeredGreyMiniLabel);

            headerR = previewRect;
            headerR.height = 42;
            headerR.position += new Vector2(0, previewRect.height + 2);
            float totalVerts = (previewShape.Count) * (s._lathe_xSubdivCount + 2);
            float totalPoly = (previewShape.Count - 1) * s._lathe_xSubdivCount;
            GUI.Label(headerR, "Y Subdivs: " + previewShape.Count + "\nCalculated Vertices: " + totalVerts + "\nPredicted Tris: " + totalPoly * 2 + " Poly: " + totalPoly, EditorStyles.centeredGreyMiniLabel);


            GUI.BeginGroup(previewRect, FGUI_Resources.BGInBoxStyle);

            Color preH = Handles.color;
            Color preG = GUI.color;

            float width = previewRect.width / 2f;
            float height = previewRect.height;

            Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
            GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);

            DrawSubdivsPreviewLines(previewShape, width, height, true);

            Handles.color = preH;
            GUI.color = preG;
            GUI.EndGroup();
        }


        void DrawLatheEditorRight(Rect r)
        {
            GUI.Box(r, GUIContent.none, FGUI_Resources.BGInBoxStyleH);

            SetDisplayRect2(r);
            DrawCurves(r, lathe_points, true, true);
        }


        void RefreshDefaultCurve(EMeshGenerator? type = null)
        {
            if (type == null)
            {
                if (EditedTileSetup == null) return;
                type = EditedTileSetup.GenTechnique;
            }

            if (type.Value == EMeshGenerator.Lathe)
            {
                if (lathe_points.Count == 0)
                {
                    lathe_points.Add(new CurvePoint(0.5f, 0.05f, true));
                    lathe_points.Add(new CurvePoint(0.7f, 0.175f, true));
                    lathe_points.Add(new CurvePoint(0.7f, 0.8f, true));
                    lathe_points.Add(new CurvePoint(0.45f, 1f, true));
                    shapeChanged = true;
                }
            }
            else if (type.Value == EMeshGenerator.Loft)
            {
                if (_loft_depth.Count == 0)
                {
                    _loft_depth.Add(new CurvePoint(0.45f, 0f, true));
                    _loft_depth.Add(new CurvePoint(0.45f, .4f, true));
                    _loft_depth.Add(new CurvePoint(0.2f, .4f, true));
                    _loft_depth.Add(new CurvePoint(0.2f, 1f, true));
                    shapeChanged = true;
                }

                if (_loft_distribute.Count == 0)
                {
                    _loft_distribute.Add(new CurvePoint(0.0f, 0.5f, true));
                    _loft_distribute.Add(new CurvePoint(1f, 0.5f, true));
                    shapeChanged = true;
                }


                if (_loft_height.Count <= 1)
                {
                    _loft_height.Clear();
                    _loft_height.Add(new CurvePoint(0.0f, 0f, true));
                    _loft_height.Add(new CurvePoint(1f, 0f, true));
                    shapeChanged = true;
                }
            }
            else if (type.Value == EMeshGenerator.Extrude)
            {
                if (_extrude_curve.Count == 0)
                {
                    //_extrude_curve.Add(new CurvePoint(1f, .8f, true));
                    _extrude_curve.Add(new CurvePoint(0.625f, .8f, true));
                    _extrude_curve.Add(new CurvePoint(0.625f, .3f, true));
                    _extrude_curve.Add(new CurvePoint(0.7f, .215f, true));
                    _extrude_curve.Add(new CurvePoint(0.8f, .2f, true));
                    //_extrude_curve.Add(new CurvePoint(1f, .2f, true));
                    shapeChanged = true;
                }
            }
            else if (type.Value == EMeshGenerator.Sweep)
            {
                if (s._sweep_path.Count == 0)
                {
                    s._sweep_path.Add(new CurvePoint(0.0f, 1f, true));
                    s._sweep_path.Add(new CurvePoint(0.0f, .3f, true));
                    s._sweep_path.Add(new CurvePoint(0.04f, .2f, true));
                    s._sweep_path.Add(new CurvePoint(0.1f, .137f, true));
                    s._sweep_path.Add(new CurvePoint(0.167f, .095f, true));
                    s._sweep_path.Add(new CurvePoint(0.23f, .074f, true));
                    s._sweep_path.Add(new CurvePoint(0.293f, .0637f, true));
                    s._sweep_path.Add(new CurvePoint(0.367f, .0637f, true));
                    shapeChanged = true;
                }

                if (s._sweep_shape.Count == 0)
                {
                    s._sweep_shape.Add(new CurvePoint(0.3f, 0.6f, true));
                    s._sweep_shape.Add(new CurvePoint(0.3f, 0.33f, true));
                    s._sweep_shape.Add(new CurvePoint(0.33f, 0.285f, true));

                    s._sweep_shape.Add(new CurvePoint(0.67f, 0.285f, true));
                    s._sweep_shape.Add(new CurvePoint(0.7f, 0.33f, true));
                    s._sweep_shape.Add(new CurvePoint(0.7f, 0.6f, true));

                    s._sweep_shape.Add(new CurvePoint(0.67f, 0.645f, true));
                    s._sweep_shape.Add(new CurvePoint(0.33f, 0.645f, true));

                    shapeChanged = true;
                }


                if (s._sweep_radius.Count <= 1)
                {
                    s._sweep_radius.Clear();
                    s._sweep_radius.Add(new CurvePoint(0.5f, 0.0f, true));
                    s._sweep_radius.Add(new CurvePoint(0.5f, 1f, true));
                    shapeChanged = true;
                }
            }
        }


        void DrawLatheEditorLeft(Rect r)
        {
            SetDisplayRect(r);

            #region If lathe list is empty generate example shape

            if (lathe_points.Count == 0)
            {
                RefreshDefaultCurve(EMeshGenerator.Lathe);
            }

            #endregion

            GUI.Box(r, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
            DrawCurves(r, lathe_points, false, false);

            bool changed = DisplaySplineHandler(r, lathe_points, null);
            if (changed) shapeChanged = true;
        }

        #endregion



        #region Loft


        List<CurvePoint> _loft_depth { get { return s._loft_depth; } }
        List<CurvePoint> _loft_distribute { get { return s._loft_distribute; } }
        List<CurvePoint> _loft_height { get { return s._loft_height; } }



        public void GUI_LoftTopPanel()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 80;
            width = EditorGUILayout.FloatField("Distr Width:", width);
            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 60;
            height = EditorGUILayout.FloatField("Height:", height);
            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 80;
            depth = EditorGUILayout.FloatField("Distr Depth:", depth);

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 90;
            s._loftDepthCurveWidener = EditorGUILayout.FloatField("Depth Width:", s._loftDepthCurveWidener, GUILayout.Width(130));
            s._loftDepthCurveWidener = Mathf.Clamp(s._loftDepthCurveWidener, 0.25f, 3f);

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();


            EditorGUIUtility.labelWidth = 122;
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();


            s._loft_DepthSubdivLimit = EditorGUILayout.Slider("Depth Subdiv Limit:", s._loft_DepthSubdivLimit, 30, 1, GUILayout.MaxWidth(260));
            if (GenTechnique == EMeshGenerator.Lathe) s._loft_DepthSubdivLimit = Mathf.FloorToInt(s._loft_DepthSubdivLimit);

            GUILayout.Space(14);
            EditorGUIUtility.labelWidth = 60;
            s._loft_Collapse = EditorGUILayout.Slider("Collapse:", s._loft_Collapse, 0f, 1f, GUILayout.Width(190));
            GUILayout.Space(14);

            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 142;
            s._loft_DistribSubdivLimit = EditorGUILayout.Slider("Distrubute Subdiv Limit:", s._loft_DistribSubdivLimit, 30, 1);
            s.SubdivMode = (ESubdivideCompute)EditorGUILayout.EnumPopup(s.SubdivMode, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 90;

            if (EditorGUI.EndChangeCheck()) { shapeChanged = true; }

        }

        bool _loft_drawDistrOrHeight = false;
        void DrawLoftEditor(Rect editorRect, Rect displayArea)
        {
            displayArea.position = Vector2.zero;
            editorRect.position = Vector2.zero;


            #region If null lists then generating example shapes

            RefreshDefaultCurve(EMeshGenerator.Loft);

            #endregion

            Rect lRect = displayArea;
            lRect.position += new Vector2(0, displayArea.height * 0.045f);
            lRect.size *= 0.9f;
            lRect.width *= s._loftDepthCurveWidener;
            DrawLoftDepthRect(lRect);
            DrawCurves(lRect, _loft_depth, false, false);
            /*if ( lRect.Contains(mousePos))*/
            UpdateCurveInputEvents(lRect, displayArea, _loft_depth);

            Rect rRect = displayArea;
            rRect.position += new Vector2(displayArea.width * s._loftDepthCurveWidener, displayArea.height * 0.045f);
            rRect.size *= 0.9f;
            DrawLoftDistributeRect(rRect);
            List<CurvePoint> distrPlace = _loft_distribute;
            if (_loft_drawDistrOrHeight) distrPlace = _loft_height;

            DrawCurves(rRect, distrPlace, false, true);
            /*if (rRect.Contains(mousePos))*/
            UpdateCurveInputEvents(rRect, displayArea, distrPlace);

            DrawCurveOptionsButton(lRect, _loft_depth);
            DrawCurveOptionsButton(rRect, distrPlace);

            //if (DrawCurveButton(lRect, EditorGUIUtility.IconContent("TreeEditor.Duplicate").image)) { _copyCurveRef = _loft_depth; }
            //DrawPasteCurveButton(lRect, _loft_depth);

            //if (DrawCurveButton(rRect, EditorGUIUtility.IconContent("TreeEditor.Duplicate").image)) { _copyCurveRef = _loft_distribute; }
            //DrawPasteCurveButton(rRect, _loft_distribute);

            // Curve point handlers must be called in a row because of WindowsBegin()
            BeginWindows();
            _latestEditorDisplayRect = _editorDisplayRect1;
            bool changed = DisplaySplineHandler(lRect, _loft_depth, false);
            _latestEditorDisplayRect = _editorDisplayRect2;
            bool changed2 = DisplaySplineHandler(rRect, distrPlace, true);
            EndWindows();

            if (changed || changed2)
            {
                shapeChanged = true;
                repaintRequest = true;
            }

            Rect rMenu = editorRect;
            rMenu.position = new Vector2(editorRect.width - 240, 0);
            rMenu.width = 240;
            DrawCurvePointMenu(rMenu);

            Rect previewRect = displayArea;
            previewRect.position += new Vector2(displayArea.width * (1f + s._loftDepthCurveWidener), 0f);
            previewRect.size *= 0.7f;
            previewRect.position += new Vector2(0f, previewRect.height * 0.2f);

            Rect pRect = previewRect;
            pRect.width *= s._loftDepthCurveWidener;
            previewRect.position += new Vector2(pRect.width, 0);
            DrawLoftPreviewView(pRect, previewRect);
        }

        void DrawLoftDepthRect(Rect r, string title = "Depth", string axis1 = "Z", string axis2 = "Y")
        {
            SetDisplayRect(r);
            GUI.Box(r, GUIContent.none, FGUI_Resources.BGInBoxStyleH);

            DrawHeaderLabelOnRect(r, title);
            DrawAxisInRect(r, axis2, axis1);
        }

        void DrawLoftDistributeRect(Rect r)
        {
            SetDisplayRect2(r);
            GUI.Box(r, GUIContent.none, FGUI_Resources.BGInBoxStyleH);

            DrawHeaderLabelOnRect(r, _loft_drawDistrOrHeight ? "Height (Not adding subdivs)" : "Distribute");
            Rect hButton = new Rect(r);
            hButton.height = 19;
            hButton.width = 108;
            hButton.position += new Vector2(r.width - 112, -22);
            if (GUI.Button(hButton, _loft_drawDistrOrHeight ? "View Distribute" : "View Height")) { _loft_drawDistrOrHeight = !_loft_drawDistrOrHeight; }

            DrawAxisInRect(r, "Z", "X");
        }

        void DrawLoftPreviewView(Rect pRect1, Rect pRect2, bool loopSec = false)
        {
            if (previewShape.Count == 0 && previewShape2.Count == 0) return;

            Color preH = Handles.color;
            Color preG = GUI.color;


            Rect prevRect = pRect1;
            prevRect.width += pRect2.width;

            Rect headerR = prevRect;
            headerR.height = 32;
            headerR.position -= new Vector2(0, 40);
            GUI.Label(headerR, "Subdivisions Preview", EditorStyles.centeredGreyMiniLabel);

            headerR = prevRect;
            headerR.height = 42;
            headerR.position += new Vector2(0, pRect1.height + 2);

            int teoVertsC = previewShape.Count + previewShape2.Count;
            float totalVerts = (previewShape.Count * previewShape2.Count);
            float totalPoly = Mathf.Round(((previewShape.Count + 2) * (previewShape2.Count)) / 2f);
            GUI.Label(headerR, "Subdivs: " + teoVertsC + "\nCalculated Vertices: " + totalVerts + "\nPredicted Tris: " + totalPoly * 2 + " Poly: " + totalPoly, EditorStyles.centeredGreyMiniLabel);


            Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
            GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);


            GUI.BeginGroup(pRect1, FGUI_Resources.BGInBoxStyle);
            DrawSubdivsPreviewLines(previewShape, pRect1.width, pRect1.height, false);
            GUI.EndGroup();


            GUI.BeginGroup(pRect2, FGUI_Resources.BGInBoxStyle);
            DrawSubdivsPreviewLines(previewShape2, pRect2.width, pRect2.height, false, loopSec);
            GUI.EndGroup();

            Handles.color = preH;
            GUI.color = preG;
        }



        #endregion



        #region Extrude

        List<CurvePoint> _extrude_curve { get { return s._extrude_curve; } }


        public void GUI_ExtrudeTopPanel()
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60;
            width = EditorGUILayout.FloatField("Width:", width);
            GUILayout.Space(16);
            height = EditorGUILayout.FloatField("Height:", height);

            GUILayout.Space(16);
            s.depth = EditorGUILayout.FloatField("Depth:", s.depth);

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 62;
            s._extrudeMirror = EditorGUILayout.Toggle("Mirror:", s._extrudeMirror, GUILayout.Width(102));
            EditorGUIUtility.labelWidth = 72;
            s._extrudeFrontCap = EditorGUILayout.Toggle("Front Cap:", s._extrudeFrontCap, GUILayout.Width(102));
            EditorGUIUtility.labelWidth = 42;
            s._extrudeBackCap = EditorGUILayout.Toggle("Back:", s._extrudeBackCap, GUILayout.Width(82));
            EditorGUIUtility.labelWidth = 102;


            GUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            s._extrude_SubdivLimit = EditorGUILayout.Slider("Isoparm Limit(Y):", s._extrude_SubdivLimit, 30, 1);
            s.SubdivMode = (ESubdivideCompute)EditorGUILayout.EnumPopup(s.SubdivMode, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 90;
            if (EditorGUI.EndChangeCheck()) { shapeChanged = true; }
        }


        void DrawExtrudeEditor(Rect editorRect, Rect displayArea)
        {
            displayArea.position = Vector2.zero;
            editorRect.position = Vector2.zero;

            #region If null lists then generating example shapes

            if (_extrude_curve.Count == 0)
            {
                RefreshDefaultCurve(EMeshGenerator.Extrude);
            }

            #endregion

            Rect lRect = displayArea;
            if (s._extrudeMirror) lRect.width = displayArea.width / 2;

            GUI.Box(displayArea, GUIContent.none, FGUI_Resources.BGInBoxStyleH);
            //DrawHeaderLabelOnRect(displayArea, "Symmetry Shape");

            DrawAxisInRect(lRect, "Y", "X");
            SetDisplayRect(lRect);
            if (s._extrudeMirror) GUI.Box(lRect, GUIContent.none, FGUI_Resources.BGInBoxStyleH);

            DrawCurves(lRect, _extrude_curve, false, false, !s._extrudeMirror);
            UpdateCurveInputEvents(lRect, displayArea, _extrude_curve);

            DrawCurveOptionsButton(lRect, _extrude_curve);
            //if (DrawCurveButton(lRect, EditorGUIUtility.IconContent("TreeEditor.Duplicate").image)) { _copyCurveRef = _extrude_curve; }
            //DrawPasteCurveButton(lRect, _extrude_curve);

            Rect rRect = lRect;
            rRect.position += new Vector2(rRect.width, 0);
            if (s._extrudeMirror) DrawCurves(rRect, _extrude_curve, true, false);

            bool changed = DisplaySplineHandler(lRect, _extrude_curve, null);

            if (changed)
            {
                shapeChanged = true;
                repaintRequest = true;
            }

            Rect rMenu = editorRect;
            rMenu.position = new Vector2(editorRect.width - 240, 0);
            rMenu.width = 240;
            DrawCurvePointMenu(rMenu);

            Rect previewRect = displayArea;
            previewRect.position += new Vector2(displayArea.width * 1.175f, 0f);
            previewRect.size *= 0.7f;
            previewRect.position += new Vector2(0f, previewRect.height * 0.2f);

            DrawExtrudePreviewShape(previewRect);
        }


        private void DrawExtrudePreviewShape(Rect previewRect)
        {
            if (_extrude_curve.Count == 0) return;


            Rect headerR = previewRect;
            headerR.height = 32;
            headerR.position -= new Vector2(0, 40);
            GUI.Label(headerR, "Subdivisions Preview", EditorStyles.centeredGreyMiniLabel);

            headerR = previewRect;
            headerR.height = 42;
            headerR.position += new Vector2(0, previewRect.height + 2);
            float totalPoly = (previewShape.Count - 2) * 4 - 2;
            if (!s._extrudeFrontCap) totalPoly -= (previewShape.Count - 2);
            if (!s._extrudeBackCap) totalPoly -= (previewShape.Count - 2);
            float totalVerts = (previewShape.Count - 2) * 4;
            GUI.Label(headerR, "Vertices: " + totalVerts + "\nPredicted Tris: " + totalPoly * 2 + " Poly: " + totalPoly, EditorStyles.centeredGreyMiniLabel);


            GUI.BeginGroup(previewRect, FGUI_Resources.BGInBoxStyle);

            Color preH = Handles.color;
            Color preG = GUI.color;

            float width = previewRect.width;
            if (s._extrudeMirror) width /= 2f;
            float height = previewRect.height;

            Handles.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
            GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);

            DrawSubdivsPreviewLines(previewShape, width, height, s._extrudeMirror, !s._extrudeMirror);

            Handles.color = preH;
            GUI.color = preG;
            GUI.EndGroup();
        }






        #endregion







        #region Sweep

        public void GUI_SweepTopPanel()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 72;
            width = EditorGUILayout.FloatField("Path Width:", width);
            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 76;
            height = EditorGUILayout.FloatField("Path Height:", height);
            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 70;
            depth = EditorGUILayout.FloatField("Radius:", depth);
            if (depth < 0.01f) depth = 0.01f;
            EditorGUILayout.EndHorizontal();


            EditorGUIUtility.labelWidth = 122;
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();

            s._sweep_distribSubdivLimit = EditorGUILayout.Slider("Depth Subdiv Limit:", s._sweep_distribSubdivLimit, 30, 1, GUILayout.MaxWidth(260));
            s._sweep_distribSubdivLimit = Mathf.FloorToInt(s._sweep_distribSubdivLimit);


            GUILayout.Space(16);
            EditorGUIUtility.labelWidth = 99;
            s._sweep_Close = EditorGUILayout.Toggle("Close Shape:", s._sweep_Close, GUILayout.MaxWidth(140));

            EditorGUIUtility.labelWidth = 134;
            GUILayout.Space(6);
            s._sweep_shapeSubdivLimit = EditorGUILayout.Slider("Radius Subdiv Limit:", s._sweep_shapeSubdivLimit, 30, 1);
            s.SubdivMode = (ESubdivideCompute)EditorGUILayout.EnumPopup(s.SubdivMode, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck()) { shapeChanged = true; }

        }

        bool _sweep_drawShapeOrRadius = false;
        public void DrawSweepEditor(Rect editorRect, Rect displayArea)
        {

            displayArea.position = Vector2.zero;
            editorRect.position = Vector2.zero;


            #region If null lists then generating example shapes

            RefreshDefaultCurve(EMeshGenerator.Sweep);

            #endregion

            Rect lRect = displayArea;
            lRect.position += new Vector2(0, displayArea.height * 0.045f);
            lRect.size *= 0.9f;
            DrawLoftDepthRect(lRect, "Path", "X");
            DrawCurves(lRect, s._sweep_path, false, false);
            UpdateCurveInputEvents(lRect, displayArea, s._sweep_path);

            Rect rRect = displayArea;
            rRect.position += new Vector2(displayArea.width, displayArea.height * 0.045f);
            rRect.size *= 0.9f;
            rRect.width = rRect.height;
            DrawSweepRadiusRect(rRect);
            List<CurvePoint> sweepShapePlace = s._sweep_shape;
            if (_sweep_drawShapeOrRadius) sweepShapePlace = s._sweep_radius;

            DrawCurves(rRect, sweepShapePlace, false, true, _sweep_drawShapeOrRadius == false && s._sweep_Close);
            UpdateCurveInputEvents(rRect, displayArea, sweepShapePlace);

            DrawCurveOptionsButton(lRect, s._sweep_shape);
            DrawCurveOptionsButton(rRect, sweepShapePlace);

            // Curve point handlers must be called in a row because of WindowsBegin()
            BeginWindows();
            _latestEditorDisplayRect = _editorDisplayRect1;
            bool changed = DisplaySplineHandler(lRect, s._sweep_path, false);
            _latestEditorDisplayRect = _editorDisplayRect2;
            bool changed2 = DisplaySplineHandler(rRect, sweepShapePlace, true);
            EndWindows();

            if (changed || changed2)
            {
                shapeChanged = true;
                repaintRequest = true;
            }

            Rect rMenu = editorRect;
            rMenu.position = new Vector2(editorRect.width - 240, 0);
            rMenu.width = 240;
            DrawCurvePointMenu(rMenu);

            Rect previewRect = displayArea;
            previewRect.position += new Vector2(lRect.width + rRect.width + 90, 0f);
            previewRect.size *= 0.7f;
            previewRect.position += new Vector2(0f, previewRect.height * 0.2f);

            Rect pRect = previewRect;
            previewRect.position += new Vector2(pRect.width, 0);
            previewRect.width = previewRect.height;
            DrawLoftPreviewView(pRect, previewRect, s._sweep_Close);

        }

        void DrawSweepRadiusRect(Rect r)
        {
            SetDisplayRect2(r);
            GUI.Box(r, GUIContent.none, FGUI_Resources.BGInBoxStyleH);

            DrawHeaderLabelOnRect(r, _sweep_drawShapeOrRadius ? "Radius over path length" : "Shape");
            Rect hButton = new Rect(r);
            hButton.height = 19;
            hButton.width = 108;
            hButton.position += new Vector2(r.width - 112, -22);
            if (GUI.Button(hButton, _sweep_drawShapeOrRadius ? "View Shape" : "View Radius")) { _sweep_drawShapeOrRadius = !_sweep_drawShapeOrRadius; }

            if (!_sweep_drawShapeOrRadius)
                DrawAxisInRect(r, "Z", "X");
            else
                DrawAxisInRect(r, "L", "R");
        }


        #endregion




        #region Custom Mesh

        public void GUI_CustomMeshTopPanel()
        {
            GUILayout.Space(8);

            EditedTileSetup.ExtraMesh = (EExtraMesh)EditorGUILayout.EnumPopup("Type: ", EditedTileSetup.ExtraMesh);
            if (EditedTileSetup.ExtraMesh == EExtraMesh.CustomMesh)
            {
                EditorGUILayout.HelpBox("Use custom mesh or other tile mesh to join it with the tile design in the 'Combiner' menu.", MessageType.Info);

                GUILayout.Space(8);

                EditedTileSetup.CustomMesh = (Mesh)EditorGUILayout.ObjectField("Custom Mesh:", EditedTileSetup.CustomMesh, typeof(Mesh), false);
                //GUILayout.Space(4);
                //EditedTileSetup.Material = (Material)EditorGUILayout.ObjectField("Material:", EditedTileSetup.Material, typeof(Material), false);
                GUILayout.Space(8);
                EditorGUIUtility.labelWidth = 200;
                EditedTileSetup._customMeshOverwriteVertexColor = EditorGUILayout.Toggle("Overwrite Vertex Color:", EditedTileSetup._customMeshOverwriteVertexColor);
                EditorGUIUtility.labelWidth = 0;

                if (EditedTileSetup._customMeshOverwriteVertexColor)
                {
                    EditorGUI.indentLevel++;
                    EditedTileSetup._customMeshOverwriteVertexColorValues = EditorGUILayout.ColorField("Target Vertex Color:", EditedTileSetup._customMeshOverwriteVertexColorValues);
                    EditorGUI.indentLevel--;
                }
            }
            else if (EditedTileSetup.ExtraMesh == EExtraMesh.CableGenerator)
            {
                //GUILayout.Space(8);
                //EditorGUILayout.HelpBox("Use cable generator algorithm to create mesh.", MessageType.Info);
                GUILayout.Space(8);

                EditorGUI.BeginChangeCheck();

                if (s._CablePoints == null || s._CablePoints.Count < 2)
                {
                    s._CablePoints = new List<Vector3>();
                    s._CablePoints.Add(new Vector3(0f, 0f, -0.5f));
                    s._CablePoints.Add(new Vector3(0f, 0f, 0.5f));
                }

                if (s._CableRadius <= 0.001f) s._CableRadius = 0.02f;

                EditorGUIUtility.labelWidth = 60;
                EditorGUILayout.BeginHorizontal();
                s._CableRadius = EditorGUILayout.FloatField("Radius:", s._CableRadius);
                GUILayout.Space(8);
                s._CableLoose = EditorGUILayout.FloatField("Loose:", s._CableLoose);
                GUILayout.Space(8);
                s._CableHanging = EditorGUILayout.Slider("Hanging:", s._CableHanging, 0f, 1f);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(6);

                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.BeginHorizontal();
                s._CableView = (ECableView)EditorGUILayout.EnumPopup("Display Settings For: ", s._CableView);
                if (s._CableView == ECableView.CablePoints) if (GUILayout.Button("+", GUILayout.Width(22))) { s._CablePoints.Add(s._CablePoints[s._CablePoints.Count - 1] + new Vector3(0f, 0f, 0.25f)); }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(6);

                if (s._CableMeshSettings == null) s._CableMeshSettings = new TileCableGenerator.CableMeshSettings();
                if (s._CableTexturingSettings == null) s._CableTexturingSettings = new TileCableGenerator.CableTexturingSettings();
                if (s._CableClonerSettings == null) s._CableClonerSettings = new TileCableGenerator.CableClonerSettings();
                if (s._CableRandomizationSettings == null) s._CableRandomizationSettings = new TileCableGenerator.CableRandomizationSettings();


                if (s._CableView == ECableView.Mesh)
                {
                    s._CableMeshSettings.LengthSubdivs = EditorGUILayout.IntSlider("Length Subdivs: ", s._CableMeshSettings.LengthSubdivs, 6, 36);
                    s._CableMeshSettings.CircleSubdivs = EditorGUILayout.IntSlider("Circle Subdivs: ", s._CableMeshSettings.CircleSubdivs, 3, 16);
                    s._CableMeshSettings.RollOffset = EditorGUILayout.Slider("Roll Offset: ", s._CableMeshSettings.RollOffset, 0, 360);
                    s._CableMeshSettings.JoinEnds = EditorGUILayout.Toggle("Join Point Ends: ", s._CableMeshSettings.JoinEnds);
                }
                else if (s._CableView == ECableView.Texturing)
                {
                    s._CableTexturingSettings.LengthTiling = EditorGUILayout.FloatField("Length Tiling: ", s._CableTexturingSettings.LengthTiling);
                    s._CableTexturingSettings.VerticalTiling = EditorGUILayout.FloatField("Vertical Tiling: ", s._CableTexturingSettings.VerticalTiling);
                    s._CableTexturingSettings.UVRotate = EditorGUILayout.Slider("UV Rotate: ", s._CableTexturingSettings.UVRotate, 0, 360);
                }
                else if (s._CableView == ECableView.Cloner)
                {
                    s._CableClonerSettings.InstancesCount = EditorGUILayout.Vector3IntField("Instances Count: ", s._CableClonerSettings.InstancesCount);
                    s._CableClonerSettings.ClonesOffsets = EditorGUILayout.Vector3Field("Clones Offsets: ", s._CableClonerSettings.ClonesOffsets);
                    s._CableClonerSettings.ScaleOffsets = EditorGUILayout.Slider("Scale Offsets: ", s._CableClonerSettings.ScaleOffsets, 0f, 2f);
                    s._CableClonerSettings.CircularGrid = EditorGUILayout.Toggle("Circular Grid: ", s._CableClonerSettings.CircularGrid);
                }
                else if (s._CableView == ECableView.Randomization)
                {
                    s._CableRandomizationSettings.RandomizeTrails = EditorGUILayout.Vector2Field("Randomize Trails: ", s._CableRandomizationSettings.RandomizeTrails);
                    s._CableRandomizationSettings.NoiseScale = EditorGUILayout.FloatField("Noise Scale: ", s._CableRandomizationSettings.NoiseScale);
                    s._CableRandomizationSettings.RandomizeLoose = EditorGUILayout.Vector2Field("Randomize Loose: ", s._CableRandomizationSettings.RandomizeLoose);
                }
                else if (s._CableView == ECableView.CablePoints)
                {
                    int toRemove = -1;
                    for (int p = 0; p < s._CablePoints.Count; p++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        s._CablePoints[p] = EditorGUILayout.Vector3Field("[" + p + "]: ", s._CablePoints[p]);
                        if (s._CablePoints.Count > 2) if (GUILayout.Button(FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Height(20), GUILayout.Width(22))) { toRemove = p; }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (toRemove != -1) s._CablePoints.RemoveAt(toRemove);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    shapeChanged = true;
                    repaintRequest = true;
                    _SetDirty();
                }

                GUILayout.Space(8);
            }

            GUILayout.Space(5);
            EditedTileSetup.Origin = (EOrigin)EditorGUILayout.EnumPopup("Origin:", s.Origin);
            GUILayout.Space(8);
        }

        public void GUI_PrimitiveTopPanel()
        {
            GUILayout.Space(8);
            EditorGUILayout.HelpBox("Generate primitive shape to adjust it later in combine stage.", MessageType.Info);
            GUILayout.Space(8);

            EditedTileSetup._primitive_Type = (EPrimitiveType)EditorGUILayout.EnumPopup("Primitive Type:", EditedTileSetup._primitive_Type);
            //EditedTileSetup.Material = (Material)EditorGUILayout.ObjectField("Material:", EditedTileSetup.Material, typeof(Material), false);
            GUILayout.Space(2);

            #region Vertex Color

            EditorGUIUtility.labelWidth = 200;
            EditedTileSetup._customMeshOverwriteVertexColor = EditorGUILayout.Toggle("Enable Vertex Color:", EditedTileSetup._customMeshOverwriteVertexColor);
            EditorGUIUtility.labelWidth = 0;

            if (EditedTileSetup._customMeshOverwriteVertexColor)
            {
                EditorGUI.indentLevel++;
                EditedTileSetup._customMeshOverwriteVertexColorValues = EditorGUILayout.ColorField("Set Vertex Color:", EditedTileSetup._customMeshOverwriteVertexColorValues);
                EditorGUI.indentLevel--;
            }

            #endregion

            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            EditedTileSetup._tryWeldVertices = EditorGUILayout.Toggle("Try Weld Vertices:", EditedTileSetup._tryWeldVertices);

            if (EditedTileSetup._tryWeldVertices)
            {
                GUILayout.Space(14);
                EditorGUIUtility.labelWidth = 28;
                EditedTileSetup._tryWeldVerticesV2 = EditorGUILayout.Toggle("V2:", EditedTileSetup._tryWeldVerticesV2);
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
            EditedTileSetup._primitive_scale = EditorGUILayout.Vector3Field("Scale Primitive:", EditedTileSetup._primitive_scale);
            GUILayout.Space(7);

            #region Cube

            if (EditedTileSetup._primitive_Type == EPrimitiveType.Cube)
            {
                #region Faces

                GUILayout.Space(9);
                EditorGUILayout.LabelField("Box Faces to Generate", EditorStyles.centeredGreyMiniLabel);
                int separ = 8;
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 28;
                EditedTileSetup._primitive_cube_topFace = EditorGUILayout.Toggle("Top:", EditedTileSetup._primitive_cube_topFace, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                GUILayout.Space(separ);
                EditorGUIUtility.labelWidth = 48;
                EditedTileSetup._primitive_cube_bottomFace = EditorGUILayout.Toggle("Bottom:", EditedTileSetup._primitive_cube_bottomFace, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                GUILayout.Space(separ);
                EditorGUIUtility.labelWidth = 34;
                EditedTileSetup._primitive_cube_leftFace = EditorGUILayout.Toggle("Left:", EditedTileSetup._primitive_cube_leftFace, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                GUILayout.Space(separ);
                EditorGUIUtility.labelWidth = 36;
                EditedTileSetup._primitive_cube_rightFace = EditorGUILayout.Toggle("Right:", EditedTileSetup._primitive_cube_rightFace, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                GUILayout.Space(separ);
                EditorGUIUtility.labelWidth = 40;
                EditedTileSetup._primitive_cube_frontFace = EditorGUILayout.Toggle("Front:", EditedTileSetup._primitive_cube_frontFace, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                GUILayout.Space(separ);
                EditorGUIUtility.labelWidth = 38;
                EditedTileSetup._primitive_cube_backFace = EditorGUILayout.Toggle("Back:", EditedTileSetup._primitive_cube_backFace, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(5);

                #endregion

                GUILayout.Space(12);
                EditedTileSetup._primitive_plane_subdivs = EditorGUILayout.Vector3IntField("Subdivisions:", EditedTileSetup._primitive_plane_subdivs);
                EditedTileSetup._Primitive_Cube_ClampParams();
                GUILayout.Space(3);
                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 92;
                EditedTileSetup._primitive_cube_bevel = EditorGUILayout.Slider("Bevel:", EditedTileSetup._primitive_cube_bevel, 0f, 0.5f);
                //EditedTileSetup._primitive_cube_bevel = EditorGUILayout.FloatField("Bevel Size:", EditedTileSetup._primitive_cube_bevel);
                //GUILayout.Space(14);
                //EditedTileSetup._primitive_cube_bevelSubdivs = EditorGUILayout.IntField("Bevel Subdivs:", EditedTileSetup._primitive_cube_bevelSubdivs);
                EditorGUIUtility.labelWidth = 0;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(7);
            }
            else if (EditedTileSetup._primitive_Type == EPrimitiveType.Plane)
            {
                Vector2Int subs = new Vector2Int(EditedTileSetup._primitive_plane_subdivs.x, EditedTileSetup._primitive_plane_subdivs.y);
                subs = EditorGUILayout.Vector2IntField("Subdivisions:", subs);
                EditedTileSetup._primitive_plane_subdivs = new Vector3Int(subs.x, subs.y, 1);

                EditedTileSetup._Primitive_Plane_ClampParams();
            }
            else if (EditedTileSetup._primitive_Type == EPrimitiveType.Sphere)
            {
                EditorGUILayout.HelpBox("In the current version there is only default Unity Sphere generating possible", MessageType.Info);
            }
            else if (EditedTileSetup._primitive_Type == EPrimitiveType.Cylinder)
            {
                EditorGUILayout.HelpBox("In the current version there is only default Unity Cylinder generating possible", MessageType.Info);
            }

            #endregion


            GUILayout.Space(7);
            EditorGUIUtility.labelWidth = 220;
            EditedTileSetup._randomizeVerticesOffset = EditorGUILayout.Vector3Field("Randomize Vertices Filter:", EditedTileSetup._randomizeVerticesOffset);
            if (EditedTileSetup._tryWeldVertices == false) if (EditedTileSetup._randomizeVerticesOffset != Vector3.zero) { EditorGUILayout.HelpBox("Use 'Try Weld Vertices' to keep primitive in shape", MessageType.None); }
            EditedTileSetup._randomizeVerticesNoiseScale = EditorGUILayout.Vector2Field("Filter Noise Scale:", EditedTileSetup._randomizeVerticesNoiseScale);
            EditorGUIUtility.labelWidth = 0;
            GUILayout.Space(7);
        }

        void DrawCustomMeshEditor(Rect editorRect, Rect displayArea)
        {
        }

        void DrawPrimitiveEditor(Rect editorRect, Rect displayArea)
        {
        }

        #endregion


        #endregion



        #region GUI Helpers



        void DrawAxisInRect(Rect r, string up, string right)
        {
            #region Axis display

            Vector2 axisOrig = new Vector2(r.x + 12, r.y + r.height - 23f);
            Rect axisR = r;
            axisR.position = axisOrig + new Vector2(12, 4);
            axisR.size = new Vector2(20, 20);
            GUI.Label(axisR, right, EditorStyles.centeredGreyMiniLabel);

            axisR = r;
            axisR.position = axisOrig + new Vector2(-10, -20);
            axisR.size = new Vector2(20, 20);
            GUI.Label(axisR, up, EditorStyles.centeredGreyMiniLabel);

            Color preH = Handles.color;
            Handles.color = _curveCol * 0.8f;
            axisOrig -= new Vector2(2, -15);
            Handles.DrawLine(axisOrig, axisOrig + new Vector2(15, 0));
            Handles.DrawLine(axisOrig, axisOrig + new Vector2(0, -15));
            Handles.color = preH;

            #endregion
        }


        bool DrawCurveButton(Rect r, Texture icon, string tooltip = "", float rOffset = 0f)
        {
            Vector2 buttOrig = new Vector2(r.x + r.width - 24 - rOffset, r.y + r.height - 27f);
            Rect buttonR = r;
            buttonR.position = buttOrig;
            buttonR.size = new Vector2(20, 20);
            if (GUI.Button(buttonR, new GUIContent(icon, tooltip), FGUI_Resources.ButtonStyle)) { return true; }
            return false;
        }

        void DrawCurveOptionsButton(Rect r, List<CurvePoint> curve, float rOffset = 0f)
        {
            Vector2 buttOrig = new Vector2(r.x + r.width - 24 - rOffset, r.y + r.height - 23f);
            Rect buttonR = r;
            buttonR.position = buttOrig;
            buttonR.size = new Vector2(16, 16);

            if (GUI.Button(buttonR, FGUI_Resources.GUIC_More, EditorStyles.label))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Maximize Curve Editor"), false, () => { _maximizedCurve = curve; _maximizedZoom = 1f; _maximizedOffset = Vector2.zero; });
                menu.AddItem(new GUIContent("Copy Curve"), false, () => { _copyCurveRef = curve; });

                if (_copyCurveRef != null && _copyCurveRef != curve)
                {
                    menu.AddItem(GUIContent.none, false, () => { });
                    menu.AddItem(new GUIContent("Paste Curve"), false, () =>
                    {
                        CurvePoint.CopyListFromTo(_copyCurveRef, curve);
                    });
                }

                menu.AddItem(GUIContent.none, false, () => { });
                menu.AddItem(new GUIContent("Clear Curve"), false, () => { curve.Clear(); });

                menu.ShowAsContext();
            }
        }


        void DrawHeaderLabelOnRect(Rect r, string label)
        {
            Rect labelR = r;
            labelR.height = 24;
            labelR.position -= new Vector2(0, 23);
            GUI.Label(labelR, label, EditorStyles.centeredGreyMiniLabel);
        }



        #endregion


    }
}
#endif