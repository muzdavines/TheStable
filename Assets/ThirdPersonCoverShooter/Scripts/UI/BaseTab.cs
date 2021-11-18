using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Button that actives objects when pressed. Can deactivate sibling objects of the target Object.
    /// </summary>
    public class BaseTab : PressButton
    {
        /// <summary>
        /// Object to activate when the tab is on.
        /// </summary>
        [Tooltip("Object to activate when the tab is on.")]
        public GameObject Object;

        /// <summary>
        /// Objects that are activated when the tab is active.
        /// </summary>
        [Tooltip("Objects that are activated when the tab is active.")]
        public GameObject[] Active;

        /// <summary>
        /// Objects that are activated when the tab is active.
        /// </summary>
        [Tooltip("Objects that are activated when the tab is active.")]
        public GameObject[] Inactive;

        /// <summary>
        /// Should the neighbouring objects be disabled when the target object is active.
        /// </summary>
        [Tooltip("Should the neighbouring objects be disabled when the target object is active.")]
        public bool DeactivateSiblings = true;

        private void Update()
        {
            var isActive = Object != null && Object.activeSelf;

            for (int i = 0; i < Active.Length; i++)
                if (Active[i] != null && Active[i].activeSelf != isActive)
                    Active[i].SetActive(isActive);

            for (int i = 0; i < Inactive.Length; i++)
                if (Inactive[i] != null && Inactive[i].activeSelf == isActive)
                    Inactive[i].SetActive(!isActive);
        }

        protected override void OnPress()
        {
            if (Object != null)
            {
                Object.SetActive(true);

                if (DeactivateSiblings && Object.transform.parent != null)
                {
                    for (int i = 0; i < Object.transform.parent.childCount; i++)
                    {
                        var child = Object.transform.parent.GetChild(i);

                        if (child != Object.transform)
                            child.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
