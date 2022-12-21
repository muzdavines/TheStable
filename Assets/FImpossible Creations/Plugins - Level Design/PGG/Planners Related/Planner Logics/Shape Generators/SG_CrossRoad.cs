using FIMSpace.Generating.Checker;
using UnityEngine;

namespace FIMSpace.Generating.Planning.GeneratingLogics
{

    public class SG_CrossRoad : ShapeGeneratorBase
    {
        public override string TitleName() { return "Complex/Cross Road (mini city example)"; }

        public MinMax StreetsCount = new MinMax(24, 32);
        public MinMax StreetsLength = new MinMax(8, 14);
        [Range(1, 6)] public int StreetThickness = 1;

        public override CheckerField3D GetChecker(FieldPlanner planner)
        {
            CheckerField3D fullStreet = new CheckerField3D();

            Vector3Int[] latestPos = new Vector3Int[4]; // Remembering end street positions for continous generation
            Vector3Int[] latestDir = new Vector3Int[4];
            for (int i = 0; i < latestPos.Length; i++) { latestPos[i] = Vector3Int.zero; latestDir[i] = Vector3Int.zero; }

            // Generating street fragments and buildings around them
            for (int i = 0; i < StreetsCount.GetRandom(); i++)
            {
                int mod = i % 4;
                Vector3Int mainDir;

                // Cross out streets spreading out
                if (mod == 0) mainDir = Vector3Int.right;
                else if (mod == 1) mainDir = new Vector3Int(0,0,1);
                else if (mod == 2) mainDir = Vector3Int.left;
                else mainDir = new Vector3Int(0, 0, -1);

                CheckerField3D str = new CheckerField3D();

                if (i > 3) // After casting all 4 directions
                    if (FGenerators.GetRandom(0f, 1f) < 0.35f) // Chance to go with street to side in smaller distance
                    {
                        int randomSign = FGenerators.GetRandom(0f, 1f) > 0.5f ? 1 : -1;
                        mainDir = PGGUtils.GetRotatedFlatDirectionFrom(mainDir) * (randomSign);
                    }

                // Casting path line to desired position and remembering end position
                Vector3Int finalPos = latestPos[mod] + latestDir[mod] + mainDir * (StreetsLength.GetRandom());
                str.AddLinesTowards(latestPos[mod] + latestDir[mod], finalPos, 0.75f, StreetThickness);
                //str.AddLinesTowards(latestPos[mod] - mainDir + latestDir[mod], finalPos, 0.75f, StreetThickness);
                latestPos[mod] = finalPos;
                latestDir[mod] = mainDir;

                fullStreet.Join(str);

                RefreshPreview(fullStreet);
            }

            return fullStreet;
        }

    }
}