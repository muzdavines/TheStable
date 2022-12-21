using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {

        bool CheckSegmentAvailableOnJoinPoint(PipeSegmentSetup.JoinPoint join, Vector3 startDir, Quaternion rot, float tolerance = 1f)
        {
            Vector3 jDir = -(rot * join.outAxis).normalized;
            if (tolerance >= 1f)
            {
                if (startDir == jDir) return true;
            }
            else
            {

                float dot = Vector3.Dot(startDir, jDir);
                return dot >= tolerance;
            }

            return false;
        }


        bool IsTargetInRange(Vector3 joinPoint, Vector3 targetPosition)
        {
            Vector3 snapOutPoint = joinPoint;
            float distanceToTarget = Vector3.Distance(snapOutPoint, targetPosition);

            float refScale = MaxDistanceToEnding;
            float rangeFactor = distanceToTarget - refScale;

            if (rangeFactor < refScale) return true;

            return false;
        }


    }
}
