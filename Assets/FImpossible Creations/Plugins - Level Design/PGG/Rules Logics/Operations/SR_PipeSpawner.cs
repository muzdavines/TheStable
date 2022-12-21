using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.Rules.Operations
{
    public class SR_PipeSpawner : SpawnRuleBase, ISpawnProcedureType
    {
        public override string TitleName() { return "Pipe Spawner"; }
        public override string Tooltip() { return "Spawning 'Pipe Generator' preset without need of creating prefab with 'PipeGenerator' component\n" + base.Tooltip(); }
        public override bool CanBeGlobal() { return false; }
        public override bool CanBeNegated() { return false; }

        public EProcedureType Type { get { return EProcedureType.Coded; } }

        public PipePreset PipePreset;

        [Tooltip("Pipe can't finish in exact position of 'Desired Ending' point, but you can determinate how far from this point pipe ending process can start")]
        public float MaxDistanceToEnding = 2f;
        [Tooltip("How many times algorithm can iterate to find right path towards desired position")]
        public int MaxTries = 128;

        [Space(2)]
        [Tooltip("If you want to use raycasting and run finish-align process with additional segments")]
        public bool AlignFinish = true;
        [Tooltip("Collision mask for finding end align point")]
        public LayerMask AlignFinishOptionalsOn = ~(0 << 0);

        [Space(4)]
        [Tooltip("Not allowing to generate pipe segments when couldn't find path towards target position, if untoggled pipe will be generate lastest unfinished segments-path")]
        public bool DontGenerateIfNotEnded = true;
        [Tooltip("If first iteration segments should have disabled collision checking, useful when you start pipe from inside of some collider")]
        public int FirstSegmentsWithoutCollision = 1;
        [Tooltip("Squasing / scaling down last segments to not go through walls when segment model is too long in it's size")]
        public bool AlignScaleForFinishingSegments = true;


        [FPD_Header("Align Start Properties")]
        [Tooltip("Raycast collision mask for start alignment search")]
        public LayerMask AlignStartOn = ~(0 << 0);
        [Tooltip("How far finish collision point can be away from game object's position")]
        public float AlignStartMaxDistance = 2.5f;
        [Tooltip("You can choose in which directions start align point should be search")]
        public Vector3[] AlignStartDirections = new Vector3[] { Vector3.right, -Vector3.right, Vector3.up, Vector3.back };


        [FPD_Header("Raycasting Collision")]
        [Tooltip("Collision mask for detecting obstacles in segments-path way")]
        public LayerMask ObstaclesMask = ~(0 << 0);
        [Tooltip("Box cast scale for detecting obstacles, you can preview it's size with scene gizmos")]
        public float BoxcastScale = 0f;
        [Tooltip("Using bounding box object's mesh collision to avoid self collision when generating segments'path")]
        [Range(0.0f, 1f)] public float SelfCollisionScale = 0.75f;

        [FPD_Header("Extra Conditions")]
        [Tooltip("Using this collision mask you can define if pipe segments-path should be near for example to walls, then it will try finding path which is near to wall instead of pipe haning in air")]
        public LayerMask HoldMask = (1 << 0);
        [Tooltip("You can choose directions on which pipe segments should hold on")]
        public Vector3[] HoldDirections = new Vector3[] { Vector3.right, -Vector3.right, Vector3.up, -Vector3.up, Vector3.forward, -Vector3.forward };
        [Tooltip("How far segments can be to hold mask collision check, beware to not set it too low")]
        public float MinimalDistanceToHoldMask = 1.5f;

        [FPD_Header("Random Find Desired Position ON START")]
        public LayerMask RFindMask = ~(0 << 0);
        public Vector3[] RFindDirections = new Vector3[] { Vector3.right, -Vector3.right, Vector3.up, -Vector3.up, Vector3.forward, -Vector3.forward };
        public bool WorldSpaceRFindDirs = true;
        public float RFindMinimumDistance = 5f;
        public float RFindMaxDistance = 25f;
        public bool FlattendRFindNormal = true;
        [Range(1, 32)] public int RFindTries = 16;
        [Range(1, 24)] public int RFindSteps = 14;


        [Space(6)]
        public bool Debug = false;


        #region Editor

#if UNITY_EDITOR
        private PipeGenerator.EEditorState _editorState = PipeGenerator.EEditorState.Setup;

        public override bool EditorDrawPublicProperties()
        {
            return false;
        }

        SerializedProperty sp_prst;
        SerializedProperty sp_setp;
        SerializedProperty sp_twek;
        SerializedProperty sp_extr;
        SerializedProperty sp_debug;

        public override void NodeBody(SerializedObject so)
        {
            EditorGUILayout.HelpBox("To see result in preview, remember to enable 'Run Additional Generators' in 'Test Generating Settings'!", MessageType.None);
            EditorGUILayout.HelpBox("Pipe Generator requires scene/interior with colliders!", MessageType.None);
            GUILayout.Space(4);

            if (sp_prst == null) sp_prst = so.FindProperty("PipePreset");
            if (sp_setp == null) sp_setp = so.FindProperty("MaxDistanceToEnding");
            if (sp_twek == null) sp_twek = so.FindProperty("AlignStartOn");
            if (sp_extr == null) sp_extr = so.FindProperty("HoldMask");
            if (sp_debug == null) sp_debug = so.FindProperty("Debug");

            if (PipePreset == null || sp_prst.objectReferenceValue == null)
            {
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(sp_prst);
                GUILayout.Space(4);
                EditorGUILayout.HelpBox("Pipe Preset needed for more options!\nYou can prepare Pipe Preset with 'Pipe Generator' component", MessageType.Warning);
            }
            else
            {

                EditorGUILayout.BeginHorizontal();
                Color bc = GUI.backgroundColor;

                if (_editorState == PipeGenerator.EEditorState.Setup) GUI.backgroundColor = Color.green; if (GUILayout.Button(new GUIContent(" Setup", FGUI_Resources.Tex_GearSetup), FGUI_Resources.ButtonStyle, GUILayout.Height(24))) _editorState = PipeGenerator.EEditorState.Setup; GUI.backgroundColor = bc;
                if (_editorState == PipeGenerator.EEditorState.Adjust) GUI.backgroundColor = Color.green; if (GUILayout.Button(new GUIContent(" Tweak", FGUI_Resources.Tex_Sliders), FGUI_Resources.ButtonStyle, GUILayout.Height(24))) _editorState = PipeGenerator.EEditorState.Adjust; GUI.backgroundColor = bc;
                if (_editorState == PipeGenerator.EEditorState.Extra) GUI.backgroundColor = Color.green; if (GUILayout.Button(new GUIContent(" Extra", FGUI_Resources.Tex_Tweaks), FGUI_Resources.ButtonStyle, GUILayout.Height(24))) _editorState = PipeGenerator.EEditorState.Extra; GUI.backgroundColor = bc;

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(6);

                if (_editorState == PipeGenerator.EEditorState.Setup)
                {
                    SerializedProperty sp = sp_prst.Copy();
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp);
                    GUILayout.Space(4);

                }
                else if (_editorState == PipeGenerator.EEditorState.Adjust)
                {
                    SerializedProperty sp = sp_twek.Copy();
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp);
                    GUILayout.Space(4);
                }
                else if (_editorState == PipeGenerator.EEditorState.Extra)
                {
                    SerializedProperty sp = sp_extr.Copy();
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp); sp.NextVisible(false);
                    EditorGUILayout.PropertyField(sp);
                    GUILayout.Space(4);
                }
            }


            FGUI_Inspector.DrawUILine(0.6f, 0.33f, 1, 6);
            base.NodeBody(so);
            sp_prst.serializedObject.ApplyModifiedProperties();
        }


#endif

        #endregion

        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            base.CheckRuleOn(mod, ref spawn, preset, cell, grid, restrictDirection);
            CellAllow = true;
        }

        public override void CellInfluence(FieldSetup preset, FieldModification mod, FieldCell cell, ref SpawnData spawn, FGenGraph<FieldCell, FGenPoint> grid, Vector3? restrictDirection = null)
        {
            _EditorDebug = Debug;
        }

        public override void OnConditionsMetAction(FieldModification mod, ref SpawnData thisSpawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            SpawnData spwn = thisSpawn;
            spwn.DontSpawnMainPrefab = true;

            Action<GameObject> pipesSpawn =
            (o) =>
            {
                Matrix4x4 mx = GetMatrix(spwn);

                GameObject spawner = new GameObject("Pipe-Spawner");
                spawner.transform.position = mx.MultiplyPoint(Vector3.zero);
                spawner.transform.rotation = mx.rotation;

                var pp = spawner.AddComponent<PipeGenerator>();
                pp.SetPreset(PipePreset);

                pp.MaxDistanceToEnding = MaxDistanceToEnding;
                pp.MaxTries = MaxTries;

                pp.AlignFinish = AlignFinish;
                pp.AlignFinishOptionalsOn = AlignFinishOptionalsOn;

                pp.DontGenerateIfNotEnded = DontGenerateIfNotEnded;
                pp.FirstSegmentsWithoutCollision = FirstSegmentsWithoutCollision;
                pp.AlignScaleForFinishingSegments = AlignScaleForFinishingSegments;

                pp.AlignStartOn = AlignStartOn;
                pp.AlignStartMaxDistance = AlignStartMaxDistance;
                pp.AlignStartDirections = AlignStartDirections;

                pp.ObstaclesMask = ObstaclesMask;
                pp.BoxcastScale = BoxcastScale;
                pp.SelfCollisionScale = SelfCollisionScale;

                pp.HoldMask = HoldMask;
                pp.HoldDirections = HoldDirections;
                pp.MinimalDistanceToHoldMask = MinimalDistanceToHoldMask;

                pp.RFindSeed = FGenerators.GetRandom(-10000,10000);
                pp.RFindMask = RFindMask;
                pp.RFindDirections = RFindDirections;
                pp.WorldSpaceRFindDirs = WorldSpaceRFindDirs;
                pp.RFindMinimumDistance = RFindMinimumDistance;
                pp.RFindMaxDistance = RFindMaxDistance;
                pp.FlattendRFindNormal = FlattendRFindNormal;
                pp.RFindTries = RFindTries;
                pp.RFindSteps = RFindSteps;

                spwn.AdditionalGenerated = new List<GameObject>();
                spwn.AdditionalGenerated.Add(spawner);
                spwn.Prefab = spawner;
            };

            thisSpawn.OnGeneratedEvents.Add(pipesSpawn);

        }

        Matrix4x4 GetMatrix(SpawnData spawn)
        {
            Quaternion spawnRot = spawn.GetRotationOffset();
            Vector3 pos = spawn.GetWorldPositionWithFullOffset();
            return Matrix4x4.TRS(pos, spawnRot, Vector3.one);
        }

#if UNITY_EDITOR
        public override void OnDrawDebugGizmos(FieldSetup preset, SpawnData spawn, FieldCell cell, FGenGraph<FieldCell, FGenPoint> grid)
        {
            base.OnDrawDebugGizmos(preset, spawn, cell, grid);

            Gizmos.color = new Color(0.8f, 1f, 0.8f, 0.3f);
            Gizmos.matrix = GetMatrix(spawn);

            Gizmos.DrawCube(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f));

            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = _DbPreCol;
        }
#endif

    }
}