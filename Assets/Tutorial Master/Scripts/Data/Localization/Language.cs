using System;

namespace HardCodeLab.TutorialMaster.Localization
{
    [Serializable]
    public class Language
    {
        public string Id;
        public string Name;

        public Language(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }
    }
}