using UnityEngine;
using UnityEngine.EventSystems;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    public class ItemData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public Item item;
        public int item_amount = 1;

        public int parent_slot_id;

        private InventoryModule inv;
        private Vector2 offset;

        void Start()
        {
            inv = GameObject.FindWithTag("InventoryModule").GetComponent<InventoryModule>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (item != null)
            {
                offset = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y);
                this.transform.position = eventData.position - offset;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (item != null)
            {
                this.transform.SetParent(this.transform.root);
                this.transform.position = eventData.position - offset;
                GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (item != null)
            {
                this.transform.position = eventData.position - offset;
                this.transform.SetAsLastSibling();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            this.transform.SetParent(inv.slots[parent_slot_id].transform);
            this.transform.position = inv.slots[parent_slot_id].transform.position;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

        void Update()
        {
            if (item_amount > 1)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}