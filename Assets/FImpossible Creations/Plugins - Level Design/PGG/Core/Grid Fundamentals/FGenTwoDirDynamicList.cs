
using System;
using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public class FGenTwoDirDynamicList<T> where T : class, new()
    {
        private List<T> positive;
        private List<T> negative;

        public FGenTwoDirDynamicList()
        {
            positive = new List<T>();
            negative = new List<T>();
            negative.Add(null);
        }

        public T GetLastNegative()
        {
            return negative[negative.Count - 1];
        }

        public int GetNegativeLength()
        {
            return negative.Count;
        }

        public List<T> GetNegativeSource()
        {
            return negative;
        }

        public int GetPositiveLength()
        {
            return positive.Count;
        }

        public List<T> GetPositiveSource()
        {
            return positive;
        }

        public T GetAt(int i, Action<T, int> generatedCallback = null, bool generateIfOut = true)
        {
            if (i < 0)
            {
                if (-i >= negative.Count)
                {
                    if (generateIfOut == false) return null;

                    for (int n = negative.Count; n <= -i; ++n)
                    {
                        T newT = new T();
                        negative.Add(newT);
                        if (generatedCallback != null) generatedCallback(newT, -n);
                    }
                }

                return negative[-i];
            }
            else
            {

                if (i >= positive.Count)
                {
                    if (generateIfOut == false) return null;

                    for (int p = positive.Count; p <= i; ++p)
                    {
                        T newT = new T();
                        positive.Add(newT);
                        if (generatedCallback != null) generatedCallback(newT, p);
                    }
                }

                return positive[i];
            }
        }

        /// <summary>
        /// First cell must exists in list by calling GetAt
        /// </summary>
        public void SetAt(int i, T cell)
        {
            if (i >= 0) positive[i] = cell;
            else negative[-i] = cell;
        }


        public void Clear()
        {
            positive.Clear();
            negative.Clear();
            negative.Add(null);
        }

    }
}
