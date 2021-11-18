using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    public class ItemSlot : MonoBehaviour, IDropHandler
    {
        public int Id;
        private InventoryModule inventoryModule;

        public string slotCategory;

        public UnityEvent OnItemDropped;

        void Start()
        {
            inventoryModule = GameObject.FindWithTag("InventoryModule").GetComponent<InventoryModule>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            ItemData droppedItem = eventData.pointerDrag.GetComponent<ItemData>();

            if (inventoryModule.items_inventory[Id].id == -1)
            {
                if (slotCategory.Equals(droppedItem.item.category) || slotCategory == "Common")
                {
                    inventoryModule.items_inventory[droppedItem.parent_slot_id] = new Item();
                    inventoryModule.items_inventory[Id] = droppedItem.item;
                    droppedItem.GetComponent<ItemData>().parent_slot_id = Id;
                    OnItemDropped.Invoke();
                }
            }
            else
            {
                if (inventoryModule.items_inventory[droppedItem.parent_slot_id].category.Equals(slotCategory))
                {
                    if (transform.childCount > 0)
                    {
                        Transform item = transform.GetChild(0);
                        item.GetComponent<ItemData>().parent_slot_id = droppedItem.parent_slot_id;
                        item.transform.SetParent(inventoryModule.slots[droppedItem.parent_slot_id].transform);
                        item.transform.position = inventoryModule.slots[droppedItem.parent_slot_id].transform.position;

                        droppedItem.parent_slot_id = Id;
                        droppedItem.transform.SetParent(this.transform);
                        droppedItem.transform.position = this.transform.position;

                        inventoryModule.items_inventory[droppedItem.parent_slot_id] =
                            item.GetComponent<ItemData>().item;
                        inventoryModule.items_inventory[Id] = droppedItem.item;
                    }
                }
            }
        }
    }
}