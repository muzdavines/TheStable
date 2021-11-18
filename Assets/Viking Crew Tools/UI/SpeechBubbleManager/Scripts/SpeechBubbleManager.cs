using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

using Random = UnityEngine.Random;

namespace VikingCrew.Tools.UI {
    public class SpeechBubbleManager : MonoBehaviour
    {
        public enum SpeechbubbleType
        {
            NORMAL,
            SERIOUS,
            ANGRY,
            THINKING,
        }
        [Serializable]
        public class SpeechbubblePrefab
        {
            public SpeechbubbleType type;
            public SpeechBubbleBase prefab;
        }

        [Header("Default settings:")]
        [FormerlySerializedAs("defaultColor")]
        [SerializeField]
        private Color _defaultColor = Color.white;

        [FormerlySerializedAs("defaultTimeToLive")]
        [SerializeField]
        private float _defaultTimeToLive = 1;

        [FormerlySerializedAs("is2D")]
        [SerializeField]
        private bool _is2D = true;

        [Tooltip("If you want to change the size of your speechbubbles in a scene without having to change the prefabs then change this value")]
        [FormerlySerializedAs("sizeMultiplier")]
        [SerializeField]
        private float _sizeMultiplier = 1f;

        [Tooltip("If you want to use different managers, for example if you want to have one manager for allies and one for enemies in order to style their speech bubbles differently set this to false. Note that you will need to keep track of a reference some other way in that case.")]
        [SerializeField]
        private bool _isSingleton = true;

        [Header("Prefabs mapping to each type:")]
        [FormerlySerializedAs("prefabs")]
        [SerializeField]
        private List<SpeechbubblePrefab> _prefabs;
        
        [Header("Optional")]
        [SerializeField]
        [Tooltip("Will use main camera if left as null")]
        private Camera _cam;

        private readonly Dictionary<SpeechbubbleType, SpeechBubbleBase> _prefabsDict = new Dictionary<SpeechbubbleType, SpeechBubbleBase>();
        private readonly Dictionary<SpeechbubbleType, Queue<SpeechBubbleBase>> _speechBubbleQueueDict = new Dictionary<SpeechbubbleType, Queue<SpeechBubbleBase>>();
        private readonly Dictionary<Transform, List<SpeechBubbleBase>> _actorBubblesMapping = new Dictionary<Transform, List<SpeechBubbleBase>>();

        private static SpeechBubbleManager _instance;
        public static SpeechBubbleManager Instance {
            get {
                Assert.IsNotNull(_instance, "The static variable for Instance has not been set. Did you do this call before Awake() has finished or unchecked \"Is Singleton\" maybe?");
                return _instance;
            }
        }

        public Camera Cam
        {
            get
            {
                return _cam;
            }

            set
            {
                _cam = value;
                foreach (var bubbleQueue in _speechBubbleQueueDict.Values)
                {
                    foreach (var bubble in bubbleQueue)
                    {
                        bubble.Cam = _cam;
                    }
                }
            }
        }

        protected void Awake()
        {
            if (_cam == null) _cam = Camera.main;

            if (_isSingleton) {
                Assert.IsNull(_instance, "_intance was not null. Do you maybe have several Speech Bubble Managers in your scene, all trying to be singletons?");
                _instance = this;
            }
            _prefabsDict.Clear();
            _speechBubbleQueueDict.Clear();
            foreach (var prefab in _prefabs)
            {
                _prefabsDict.Add(prefab.type, prefab.prefab);
                _speechBubbleQueueDict.Add(prefab.type, new Queue<SpeechBubbleBase>());
            }
        }
        
        private IEnumerator DelaySpeechBubble(float delay, Transform objectToFollow, string text, SpeechbubbleType type, float timeToLive, Color color, Vector3 offset)
        {
            yield return new WaitForSeconds(delay);
            if (objectToFollow)
                AddSpeechBubble(objectToFollow, text, type, timeToLive, color, offset);
        }

        /// <summary>
        /// Adds a speechbubble to a certain position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="timeToLive"></param>
        /// <param name="color"></param>
        public SpeechBubbleBase AddSpeechBubble(Vector3 position, string text, SpeechbubbleType type = SpeechbubbleType.NORMAL, float timeToLive = 0, Color color = default(Color))
        {
            if (timeToLive == 0) timeToLive = _defaultTimeToLive;
            if (color == default) color = _defaultColor;
            SpeechBubbleBase bubbleBehaviour = GetBubble(type);
            bubbleBehaviour.Setup(position, text, timeToLive, color, Cam);
            _speechBubbleQueueDict[type].Enqueue(bubbleBehaviour);
            return bubbleBehaviour;
        }

        /// <summary>
        /// Adds a speechbubble that will follow a certain transform.
        /// It is recommended you use a character's head or mouth transform.
        /// </summary>
        /// <param name="objectToFollow"></param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="timeToLive">if 0 then will use default time to live</param>
        /// <param name="color">Color to tint, default will be white</param>
        /// <param name="offset">Offset from objectToFollow</param>
        public SpeechBubbleBase AddSpeechBubble(Transform objectToFollow, string text, SpeechbubbleType type = SpeechbubbleType.NORMAL, float timeToLive = 0, Color color = default(Color), Vector3 offset = new Vector3())
        {
            if (timeToLive == 0) timeToLive = _defaultTimeToLive;
            if (color == default) color = _defaultColor;

            SpeechBubbleBase bubbleBehaviour = GetBubble(type);
            bubbleBehaviour.Setup(objectToFollow, offset, text, timeToLive, color, Cam);
            _speechBubbleQueueDict[type].Enqueue(bubbleBehaviour);

            if(objectToFollow != null)
                SetupSpeechBubbleOffsetInRelationToActor(objectToFollow, bubbleBehaviour);

            return bubbleBehaviour;
        }

        private void SetupSpeechBubbleOffsetInRelationToActor(Transform objectToFollow, SpeechBubbleBase bubbleBehaviour)
        {
            var actorBubbles = GetOrCreateSpeechBubblesCollectionForActor(objectToFollow);
            actorBubbles.Add(bubbleBehaviour);
            UpdateActorSpeechBubblesOffsets(actorBubbles);
        }

        private List<SpeechBubbleBase> GetOrCreateSpeechBubblesCollectionForActor(Transform actor)
        {
            List<SpeechBubbleBase> actorBubbles;
            if (!_actorBubblesMapping.TryGetValue(actor, out actorBubbles))
            {
                actorBubbles = new List<SpeechBubbleBase>();
                _actorBubblesMapping[actor] = actorBubbles;
            }
            return actorBubbles;
        }

        /// <summary>
        /// Adds a speechbubble that will follow a certain transform.
        /// It is recommended you use a character's head or mouth transform.
        /// 
        /// The speech bubble will be delayed and will only show up after the delay, making it possible to add a whole monologue or conversation between characters at once.
        /// If objectToFollow should be destroyed then no speech bubble will show up.
        /// </summary>
        /// <param name="delay">The time in seconds to wait until bubble shall be shown</param>
        /// <param name="objectToFollow"></param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <param name="timeToLive"></param>
        /// <param name="color"></param>
        /// <param name="offset"></param>
        public void AddDelayedSpeechBubble(float delay, Transform objectToFollow, string text, SpeechbubbleType type = SpeechbubbleType.NORMAL, float timeToLive = 0, Color color = default(Color), Vector3 offset = new Vector3())
        {
            StartCoroutine(DelaySpeechBubble(delay, objectToFollow, text, type, timeToLive, color, offset));
        }
        
        /// <summary>
        /// Gets a reused speechbubble from the queue or, if no free ones exist already, creates
        /// a new one.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private SpeechBubbleBase GetBubble(SpeechbubbleType type = SpeechbubbleType.NORMAL)
        {
            SpeechBubbleBase bubbleBehaviour;
            //Check to see if there is a free speechbuble of the right kind to reuse
            if (_speechBubbleQueueDict[type].Count == 0 || _speechBubbleQueueDict[type].Peek().gameObject.activeInHierarchy)
            {
                bubbleBehaviour = CreateNewSpeechBubble(type);
            }
            else
            {
                bubbleBehaviour = _speechBubbleQueueDict[type].Dequeue();
            }
            //Set as last to always place latest on top (in case of screenspace ui that is..)
            bubbleBehaviour.transform.SetAsLastSibling();
            return bubbleBehaviour;
        }

        private SpeechBubbleBase CreateNewSpeechBubble(SpeechbubbleType type)
        {   
            var newBubble = Instantiate(GetPrefab(type));
            newBubble.transform.SetParent(transform);
            newBubble.transform.localScale = _sizeMultiplier * GetPrefab(type).transform.localScale;
            
            //If this is not 2D then the speechbubble will need a world space canvas.
            if (!_is2D)
            {
                var canvas = newBubble.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.overrideSorting = true;
            }

            newBubble.SpeechBubbleFinishedEvent += OnSpeechBubbleFinished;
            return newBubble;
        }

        private void OnSpeechBubbleFinished(SpeechBubbleBase sender, Transform followedObject)
        {
            if (followedObject == null) return;
            if(_actorBubblesMapping.TryGetValue(followedObject, out var actorBubbles))
            {
                actorBubbles.Remove(sender);
                UpdateActorSpeechBubblesOffsets(actorBubbles);
            }
        }

        private static void UpdateActorSpeechBubblesOffsets(List<SpeechBubbleBase> actorBubbles)
        {
            if (actorBubbles.Count == 0) return;
            var queueOffset = 0f;

            for (int i = actorBubbles.Count -1; i >= 0; i--)
            {
                var item = actorBubbles[i];
                item.QueueOffset = queueOffset * Vector3.up;
                queueOffset += item.Height;
            }
        }

        private SpeechBubbleBase GetPrefab(SpeechbubbleType type)
        {
            return _prefabsDict[type];
        }

        public SpeechbubbleType GetRandomSpeechbubbleType()
        {
            return _prefabs[Random.Range(0, _prefabs.Count)].type;
        }

        /// <summary>
        /// Clears all visible Speech Bubbles from the scene, causing them to be instantly recycled
        /// </summary>
        public void Clear() {
            foreach (var bubbleQueue in _speechBubbleQueueDict) {
                foreach (var bubble in bubbleQueue.Value) {
                    bubble.Clear();
                }
            }
        }
        [ContextMenu("Test")]
        public void DebugAddSpeechBubble()
        {
            Instance.AddSpeechBubble(Vector3.zero, "TEST TEST\nTEST TEST\nTEST TEST\n", SpeechbubbleType.NORMAL, 5);
        }
    }
}