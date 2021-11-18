using UnityEngine;
using UnityEngine.UI;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    public class LanguageChanger : MonoBehaviour
    {
        public TutorialMasterManager TutorialManager;

        private Dropdown _dropdown;

        void Awake()
        {
            _dropdown = GetComponent<Dropdown>();

            if (_dropdown == null)
                return;

            _dropdown.onValueChanged.AddListener(delegate
            {
                TutorialManager.SetLanguage(_dropdown.value);
            });
        }
    }
}