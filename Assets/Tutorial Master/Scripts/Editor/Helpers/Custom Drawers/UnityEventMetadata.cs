using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster.EditorUI.Drawers
{
    /// <summary>
    /// Stores the metadata of a UnityEvent.
    /// </summary>
    public class UnityEventMetadata
    {
        /// <summary>
        /// Name of the UnityEvent field.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Name of the Component this UnityEvent resides in.
        /// </summary>
        public readonly string SourceComponentName;

        /// <summary>
        /// Gets the menu path that will be used in context menu.
        /// </summary>
        public string MenuPath
        {
            get
            {
                return string.Format("{0}/{1}()", SourceComponentName, Name);
            }
        }

        public UnityEventMetadata(string name, string componentNameName)
        {
            Name = name;
            SourceComponentName = componentNameName;
        }

        public override bool Equals(object obj)
        {
            var item = obj as UnityEventMetadata;
            return item != null && MenuPath == item.MenuPath;
        }

        public override int GetHashCode()
        {
            var hashCode = -1030365785;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MenuPath);
            return hashCode;
        }
    }
}