#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/Level Design/Visual Stamp", 3)]
    public class VisualStamp : MonoBehaviour, IGenerating
    {
        public Transform ToScale;
        public Renderer ToChangeMaterial;
        public MeshFilter ToChangeMesh;
        public ParticleSystem ToModifyParticle;

        [Space(4)]
        public List<Mesh> MeshesToChoose;
        public List<Material> MaterialsToChoose;
        [Space(4)]
        [HideInInspector] public Vector2 ScaleRandomize = new Vector2(1f, 1f);
        [HideInInspector] public bool UniformScale = true;
        [HideInInspector] public Vector3 ScaleAxisPower = Vector3.one;

        [Space(4)]
        [HideInInspector] public Vector2 EmissionRandomize = new Vector2(1f, 1f);
        [HideInInspector] public Vector2 EmRandomStartTime = new Vector2(0f, 0f);

        bool doneAlready = false;
        Vector3 initialLocalScale = Vector3.zero;


        void Start()
        {
            if (doneAlready) return;
            Randomize();
        }

        public void Randomize()
        {

            if (ToChangeMesh)
                if (MeshesToChoose.Count > 0)
                    ToChangeMesh.sharedMesh = MeshesToChoose.GetRandomElement();


            if (ToChangeMaterial)
                if (MaterialsToChoose.Count > 0)
                {
                    ToChangeMaterial.sharedMaterial = MaterialsToChoose.GetRandomElement();
                }


            if (ToScale)
            {
                if (initialLocalScale == Vector3.zero) initialLocalScale = ToScale.localScale;

                if (ScaleRandomize != Vector2.one)
                {
                    Vector3 newScale = initialLocalScale;

                    if (UniformScale)
                    {
                        newScale *= FGenerators.GetRandom(ScaleRandomize.x, ScaleRandomize.y) * ScaleAxisPower.x;
                    }
                    else
                    {
                        if (ScaleAxisPower.x > 0f) newScale.x = ToScale.localScale.x * FGenerators.GetRandom(ScaleRandomize.x, ScaleRandomize.y) * ScaleAxisPower.x;
                        if (ScaleAxisPower.y > 0f) newScale.y = ToScale.localScale.y * FGenerators.GetRandom(ScaleRandomize.x, ScaleRandomize.y) * ScaleAxisPower.y;
                        if (ScaleAxisPower.z > 0f) newScale.z = ToScale.localScale.z * FGenerators.GetRandom(ScaleRandomize.x, ScaleRandomize.y) * ScaleAxisPower.z;
                    }

                    ToScale.localScale = newScale;
                }
            }


            if (ToModifyParticle)
            {
                if (EmissionRandomize != Vector2.one)
                {
                    var em = ToModifyParticle.emission;
                    em.rateOverTimeMultiplier = FGenerators.GetRandom(EmissionRandomize.x, EmissionRandomize.y);
                    em.rateOverDistanceMultiplier = FGenerators.GetRandom(EmissionRandomize.x, EmissionRandomize.y);
                }

                if (EmRandomStartTime != Vector2.zero)
                {
                    float randomTime = FGenerators.GetRandom(EmRandomStartTime.x, EmRandomStartTime.y);
                    if (randomTime <= 0f) ToModifyParticle.Play(true);
                    else StartCoroutine(IEDelayParticleStart(ToModifyParticle, randomTime));
                }
            }


            doneAlready = true;
        }


        IEnumerator IEDelayParticleStart(ParticleSystem ps, float timeToWait)
        {
            float elapsed = 0f;
            while (elapsed < timeToWait)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            ps.Play(true);
        }


        public void Generate()
        {
            Randomize();
        }

        public void PreviewGenerate()
        {
            Randomize();
        }

        public void IG_CallAfterGenerated() { }


    }



    #region Editor Class
#if UNITY_EDITOR
    /// <summary>
    /// FM: Editor class component to enchance controll over component from inspector window
    /// </summary>
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(VisualStamp))]
    public class VisualStampEditor : UnityEditor.Editor
    {
        public VisualStamp Get { get { if (_get == null) _get = (VisualStamp)target; return _get; } }
        private VisualStamp _get;

        SerializedProperty sp_ScaleRandomize;
        SerializedProperty sp_EmissionRandomize;

        private void OnEnable()
        {
            sp_ScaleRandomize = serializedObject.FindProperty("ScaleRandomize");
            sp_EmissionRandomize = serializedObject.FindProperty("EmissionRandomize");
        }

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("  Simple component to randomize object visual state like model/material/scale when game starts!", UnityEditor.MessageType.Info);

            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");


            if (Get.ToScale != null)
            {

                GUILayout.Space(6);

                Vector2 scale = Get.ScaleRandomize;
                EditorGUILayout.MinMaxSlider("Scale Randomize:", ref scale.x, ref scale.y, 0f, 1f);
                Get.ScaleRandomize = scale;

                if (Get.ScaleRandomize != Vector2.one)
                {
                    SerializedProperty spc = sp_ScaleRandomize.Copy();
                    spc.Next(false);
                    EditorGUILayout.PropertyField(spc);

                    if (!spc.boolValue)
                    {
                        spc.Next(false);
                        EditorGUILayout.PropertyField(spc);
                    }
                    else
                    {
                        spc.Next(false);
                        float x = EditorGUILayout.FloatField("Scale Factor:", spc.vector3Value.x);
                        Get.ScaleAxisPower.x = x;
                    }

                    if (Get.UniformScale)
                        EditorGUILayout.HelpBox("Scale from " + (Get.ScaleRandomize.x * Get.ScaleAxisPower.x) + " to " + (Get.ScaleRandomize.y * Get.ScaleAxisPower.x), MessageType.None);
                    else
                        EditorGUILayout.HelpBox("Scale from " + (Get.ScaleRandomize.x) + " to " + (Get.ScaleRandomize.y) + "  times AxisPower", MessageType.None);

                }
                else
                {
                    EditorGUILayout.HelpBox("Randomize range is 1-1 so not using scaling!", MessageType.None);
                }

            }


            if (Get.ToModifyParticle)
            {
                GUILayout.Space(6);

                Vector2 emiRand = Get.EmissionRandomize;
                EditorGUILayout.MinMaxSlider("Emission Randomize:", ref emiRand.x, ref emiRand.y, 0f, 3f);
                Get.EmissionRandomize = emiRand;

                if (Get.EmissionRandomize == Vector2.one)
                    EditorGUILayout.HelpBox("Randomization is 1-1 so not using emission randomize", MessageType.None);
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Emission Multiplier from " + (Get.EmissionRandomize.x) + " to " + (Get.EmissionRandomize.y), MessageType.None);
                    if (GUILayout.Button("Reset", GUILayout.Width(40))) { Get.EmissionRandomize = Vector2.one; }
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(2);
                SerializedProperty spc = sp_EmissionRandomize.Copy();
                spc.Next(false);
                EditorGUILayout.PropertyField(spc, new GUIContent("Random Start Delay:", "Random delay before particle start to play for randomization. If you set 0 - 2 then particle will play after starting scene in 0 sec up to 2 sec delay."));
                Vector2 modVal = spc.vector2Value;
                if (spc.vector2Value.x < 0f) modVal.x = 0f;
                if (spc.vector2Value.y < 0f) modVal.y = 0f;
                spc.vector2Value = modVal;
            }



            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
    #endregion


}