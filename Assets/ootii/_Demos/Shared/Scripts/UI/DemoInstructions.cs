using com.ootii.Geometry;
using UnityEngine;
using UnityEngine.UI;

namespace com.ootii.Demos
{
    public class DemoInstructions : MonoBehaviour
    {
        [Tooltip("Contains the Title, Description, and Input Settings to display.")]
        public DemoProperties DemoProperties = null;

        [Tooltip("Key to toggle the instructions panel on or off.")]
        public KeyCode ToggleKey = KeyCode.F12;

        [Header("Item Template")]
        [Tooltip("Prefab template for displaying the input items.")]
        public GameObject InputItemPrefab;

        [Header("UI Elements")]
        [Tooltip("Text UI element to display the title.")]
        public Text Title;

        [Tooltip("Text UI element to display the description.")]
        public Text Description;

        [Tooltip("UI Element that holds the input items.")]
        public RectTransform InputItemsPanel;

        [Tooltip("The CanvasGroup that controls this element's visibility.")]
        public CanvasGroup CanvasGroup;

        private void Start()
        {            
            if (DemoProperties == null) { return; }
            if (CanvasGroup == null)
            {
                CanvasGroup = DemoProperties.GetOrAddComponent<CanvasGroup>();                
            }

            if (Title != null) Title.text = DemoProperties.Title;
            if (Description != null) Description.text = DemoProperties.Description;

            if (DemoProperties.InputItems == null || InputItemsPanel == null) return;

            foreach (var item in DemoProperties.InputItems)
            {
                var inputItem = GameObject.Instantiate(InputItemPrefab);
                if (inputItem == null) continue;

                var viewmodel = inputItem.GetComponent<DemoInputItem>();
                if (viewmodel != null)
                {
                    viewmodel.SetItem(item);
                }
                
                inputItem.transform.SetParent(InputItemsPanel);
                inputItem.transform.ResetRect();
            }
        }

        private void Update()
        {
            if (CanvasGroup == null) { return; }
            if (GetKeyDown(ToggleKey))
            {
                CanvasGroup.alpha = CanvasGroup.alpha == 0 ? 1 : 0;
            }
        }

        private bool GetKeyDown(KeyCode rKey)
        {
#if ENABLE_INPUT_SYSTEM
            return UnityEngine.Input.GetKeyDown(rKey);
#else
            return UnityEngine.Input.GetKeyDown(rKey);
#endif
        }
    }
}

