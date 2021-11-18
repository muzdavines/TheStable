using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    public class InventoryModule : MonoBehaviour
    {
        public Texture2D cursorTex;

        public int inv_maxSlots;
        public int item_maxSlots;
        public int wep_maxSlots;

        public GameObject inventoryPanel;
        public GameObject weaponsPanel;
        public GameObject itemsPanel;
        public GameObject slotPanel;
        public GameObject slot;
        public GameObject item;

        public List<Item> items_inventory = new List<Item>();
        public List<GameObject> slots = new List<GameObject>();

        ItemDatabase database;

        void Start()
        {
            database = GetComponent<ItemDatabase>();

            Cursor.SetCursor(cursorTex, Vector2.zero, CursorMode.ForceSoftware);

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].GetComponent<ItemSlot>().Id = i;
                items_inventory.Add(new Item());
            }

            for (int i = 0; i < database.db.Count; i++)
            {
                AddItem(i);
            }
        }

        public void AddItem(int id)
        {
            Item itemToAdd = database.ReturnItemByID(id);

            if (itemToAdd.isStackable && CheckIfExists(itemToAdd))
            {
                for (int i = 0; i < items_inventory.Count; i++)
                {
                    if (items_inventory[i].id == itemToAdd.id)
                    {
                        ItemData data = slots[i].transform.GetChild(0).GetComponent<ItemData>();
                        data.item_amount += 1;
                        data.transform.GetChild(0).GetComponent<Text>().text = data.item_amount.ToString();
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < items_inventory.Count; i++)
                {
                    if (items_inventory[i].id == -1)
                    {
                        items_inventory[i] = itemToAdd;
                        GameObject itemObj = Instantiate(item);
                        itemObj.GetComponent<ItemData>().item = itemToAdd;
                        itemObj.GetComponent<ItemData>().parent_slot_id = i;
                        itemObj.transform.SetParent(slots[i].transform);
                        itemObj.transform.position = slots[i].transform.position;
                        itemObj.GetComponent<Image>().sprite = itemToAdd.icon;
                        break;
                    }
                }
            }
        }

        public bool CheckIfExists(Item itm)
        {
            for (int i = 0; i < items_inventory.Count; i++)
            {
                if (items_inventory[i].id == itm.id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}