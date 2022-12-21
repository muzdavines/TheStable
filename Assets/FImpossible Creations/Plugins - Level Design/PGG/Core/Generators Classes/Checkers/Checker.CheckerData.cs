using FIMSpace.Generating.Planning;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {


        public class CheckerData
        {
            public Vector2Int ChildPos = Vector2Int.zero;
            public string Data = "";

            internal CheckerData Copy()
            {
                return (CheckerData)MemberwiseClone();
            }
        }

        public void SpreadData(Vector2Int startWorldPos, int spreadDistance, string dataString)
        {
            if ( spreadDistance <= 0)
            {
                Vector2Int pos = startWorldPos;
                CheckerData chData = GetWorldPosCheckerData(pos);
                if (FGenerators.CheckIfIsNull(chData))
                {
                    CheckerData data = new CheckerData();
                    data.ChildPos = pos - Position;
                    data.Data = dataString;
                    Datas.Add(data);
                }
                else
                    chData.Data = dataString;

                return;
            }

            for (int x = -spreadDistance; x <= spreadDistance; x++)
            {
                for (int y = -spreadDistance; y <= spreadDistance; y++)
                {
                    Vector2Int pos = startWorldPos + new Vector2Int(x, y);
                    CheckerData chData = GetWorldPosCheckerData(pos);
                    if (FGenerators.CheckIfIsNull(chData ))
                    {
                        CheckerData data = new CheckerData();
                        data.ChildPos = pos - Position;
                        data.Data = dataString;
                        Datas.Add(data);
                    }
                    else
                        chData.Data = dataString;
                }
            }
        }

        public bool CheckerDataInRange(CheckerField other, int cellsDistance, string data, bool checkPrecise = false)
        {
            if ( checkPrecise)
            {
                for (int x = -cellsDistance; x <= cellsDistance; x++)
                {
                    for (int y = -cellsDistance; y <= cellsDistance; y++)
                    {
                        for (int i = 0; i < ChildPos.Count; i++)
                        {
                            Vector2Int pos = WorldPos(i) + new Vector2Int(x, y);
                            //Vector2Int pos = Position + new Vector2Int(x, y);
                            CheckerData oth = other.GetWorldPosCheckerData(pos);
                            if (oth != null) if (oth.Data == data) return true;
                        }
                    }
                }
            }
            else
            {
                for (int x = -cellsDistance; x <= cellsDistance; x++)
                {
                    for (int y = -cellsDistance; y <= cellsDistance; y++)
                    {
                        Vector2Int pos = WorldPos(0) + new Vector2Int(x, y);
                        //Vector2Int pos = Position + new Vector2Int(x, y);
                        CheckerData oth = other.GetWorldPosCheckerData(pos);
                        if (oth != null) if (oth.Data == data) return true;
                    }
                }
            }


            return false;
        }

        public bool CheckerDataInRange(List<CheckerField> others, int cellDistance, string data, bool checkPrecise = false)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (others[i] == this) continue;
                if (CheckerDataInRange(others[i], cellDistance, data, checkPrecise)) return true;
            }

            return false;
        }


        public CheckerData GetWorldPosCheckerData(Vector2Int worldPos)
        {
            for (int c = 0; c < Datas.Count; c++)
                if (Datas[c].ChildPos + Position == worldPos) return Datas[c];

            return null;
        }

    }
}