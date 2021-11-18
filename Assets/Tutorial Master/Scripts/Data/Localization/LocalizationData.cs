using System;
using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster.Localization
{
    /// <summary>
    /// Used to store and manage all Languages for a Tutorial Master.
    /// </summary>
    [Serializable]
    public class LocalizationData
    {
        public int CurrentLanguageIndex;
        public List<Language> Languages;

        public LocalizationData()
        {
            Languages = new List<Language>
            {
                new Language("English (US)")
            };

            SetLanguage(0);
        }

        public delegate void LanguageEvent(string languageId);

        public event LanguageEvent LanguageChange;

        /// <summary>
        /// Id of the Language which is currently set
        /// </summary>
        public string LanguageId
        {
            get { return Languages[CurrentLanguageIndex].Id; }
        }

        /// <summary>
        /// Change the language
        /// </summary>
        /// <param name="index">Index of a language.</param>
        public void SetLanguage(int index)
        {
            if (index < Languages.Count)
            {
                CurrentLanguageIndex = index;

                OnLanguageChanged(Languages[index].Id);
            }
        }

        /// <summary>
        /// Called when the Tutorial language has been changed.
        /// </summary>
        /// <param name="languageId">ID of a newly set Language.</param>
        protected virtual void OnLanguageChanged(string languageId)
        {
            var handler = LanguageChange;
            if (handler != null) handler(languageId);
        }
    }
}