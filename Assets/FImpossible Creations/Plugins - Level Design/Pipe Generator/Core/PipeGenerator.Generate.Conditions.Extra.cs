using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        protected virtual bool AllowWithExtraConditions(PipeSpawnData spawn, Vector3 snapPos, PipeSegmentSetup seg, PipeSegmentSetup.JoinPoint join, Vector3 startDir, Quaternion rot)
        {
            bool allow = AllowCheckHolderCondition(spawn, snapPos, rot);
            if (!allow) return false;
            return true;
        }

        bool AllowCheckHolderCondition(PipeSpawnData spawn, Vector3 snapPos, Quaternion rotation)
        {
            if (HoldMask == 0) return true;

            for (int i = 0; i < HoldDirections.Length; i++)
            {
                if (Physics.Raycast(snapPos, rotation * HoldDirections[i], MinimalDistanceToHoldMask, HoldMask, QueryTriggerInteraction.Ignore))
                    return true;
            }

            return false;
        }
    }
}
