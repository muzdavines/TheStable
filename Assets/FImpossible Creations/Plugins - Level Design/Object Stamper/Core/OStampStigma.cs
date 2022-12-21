using UnityEngine;

namespace FIMSpace.Generating
{
    [AddComponentMenu("")]
    public class OStampStigma : MonoBehaviour
    {
        public OStamperSet ReferenceSet;
        [HideInInspector] public ObjectStampEmitterBase Emitter;
        public ObjectStamperEmittedInfo EmitInfo;
    }
}