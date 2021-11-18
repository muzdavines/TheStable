using System;
using System.Collections.Generic;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.Localization
{
    [Serializable]
    public abstract class LocalizedContent<TContent> : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Stores all the language keys
        /// </summary>
        [SerializeField] protected List<string> _keys;

        /// <summary>
        /// Stores all the content for languages
        /// </summary>
        [SerializeField] protected List<TContent> _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedContent{TContent}"/> class.
        /// </summary>
        protected LocalizedContent()
        {
            Content = new Dictionary<string, TContent>();
            _keys = new List<string>();
            _values = new List<TContent>();
        }

        public Dictionary<string, TContent> Content { get; private set; }

        /// <summary>
        /// Retrieves the content for a specific language
        /// </summary>
        /// <param name="id">The Id of the language.</param>
        /// <returns></returns>
        public TContent GetContent(string id)
        {
            if (!Content.ContainsKey(id))
            {
                Content.Add(id, default(TContent));
            }

            return Content[id];
        }

        /// <inheritdoc />
        /// <summary>
        /// Migrates content from lists into a dictionary for better runtime performance.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Content = new Dictionary<string, TContent>();

            for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++)
            {
                Content.Add(_keys[i], _values[i]);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes content from a dictionary and stores them in serializable data structure (2 separate lists).
        /// </summary>
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var pair in Content)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        /// <summary>
        /// Removes the content for a language
        /// </summary>
        /// <param name="id">The Id of the language.</param>
        public void RemoveContent(string id)
        {
            Content.Remove(id);
        }

        /// <summary>
        /// Sets the content for a specific language.
        /// </summary>
        /// <param name="id">The Id of the language.</param>
        /// <param name="value">The new content.</param>
        public void SetContent(string id, TContent value)
        {
            if (Content.ContainsKey(id))
            {
                Content[id] = value;
            }
        }
    }
}