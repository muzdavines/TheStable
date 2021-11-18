using System;
using UnityEngine;
using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    [Serializable]
    public class ItemDatabase : MonoBehaviour
    {
        public List<Item> db;

        public Item ReturnItemByID(int id)
        {
            for (int i = 0; i < db.Count; i++)
            {
                if (db[i].id == id)
                {
                    return db[i];
                }
            }

            return null;
        }
    }
}