using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        bool IsFittingTo(Quaternion rot, Vector3 startDir, Vector3 newJointOutDirection, float tolerance = 1f)
        {
            Vector3 jDir = -(rot * newJointOutDirection).normalized;
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

    }
}
