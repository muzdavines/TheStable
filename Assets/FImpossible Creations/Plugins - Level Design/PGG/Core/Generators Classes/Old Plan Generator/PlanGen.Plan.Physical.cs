using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.PathFind;

namespace FIMSpace.Generating
{
    public partial class PlanHelper
    {

        #region Debug

        public bool IsColliding(ref HelperRect rect)
        {
            for (int i = 0; i < InteriorRects.Count; i++)
            {
                if (InteriorRects[i].pos == rect.pos) continue;
                Bounds mBound = rect.Bound; mBound.size *= 0.99f;
                if (InteriorRects[i].Bound.Intersects(mBound)) { rect = InteriorRects[i]; return true; }
            }

            return false;
        }

        public List<HelperRect> IsCollidingAll(HelperRect rect)
        {
            List<HelperRect> collided = new List<HelperRect>();

            for (int i = 0; i < InteriorRects.Count; i++)
            {
                if (InteriorRects[i].pos == rect.pos) continue;
                Bounds mBound = rect.Bound; mBound.size *= 0.99f;
                if (InteriorRects[i].Bound.Intersects(mBound)) collided.Add(InteriorRects[i]);
            }

            return collided;
        }


        #endregion

        bool limited = false;
        Vector2Int XLimit;
        Vector2Int ZLimit;
        public void SetLimits(Vector2Int xLimit, Vector2Int zLimit)
        {
            limited = true;
            XLimit = xLimit;
            ZLimit = zLimit;

        }

        public bool IsAnyColliding(HelperRect rect, float wallsSeparation, int? ignore)
        {
            for (int i = 0; i < InteriorRects.Count; i++)
            {
                if (ignore != null) if (i == ignore.Value) continue;
                //if (rect.TypeID == -1 && InteriorRects[i].TypeID == -1) continue; // Dont check corridors collision

                Bounds mBound = rect.Bound;

                mBound.size *= 0.99f;
                if (rect.TypeID != -1) mBound.size = mBound.size + new Vector3(1f, 0f, 1f) * (wallsSeparation);

                if (InteriorRects[i].Bound.Intersects(mBound))
                {
                    //FDebug.DrawBounds3D(mBound, Color.blue);
                    //FDebug.DrawBounds3D(InteriorRects[i].Bound, Color.red);
                    return true;
                }
            }

            return false;
        }


        HelperRect GetSmallestBoundsAlignment(HelperRect help, float wallsSeparation, bool checkLimit = true)
        {
            InteriorRects.Add(help);
            HelperRect sourceRect = help;

            // Finding combination with smallest final bounds
            Bounds smallest = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
            Bounds tempB;
            HelperRect smallestR = help;
            HelperRect? connectWith = null;
            float toCenter = float.MaxValue;

            for (int r = 0; r < 2; r++) // Checking rotated and no rotated square
            {
                HelperRect check = help;
                if (r == 1) check.rotated = true; else check.rotated = false;

                for (int i = 0; i < InteriorRects.Count - 1; i++) // Checking all already added interior rects
                {
                    // Ignoring one-door limited rooms 
                    if (InteriorRects[i].CanConnectTo() == false) continue;

                    // Checking below, right, left and above placement connection
                    for (int d = 0; d < 4; d++)
                    {
                        check.pos = GetAlignNextToPosition(wallsSeparation, ref check, InteriorRects[i], (EAlignDir)d);
                        HelperRect tempR = check;

                        for (int sd = -1; sd < 2; sd++)
                        {
                            if (sd > -1)
                            {
                                tempR = check; int refd = 0;
                                if (d < 2)
                                {
                                    refd = 2;
                                    tempR.pos.x = GetAlignInPosition(tempR, InteriorRects[i], (EAlignDir)refd + sd).x;
                                }
                                else
                                    tempR.pos.y = GetAlignInPosition(tempR, InteriorRects[i], (EAlignDir)refd + sd).y;
                            }

                            InteriorRects[InteriorRects.Count - 1] = tempR;

                            if (IsAnyColliding(tempR, wallsSeparation, InteriorRects.Count - 1) == false)
                            {
                                tempB = MeasureBounding(InteriorRects);
                                Bounds roomRect = tempR.Bound;

                                //FDebug.DrawBounds3D(roomRect, Color.red);

                                bool inLimitRange = true;
                                if (checkLimit) inLimitRange = IsInLimitRange(roomRect);

                                if (inLimitRange)
                                {
                                    if (tempB.size.magnitude <= smallest.size.magnitude && tempB.center.magnitude < toCenter)
                                    {
                                        smallestR = tempR;
                                        smallest = tempB;
                                        connectWith = InteriorRects[i];
                                        toCenter = tempB.center.magnitude;
                                    }
                                }
                                else
                                {
                                    //UnityEngine.Debug.Log("out of limit");
                                }

                            }
                            else
                            {
                            }
                        }
                    }
                }
            }

            help = smallestR;



            if (connectWith != null)
            {
                if (help.TypeID == -1 && connectWith.Value.TypeID == -1)
                {
                    // We dont want to create connections between corridors
                }
                else
                {
                    ConnectionRect cn = GetConnectionRect(help, connectWith.Value);
                    if (cn.Found)
                    {
                        bool can = true;
                        if (FGenerators.CheckIfExist_NOTNULL(help)) if (FGenerators.CheckIfExist_NOTNULL(help.SettingsRef)) if (help.SettingsRef.DoorConnectionsCount.Max <= 0) can = false;
                        
                        if (can)
                        {
                            cn.Id = ConnectionRects.Count;
                            ConnectionRects.Add(cn);

                            help.Connections.Add(cn);
                            connectWith.Value.Connections.Add(cn);
                        }

                        help.HelperBool = true;
                    }
                    else
                    {
                        Debug.Log("Not found door connection!");
                        help.HelperBool = false;
                    }
                }
            }
            else // If connection not found then we will try finding them after all other rooms creation
            {
                help.HelperBool = false;
            }

            //if ( connectWith != null) Debug.DrawLine(help.Bound.center * 2f, connectWith.Value.Bound.center * 2f, Color.green, 1.1f);

            InteriorRects.RemoveAt(InteriorRects.Count - 1);
            return help;
        }


        internal void GetConnectedRects(int id, List<int> connectedIds, List<int> connectionsIds)
        {
            for (int c = 0; c < ConnectionRects.Count; c++)
            {
                if (ConnectionRects[c].Connection1.TypeID == id)
                {
                    var conn = ConnectionRects[c];
                    int idd = conn.Connection2.TypeID;
                    if (connectionsIds.Contains(conn.Id) == false) connectionsIds.Add(conn.Id);

                    if (idd != -1)
                    {
                        if (connectedIds.Contains(idd) == false)
                        {
                            connectedIds.Add(idd);
                            GetConnectedRects(idd, connectedIds, connectionsIds);
                        }
                    }
                }
                else if (ConnectionRects[c].Connection2.TypeID == id)
                {
                    var conn = ConnectionRects[c];
                    int idd = conn.Connection1.TypeID;
                    if (connectionsIds.Contains(conn.Id) == false) connectionsIds.Add(conn.Id);

                    if (idd != -1)
                    {
                        if (connectedIds.Contains(idd) == false)
                        {
                            connectedIds.Add(idd);
                            GetConnectedRects(idd, connectedIds, connectionsIds);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removing rect from interior list and removing all related connections
        /// </summary>
        internal void RemoveRectWithIDAndAllConnectedChildren(int id)
        {
            List<int> connectedIds = new List<int>();
            List<int> connectionsIds = new List<int>();

            GetConnectedRects(id, connectedIds, connectionsIds);

            if (connectedIds.Contains(id) == false) connectedIds.Add(id);

            for (int i = 0; i < connectedIds.Count; i++)
            {
                for (int r = InteriorRects.Count - 1; r >= 0; r--)
                {
                    if (InteriorRects[r].TypeID == connectedIds[i]) InteriorRects.RemoveAt(r);
                }
            }

            for (int i = 0; i < connectionsIds.Count; i++)
            {
                for (int cn = ConnectionRects.Count - 1; cn >= 0; cn--)
                {
                    if (ConnectionRects[cn].Id == connectionsIds[i]) ConnectionRects.RemoveAt(cn);
                }
            }
        }

        public ConnectionRect GetConnectionRect(HelperRect r1, HelperRect r2)
        {
            ConnectionRect conn = new ConnectionRect();
            conn.Connection1 = r1;
            conn.Connection2 = r2;

            Bounds myB = r1.Bound;
            Bounds othB = r2.Bound;

            bool found = false;
            Bounds testPosBound = new Bounds(new Vector3(myB.center.x, 0, myB.max.z), new Vector3(myB.size.x, myB.size.y, 1) * 0.99f); // upper
            if (testPosBound.Intersects(othB))
            {
                if (Truncate(myB.min.x) == Truncate(othB.min.x))
                {
                    //UnityEngine.Debug.Log("UP " + r1.ColorID + " >> " + r2.ColorID);
                    conn.direction = EAlignDir.Up;
                    found = true;
                }
            }

            if (!found)
            {
                testPosBound = new Bounds(new Vector3(myB.center.x, 0, myB.min.z), new Vector3(myB.size.x, myB.size.y, 1) * 0.99f); // lower
                if (testPosBound.Intersects(othB))
                {
                    if (Truncate(myB.min.x) == Truncate(othB.min.x))
                    {
                        //UnityEngine.Debug.Log("D " + r1.ColorID + " >> " + r2.ColorID);
                        conn.direction = EAlignDir.Down;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                testPosBound = new Bounds(new Vector3(myB.max.x, 0, myB.center.z), new Vector3(1, myB.size.y, myB.size.z) * 0.99f); // right
                if (testPosBound.Intersects(othB))
                {
                    if (Truncate(myB.min.z) == Truncate(othB.min.z))
                    {
                        //UnityEngine.Debug.Log("R " + r1.ColorID + " >> " + r2.ColorID);
                        conn.direction = EAlignDir.Right;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                testPosBound = new Bounds(new Vector3(myB.min.x, 0, myB.center.z), new Vector3(1, myB.size.y, myB.size.z) * 0.99f); // left
                if (testPosBound.Intersects(othB))
                {
                    if (Truncate(myB.min.z) == Truncate(othB.min.z))
                    {
                        //UnityEngine.Debug.Log("LEFt " + r1.ColorID + " >> " + r2.ColorID);
                        conn.direction = EAlignDir.Left;
                        found = true;
                    }
                }
            }

            //UnityEngine.Debug.Log("Check on: " + conn.direction);
            conn.pos = GetFreeConnectionPosOnSide(r1, r2, conn.direction);
            conn.Found = found;

            conn.directOffset = conn.Bound.center - r1.Bound.center;

            return conn;
        }

        private float Truncate(float value, int decimals = 2)
        {
            if (value < 0f)
                return (float)(System.Math.Round(value, decimals) - System.Math.Ceiling(value));
            else
                return (float)(System.Math.Round(value, decimals) - System.Math.Floor(value));
        }

        public Vector2 GetFreeConnectionPosOnSide(HelperRect targetRect, HelperRect connectWith, EAlignDir direction)
        {
            Bounds bound = targetRect.Bound;
            Vector2 pos = new Vector2(bound.center.x, bound.center.z);

            if (direction == EAlignDir.Right) pos.x = bound.max.x;
            else if (direction == EAlignDir.Left) pos.x = bound.min.x;
            else if (direction == EAlignDir.Up) pos.y = bound.max.z;
            else if (direction == EAlignDir.Down) pos.y = bound.min.z;

            int steps;
            if (direction == EAlignDir.Right || direction == EAlignDir.Left) steps = (int)targetRect.RotatedSize.y;
            else steps = (int)targetRect.RotatedSize.x;

            List<Vector2> freePoses = new List<Vector2>();
            for (int i = 0; i < steps; i++)
            {
                ConnectionRect cn = new ConnectionRect();

                if (direction == EAlignDir.Right || direction == EAlignDir.Left)
                    pos.y = bound.min.z + i + 0.5f;
                else
                    pos.x = bound.min.x + i + 0.5f;

                cn.pos = pos;

                Bounds cBound = cn.Bound;
                cBound.size *= 0.99f;

                //Debug.DrawRay(new Vector3(pos.x, 0, pos.y) * 2, Vector3.up, Color.yellow, 1.1f);

                if (cBound.Intersects(connectWith.Bound) == true) // If Current position intersects with target rect
                {
                    if (ConnectionRects.Count == 0)
                        freePoses.Add(pos);
                    else
                        //for (int c = 0; c < ConnectionRects.Count; c++) // Checking all other existing connection rects
                        //{
                        //    Bounds oConn = ConnectionRects[c].Bound;
                        //    oConn.size *= 0.1f;

                        //if (oConn.Intersects(cBound) == false) // If other connection rects are not colliding with this rect
                        freePoses.Add(pos);
                    //}
                }
            }

            if (freePoses.Count > 0)
            {
                return freePoses[freePoses.Count / 2];
                //return freePoses[FGenerators.GetRandom(0, freePoses.Count)];
            }

            return Vector2.zero;
        }


        public HelperRect? FindNonCollidingAlignmentWith(HelperRect rect, float wallsSeparation, HelperRect alignWith, bool smallestBounds = false)
        {
            HelperRect? resulting = null;

            // Finding combination with smallest final bounds
            Bounds smallest = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
            Bounds tempB;
            float toCenter = float.MaxValue;
            InteriorRects.Add(rect);

            for (int r = 0; r < 2; r++) // No rotated and rotated
            {
                HelperRect check = rect;

                for (int d = 0; d < 4; d++) // Direction check
                {
                    check.pos = GetAlignNextToPosition(ref check, wallsSeparation, alignWith, (EAlignDir)d);
                    HelperRect tempR = check;

                    for (int sd = -1; sd < 2; sd++) // Side align check
                    {
                        if (sd > -1)
                        {
                            tempR = check;
                            int refd = 0;
                            if (d < 2)
                            {
                                refd = 2;
                                tempR.pos.x = GetAlignInPosition(tempR, alignWith, (EAlignDir)refd + sd).x;
                            }
                            else
                                tempR.pos.y = GetAlignInPosition(tempR, alignWith, (EAlignDir)refd + sd).y;
                        }

                        if (smallestBounds)
                        {

                            InteriorRects[InteriorRects.Count - 1] = tempR;
                            if (IsAnyColliding(tempR, wallsSeparation, InteriorRects.Count - 1) == false)
                            {
                                tempB = MeasureBounding(InteriorRects);

                                if (tempB.size.magnitude <= smallest.size.magnitude && tempB.center.magnitude < toCenter)
                                {
                                    resulting = tempR;
                                    smallest = tempB;
                                    toCenter = tempB.center.magnitude;
                                }
                            }

                        }
                        else
                        {
                            if (IsAnyColliding(tempR, wallsSeparation, null) == false) return tempR;
                        }

                    }
                }
            }

            InteriorRects.RemoveAt(InteriorRects.Count - 1);
            return resulting;
        }

        public static Vector2 GetAlignNextToPosition(float wallsSeparation, ref HelperRect rect, HelperRect target, EAlignDir direction)
        {
            Vector2 tgtPos = target.pos;

            if ((int)direction >= 2)
            {
                float signum = direction == EAlignDir.Right ? 1f : -1f;

                float offset = 0;
                offset += (target.RotatedSize.x / 2) * signum;
                if (target.RotatedSize.x % 2 != 0) offset -= .5f;
                offset += (rect.RotatedSize.x / 2) * signum;
                if (rect.RotatedSize.x % 2 != 0) offset += .5f;

                tgtPos.x += offset;

                if (rect.TypeID != -1)
                {
                    rect.separationOffset = new Vector3(signum * wallsSeparation, 0, target.totalSepOffset.z);
                    //rect.separationOffset = target.separationOffset + (Vector3.right/2f) * signum * PlanPreset.WallsSeparation;
                    rect.totalSepOffset = rect.separationOffset + Vector3.right * target.totalSepOffset.x;
                    //rect.totalSepOffset += rect.separationOffset;
                    //rect.totalSepOffset += (Vector3.right) * signum * PlanPreset.WallsSeparation;
                    tgtPos.x += rect.separationOffset.x;
                    tgtPos.y += Mathf.RoundToInt(FGenerators.GetRandom(-target.size.y / 4f, target.size.y / 4f));
                }

            }
            else
            {
                float signum = direction == EAlignDir.Up ? 1f : -1f;

                float offset = 0;
                offset += (target.RotatedSize.y / 2) * signum;
                if (target.RotatedSize.y % 2 != 0) offset -= .5f;
                offset += (rect.RotatedSize.y / 2) * signum;
                if (rect.RotatedSize.y % 2 != 0) offset += .5f;

                tgtPos.y += offset;

                if (rect.TypeID != -1)
                {
                    rect.separationOffset = new Vector3(target.totalSepOffset.x, 0, signum * wallsSeparation);
                    rect.totalSepOffset = rect.separationOffset + Vector3.forward * target.totalSepOffset.z;
                    //rect.totalSepOffset += (Vector3.forward) * signum * PlanPreset.WallsSeparation;
                    tgtPos.y += rect.separationOffset.z;
                    tgtPos.x += Mathf.RoundToInt(FGenerators.GetRandom(-target.size.x / 4f, target.size.x / 4f));
                }
            }

            return tgtPos;
        }

        public Vector2 GetAlignNextToPosition(ref HelperRect rect, float wallsSeparation, HelperRect target, EAlignDir direction)
        {
            return GetAlignNextToPositionRandom(ref rect, target, direction, wallsSeparation);

            //Vector2 tgtPos;

            //if ((int)direction >= 2)
            //{
            //    float signum = direction == EAlignDir.Right ? 1f : -1f;

            //    float offset = 0;
            //    offset += (target.RotatedSize.x / 2) * signum;
            //    if (target.RotatedSize.x % 2 != 0) offset -= .5f;
            //    offset += (rect.RotatedSize.x / 2) * signum;
            //    if (rect.RotatedSize.x % 2 != 0) offset += .5f;

            //    tgtPos.x += offset;

            //    if (rect.TypeID != -1)
            //    {
            //        rect.separationOffset = new Vector3(signum * wallsSeparation, 0, target.totalSepOffset.z);
            //        //rect.separationOffset = target.separationOffset + (Vector3.right/2f) * signum * PlanPreset.WallsSeparation;
            //        rect.totalSepOffset = rect.separationOffset + Vector3.right * target.totalSepOffset.x;
            //        //rect.totalSepOffset += rect.separationOffset;
            //        //rect.totalSepOffset += (Vector3.right) * signum * PlanPreset.WallsSeparation;
            //        tgtPos.x += rect.separationOffset.x;
            //        tgtPos.y += Mathf.RoundToInt(FGenerators.GetRandom(-target.size.y / 4f, target.size.y / 4f));
            //    }

            //}
            //else
            //{
            //    float signum = direction == EAlignDir.Up ? 1f : -1f;

            //    float offset = 0;
            //    offset += (target.RotatedSize.y / 2) * signum;
            //    if (target.RotatedSize.y % 2 != 0) offset -= .5f;
            //    offset += (rect.RotatedSize.y / 2) * signum;
            //    if (rect.RotatedSize.y % 2 != 0) offset += .5f;

            //    tgtPos.y += offset;

            //    if (rect.TypeID != -1)
            //    {
            //        rect.separationOffset = new Vector3(target.totalSepOffset.x, 0, signum * wallsSeparation);
            //        rect.totalSepOffset = rect.separationOffset + Vector3.forward * target.totalSepOffset.z;
            //        //rect.totalSepOffset += (Vector3.forward) * signum * PlanPreset.WallsSeparation;
            //        tgtPos.y += rect.separationOffset.z;
            //        tgtPos.x += Mathf.RoundToInt(FGenerators.GetRandom(-target.size.x / 4f, target.size.x / 4f));
            //    }
            //}

            //return tgtPos;
        }

        public Vector2 GetAlignNextToPositionRandom(ref HelperRect rect, HelperRect target, EAlignDir direction, float wallsSeparation = 0f)
        {
            Vector2 tgtPos = target.pos;

            if ((int)direction >= 2) // Left / right -> move left right
            {
                float signum = direction == EAlignDir.Right ? 1f : -1f;

                float offset = (rect.RotatedSize.y / 2);
                if (rect.RotatedSize.y % 2 != 0) offset += 0.5f * signum;

                if (target.RotatedSize.y % 2 == 0)
                {
                    float tx = target.RotatedSize.y / 2f;
                    if (signum == 1) offset += Mathf.RoundToInt(tx);
                    else offset += Mathf.RoundToInt(tx - 0.75f);
                }
                else
                {
                    float tx = target.RotatedSize.y / 2f;
                    offset += Mathf.RoundToInt(tx - 0.5f);
                }

                if (signum == 1) tgtPos.y = target.pos.y + offset;
                else tgtPos.y = target.pos.y - offset - 1;
                tgtPos.x += Mathf.RoundToInt(FGenerators.GetRandom(-target.size.x / 4f, target.size.x / 4f));


                if (rect.TypeID != -1)
                {
                    rect.separationOffset = new Vector3(target.totalSepOffset.x, 0, signum * wallsSeparation);
                    rect.totalSepOffset = rect.separationOffset + Vector3.forward * target.totalSepOffset.z;
                    tgtPos.y += rect.separationOffset.z;
                }

            }
            else // horizontal stripes, signum == 1 -> from the right -> move up/down
            {
                float signum = direction == EAlignDir.Up ? 1f : -1f;

                float offset = (rect.RotatedSize.x / 2);
                if (rect.RotatedSize.x % 2 != 0) offset += 0.5f * signum;

                if (target.RotatedSize.x % 2 == 0)
                {
                    float tx = target.RotatedSize.x / 2f;
                    if (signum == 1) offset += Mathf.RoundToInt(tx);
                    else offset += Mathf.RoundToInt(tx - 0.75f);
                }
                else
                {
                    float tx = target.RotatedSize.x / 2f;
                    offset += Mathf.RoundToInt(tx - 0.5f);
                }

                if (signum == 1) tgtPos.x = target.pos.x + offset;
                else tgtPos.x = target.pos.x - offset - 1;

                tgtPos.y += Mathf.RoundToInt(FGenerators.GetRandom(-target.size.y / 4f, target.size.y / 4f));


                if (rect.TypeID != -1)
                {
                    rect.separationOffset = new Vector3(signum * wallsSeparation, 0, target.totalSepOffset.z);
                    rect.totalSepOffset = rect.separationOffset + Vector3.right * target.totalSepOffset.x;
                    tgtPos.x += rect.separationOffset.x;
                }

            }

            return tgtPos;
        }


        public Vector2 GetAlignInPosition(HelperRect rect, HelperRect target, EAlignDir direction)
        {
            Vector2 tgtPos = target.pos;

            if ((int)direction >= 2)
            {
                float dir = direction == EAlignDir.Right ? 1f : -1f;

                float offset = 0;
                offset += target.RotatedSize.x / 2 * dir;
                if (target.RotatedSize.x % 2 != 0) offset -= .5f;
                offset -= rect.RotatedSize.x / 2 * dir;
                if (rect.RotatedSize.x % 2 != 0) offset += .5f;

                tgtPos.x += offset;
            }
            else
            {
                float dir = direction == EAlignDir.Up ? 1f : -1f;

                float offset = 0;
                offset += target.RotatedSize.y / 2 * dir;
                if (target.RotatedSize.y % 2 != 0) offset -= .5f;
                offset -= rect.RotatedSize.y / 2 * dir;
                if (rect.RotatedSize.y % 2 != 0) offset += .5f;

                tgtPos.y += offset;
            }

            return tgtPos;
        }

    }

}
