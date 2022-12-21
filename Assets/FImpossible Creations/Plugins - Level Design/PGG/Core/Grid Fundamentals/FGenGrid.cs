using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public class FGenGrid<T> where T : class, new()
    {
        //public FGenTwoDirDynamicList<FGenTwoDirDynamicList<FGenTwoDirDynamicList<T>>> Dimensions { get; private set; }
        public Dictionary<Vector3Int, T> Cells;

        /// <summary> For 2D depth should be Z so 'DepthIsY = false' </summary>
        public bool DepthIsY = true;
        public FGenTwoDirDynamicList<Dictionary<Vector3Int, T>> DepthLayers;

        public delegate void OnGeneratedElement(T generated);

        public FGenGrid()
        {
            Cells = new Dictionary<Vector3Int, T>();
        }

        public T GetCell(float x, float y, float z, Action<T, int> callback = null, bool generateIfOut = true)
        {
            T cll = null;
            Vector3Int key = new Vector3(x, y, z).V3toV3Int();

            if (Cells.TryGetValue(key, out cll))
            {
                return cll;
            }
            else
            {
                if (generateIfOut)
                {
                    cll = new T();
                    Cells.Add(key, cll);
                    if (callback != null) callback(cll, 0);
                }

            }

            return cll;
        }


        public T GetCell(float x, float z, Action<T, int> callback = null, bool generateIfOut = true)
        {
            return GetCell((int)x, 0, (int)z, callback, generateIfOut);
        }

        public void ReplaceCell(int x, int y, int z, ref T cell)
        {
            Vector3Int key = Key(x, y, z);
            T gcell = GetCell(key);
            if (gcell == null) Cells.Add(key, cell);
            else SetCell(key, ref cell);
        }

        public T GetCell(int x, int y, int z)
        {
            if (Cells.TryGetValue(Key(x, y, z), out T cell)) return cell;
            else return null;
        }

        public T GetCell(Vector3Int key)
        {
            if (Cells.TryGetValue(key, out T cell)) return cell;
            else return null;
        }

        public void SetCell(Vector3Int key, ref T cell)
        {
            if (Cells.TryGetValue(key, out _)) Cells[key] = cell;
            else Cells.Add(key, cell);
        }

        public Vector3Int Key(int x, int y, int z)
        {
            return new Vector3Int(x, y, z);
        }

        public void Clear()
        {
            Cells.Clear();
        }
    }
}
