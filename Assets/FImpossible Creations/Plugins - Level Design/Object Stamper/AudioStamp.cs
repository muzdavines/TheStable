#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace FIMSpace.Generating
{
    [AddComponentMenu("FImpossible Creations/Level Design/Audio Stamp", 3)]
    public class AudioStamp : MonoBehaviour, IGenerating
    {
        public AudioSource AudioToRandomize;

        [Space(4)]
        public List<AudioClip> ClipsToChoose;

        [HideInInspector] public Vector2 VolumeRandomize = new Vector2(1f, 1f);
        [HideInInspector] public Vector2 PitchRandomize = new Vector2(0f, 0f);
        [HideInInspector] public Vector2 PlayProgressRandomize = new Vector2(0f, 0f);

        bool doneAlready = false;
        Vector3 initialLocalScale = Vector3.zero;

        void Reset()
        {
            AudioToRandomize = GetComponent<AudioSource>();
            if (!AudioToRandomize) AudioToRandomize = GetComponentInChildren<AudioSource>();
            if (!AudioToRandomize) AudioToRandomize = GetComponentInParent<AudioSource>();
        }

        void Start()
        {
            if (doneAlready) return;
            Randomize();
        }

        public void Randomize()
        {
            if (AudioToRandomize == null) return;

            if ( ClipsToChoose.Count > 0)
            {
                AudioToRandomize.clip = ClipsToChoose.GetRandomElement();
            }

            if ( VolumeRandomize != Vector2.one)
            {
                AudioToRandomize.volume *= FGenerators.GetRandom(VolumeRandomize.x, VolumeRandomize.y);
            }

            if ( PitchRandomize != Vector2.zero)
            {
                AudioToRandomize.pitch += FGenerators.GetRandom(PitchRandomize.x, PitchRandomize.y);
            }

            if (AudioToRandomize.loop)
            {
                if (AudioToRandomize.clip != null)
                    if (PlayProgressRandomize != Vector2.zero)
                    {
                        float progress = FGenerators.GetRandom(PlayProgressRandomize.x, PlayProgressRandomize.y);

                        if (progress > 0f)
                        {
                            if (progress >= 0.999f) progress = 0.99f;
                            AudioToRandomize.Play();
                            AudioToRandomize.time = AudioToRandomize.clip.length * progress;
                        }

                    }
            }


            doneAlready = true;
        }


        //IEnumerator IEDelayAudioStart(AudioSource a, float timeToWait)
        //{
        //    float elapsed = 0f;
        //    while (elapsed < timeToWait)
        //    {
        //        elapsed += Time.deltaTime;
        //        yield return null;
        //    }

        //    a.Play();
        //}


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
    [UnityEditor.CustomEditor(typeof(AudioStamp))]
    public class AudioStampEditor : UnityEditor.Editor
    {
        public AudioStamp Get { get { if (_get == null) _get = (AudioStamp)target; return _get; } }
        private AudioStamp _get;

        //SerializedProperty sp_ScaleRandomize;

        private void OnEnable()
        {
            //sp_EmissionRandomize = serializedObject.FindProperty("EmissionRandomize");
        }

        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("  Simple component to randomize Audio Source Play when game starts!", UnityEditor.MessageType.Info);

            serializedObject.Update();

            GUILayout.Space(4f);
            DrawPropertiesExcluding(serializedObject, "m_Script");

            GUILayout.Space(4f);
            Vector2 sliderVal;

            sliderVal = Get.VolumeRandomize;
            EditorGUILayout.MinMaxSlider(new GUIContent("Randomize Volume:", "Multiplying initial volume value of the audio source with given range"), ref sliderVal.x, ref sliderVal.y, 0f, 1f);
            Get.VolumeRandomize = sliderVal;

            if (Get.VolumeRandomize != Vector2.one)
            {
                EditorGUILayout.HelpBox("Randomize Volume from " + sliderVal.x + " to " + sliderVal.y, MessageType.None);
                GUILayout.Space(3);
            }

            sliderVal = Get.PitchRandomize;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.MinMaxSlider(new GUIContent("Randomize Pitch:", "Adding to initial pitch value of the audio source with given range"), ref sliderVal.x, ref sliderVal.y, -2f, 2f);
            if (sliderVal != Vector2.zero) if (GUILayout.Button("Reset", GUILayout.Width(40))) { Get.PitchRandomize = Vector2.zero; }
            EditorGUILayout.EndHorizontal();
            Get.PitchRandomize = sliderVal;

            if (sliderVal != Vector2.zero)
            {
                EditorGUILayout.HelpBox("Randomize Add pitch from " + sliderVal.x + " to " + sliderVal.y, MessageType.None);
                GUILayout.Space(3);
            }

            if (Get.AudioToRandomize)
                if (Get.AudioToRandomize.loop)
                {
                    GUILayout.Space(4f);
                    sliderVal = Get.PlayProgressRandomize;
                    EditorGUILayout.MinMaxSlider(new GUIContent("Randomize Play Progress:", "Useful for looped audio, starting audiosource in random time of it's clip"), ref sliderVal.x, ref sliderVal.y, 0f, 1f);
                    Get.PlayProgressRandomize = sliderVal;

                    if (sliderVal != Vector2.zero)
                    {
                        EditorGUILayout.HelpBox("Randomize play progress from " + sliderVal.x + " to " + sliderVal.y, MessageType.None);
                        GUILayout.Space(3);
                    }
                }

            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
    #endregion


}