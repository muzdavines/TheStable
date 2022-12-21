using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class CellInstanitations
    {
        public Vector3Int CellPosition;
        public List<InstantiatedData> List = new List<InstantiatedData>();
        public InstantiatedData this[int key]
        {
            get { return List[key]; }
            set { List[key] = value; }
        }

        //public CellInstanitations()
        //{
        //    List = new List<InstantiatedData>();
        //}

        public int Count { get { if (List == null) return 0; return List.Count; } }

        public void Add(InstantiatedData data)
        {
            if (FGenerators.CheckIfExist_NOTNULL(data)) CellPosition = data.spawn.OwnerCellPos;
            List.Add(data);
        }
    }
}
