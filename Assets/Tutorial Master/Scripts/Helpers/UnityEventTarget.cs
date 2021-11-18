using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Stores the UnityEvent
    /// </summary>
    [Serializable]
    public class UnityEventTarget
    {
        /// <summary>
        /// MonoBehaviour where UnityEvent is located
        /// </summary>
        public GameObject Source;

        /// <summary>
        /// Name of a component where UnityEvent is stored
        /// </summary>
        [SerializeField]
        private string _componentName = string.Empty;

        /// <summary>
        /// Name of the UnityEvent
        /// </summary>
        [SerializeField]
        private string _unityEventName = string.Empty;

        /// <summary>
        /// UnityEvent which is targeted
        /// </summary>
        [NonSerialized]
        private UnityEvent _unityEvent;

        /// <summary>
        /// The UnityAction callback function which has been added to this UnityEvent.
        /// </summary>
        [NonSerialized]
        private List<UnityAction> _addedCallbacks = new List<UnityAction>();

        /// <summary>
        /// Retrieves the UnityEvent.
        /// </summary>
        public UnityEvent Value
        {
            get
            {
                if (_unityEvent == null)
                    FindUnityEvent();

                return _unityEvent;
            }
        }

        /// <summary>
        /// Gets the predicate which helps to get suitable UnityEvent fields.
        /// </summary>
        /// <param name="eventName">Name of the UnityEvent that will be looked for.</param>
        /// <returns>Returns a predicate.</returns>
        public static Func<FieldInfo, bool> GetPredicate(string eventName)
        {
            var predicate =
                new Func<FieldInfo, bool>
                (
                    x => x.Name.Equals(eventName)
                           && (x.IsPublic || x.HasAttribute<SerializeField>())
                           && x.FieldType == typeof(UnityEvent)
                );

            return predicate;
        }

        /// <summary>
        /// Gets the predicate which helps to get suitable UnityEvent fields.
        /// </summary>
        /// <returns>Returns a predicate.</returns>
        public static Func<FieldInfo, bool> GetPredicate()
        {
            var predicate =
                new Func<FieldInfo, bool>
                (
                    x => (x.IsPublic || x.HasAttribute<SerializeField>())
                         && x.FieldType == typeof(UnityEvent)
                );

            return predicate;
        }

        /// <summary>
        /// Gets the binding flags to help locate suitable UnityEvents.
        /// </summary>
        public static BindingFlags Flags
        {
            get
            {
                return BindingFlags.Instance
                       | BindingFlags.Public
                       | BindingFlags.NonPublic;
            }
        }

        /// <summary>
        /// Sets the unity event target. Removes added listeners from the old one (if any).
        /// </summary>
        /// <param name="component">The component where UnityEvent resides.</param>
        /// <param name="unityEventName">Name of the UnityEvent.</param>
        public void SetUnityEventTarget(Component component, string unityEventName)
        {
            if (Value != null)
                RemoveAllAddedListeners();

            Source = component.gameObject;
            _componentName = component.GetType().Name;
            _unityEventName = unityEventName;

            FindUnityEvent();
        }

        /// <summary>
        /// Adds the listener to the UnityEvent.
        /// </summary>
        /// <param name="call">The callback function that will be added.</param>
        public void AddListener(UnityAction call)
        {
            if (Value == null)
                return;

            Value.AddListener(call);
            _addedCallbacks.Add(call);
        }

        /// <summary>
        /// Removes all added callback functions from target UnityEvent. Doesn't affect existing listeners.
        /// </summary>
        public void RemoveAllAddedListeners()
        {
            if (_addedCallbacks == null)
                return;

            if (Value == null)
                return;

            foreach (var callback in _addedCallbacks)
            {
                Value.RemoveListener(callback);
            }

            _addedCallbacks = null;
        }

        /// <summary>
        /// Finds the UnityEvent source and assigns it internally.
        /// </summary>
        private void FindUnityEvent()
        {
            var targetComponent = Source.GetComponent(_componentName);

            if (targetComponent == null)
                return;

            var unityEventInfo = targetComponent
                .GetType()
                .GetFields(Flags)
                .FirstOrDefault(GetPredicate(_unityEventName));

            if (unityEventInfo == null)
                return;

            _unityEvent = unityEventInfo.GetValue(targetComponent) as UnityEvent;
        }
    }
}