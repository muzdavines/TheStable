using UnityEngine;
using UnityEngine.UI;

namespace com.ootii.Demos
{
    public class DemoInputItem : MonoBehaviour
    {
        [Tooltip("The input alias for this action.")]
        public Text InputAlias;

        [Tooltip("Description of the action.")]
        public Text Description;

        public void SetItem(DemoProperties.InputItem rInputItem)
        {
            if (InputAlias != null) InputAlias.text = rInputItem.InputAlias;
            if (Description != null) Description.text = rInputItem.ActionDescription;
        }
    }
}

