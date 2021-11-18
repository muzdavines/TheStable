using System;
using UnityEngine;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    [Serializable]
    public class Item
    {
        public int id;
        public string name;
        public string category;
        public bool isStackable;
        public Sprite icon;

        public Item()
        {
            id = -1;
        }
    }
}