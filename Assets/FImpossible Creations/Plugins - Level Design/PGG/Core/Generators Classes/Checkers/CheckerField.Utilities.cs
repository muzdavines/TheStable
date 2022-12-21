using FIMSpace.Generating.Planning;
using FIMSpace.Generating.PathFind;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Checker
{
    public partial class CheckerField
    {
        internal CheckerField Copy()
        {
            CheckerField f = (CheckerField)MemberwiseClone();

            f.parentPosition = parentPosition;
            f.Position = Position;
            f.FloatingOffset = FloatingOffset;
            f.UseBounds = UseBounds;
            f.HelperReference = HelperReference;

            f.ChildPos = new FCheckerGraph<CheckerPos>();
            for (int i = 0; i < ChildPos.Count; i++) f.ChildPos.Add(ChildPosition(i));

            f.Bounding = new List<CheckerBounds>();
            for (int i = 0; i < Bounding.Count; i++) f.Bounding.Add(Bounding[i].Copy(f));

            f.Datas = new List<CheckerData>();
            for (int i = 0; i < Datas.Count; i++) f.Datas.Add(Datas[i].Copy());


            return f;
        }

        public Vector2Int ChildPosition(int i)
        {
            return ChildPos.AllApprovedCells[i].ToV2();
        }

        public void OffsetChildCell(int i, Vector2Int offset)
        {
            Vector2Int pos = ChildPos.AllApprovedCells[i].ToV2();
            ChildPos.Remove(pos);
            ChildPos.Add(pos + offset);
        }

        public void MoveChildCell(int i, Vector2Int newPos)
        {
            Vector2Int pos = ChildPos.AllApprovedCells[i].ToV2();
            ChildPos.Remove(pos);
            ChildPos.Add(newPos);
        }

        public Vector2Int GetRandom(bool local)
        {
            if (local)
                return ChildPosition(FGenerators.GetRandom(0, ChildPos.Count));
            else
                return WorldPos(FGenerators.GetRandom(0, ChildPos.Count));
        }


        public List<Vector2Int> GetRandomizedPositionsCopy(bool local = false)
        {
            List<Vector2Int> pule = new List<Vector2Int>(); // Getting copy which will have removed elements

            if (local)
                for (int i = 0; i < ChildPos.Count; i++) pule.Add(ChildPosition(i));
            else
                for (int i = 0; i < ChildPos.Count; i++) pule.Add(WorldPos(i));

            List<Vector2Int> randomized = new List<Vector2Int>();
            for (int i = 0; i < ChildPos.Count; i++)
            {
                int randomI = FGenerators.GetRandom(0, pule.Count);
                randomized.Add(pule[randomI]);
                pule.RemoveAt(randomI);
            }

            return randomized;
        }


        public static Vector2Int RandomDirection()
        {
            return GetDirection(FGenerators.GetRandom(0, 4));
        }


        /// <summary>
        /// 0 -> right 1 -> up, 2 -> left 3 -> down
        /// </summary>
        public static Vector2Int GetDirection(int r)
        {
            if (r < 0) r = -r; r = r % 4;
            if (r == 0) return new Vector2Int(1, 0);
            else if (r == 1) return new Vector2Int(0, 1);
            else if (r == 2) return new Vector2Int(-1, 0);
            else return new Vector2Int(0, -1);
        }


        public void DrawGizmos(float scaleUp = 1f, bool useHandles = false, float drawSz = 1f)
        {
            Vector3 off = new Vector3(2, 0, 2) * scaleUp;

            if (UseBounds == false || Bounding == null || Bounding.Count == 0 )
            {
                if (ChildPos == null) ChildPos = new FCheckerGraph<CheckerPos>();

                Vector3 drawSize = new Vector3(scaleUp, scaleUp * 0.1f, scaleUp);
                Vector3 mainPos = Position.V2toV3Bound() * scaleUp;
                string pos = "";
                for (int c = 0; c < ChildPos.Count; c++)
                {
                    pos += "[" + c + "] " + ChildPosition(c) + "  , ";
                    if (!useHandles)
                        Gizmos.DrawCube(mainPos + off + FloatingOffset + ChildPosition(c).V2toV3Bound() * scaleUp, drawSize * drawSz);
#if UNITY_EDITOR
                    else
                        UnityEditor.Handles.CubeHandleCap(0, mainPos + off + FloatingOffset + ChildPosition(c).V2toV3Bound() * scaleUp, Quaternion.identity, scaleUp, EventType.Repaint);
#endif
                }
            }

            if (UseBounds)
                for (int i = 0; i < Bounding.Count; i++)
                {
                    Bounding[i].DrawGizmo(scaleUp, true);
                }

            if (!useHandles)
                Gizmos.DrawSphere(Position.V2toV3Bound() * scaleUp + off + FloatingOffset, scaleUp * 0.25f);
#if UNITY_EDITOR
            else
                UnityEditor.Handles.SphereHandleCap(0, Position.V2toV3Bound() * scaleUp + off + FloatingOffset, Quaternion.identity, scaleUp * 0.25f, EventType.Repaint);
#endif
        }

    }
}