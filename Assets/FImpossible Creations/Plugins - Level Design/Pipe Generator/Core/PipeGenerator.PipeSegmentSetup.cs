using System;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class PipeSegmentSetup
    {
        [Tooltip("Disabling this segment for the pipe generating algorithm")]
        public bool Enabled = true;
        [Tooltip("Prefab of pipe segment to be spawned")]
        public GameObject Prefab;
        //public OStamperSet OptionalStamper;

        [Space(4)] [Tooltip("For prioritizing using this segment over others (adding this value to distance to desired position)\nIt's pretty important to set cost higher for curve-segments to avoid creating path just out of curve segments!")] 
        public float UseCost = 0f;

        [NonSerialized] public Vector3 ModelForward = Vector3.forward;
        [NonSerialized] public Vector3 ModelUpAxis = Vector3.up;
        [Tooltip("If segments can be rotated only by 90 degrees steps or more")]
        [Range(0,270)] public int AllowRotationYAxisCheckPer = 90;
        [Tooltip("If segments can be rotated only by 90 degrees steps or more")]
        [Range(0,270)] public int AllowRotationZAxisCheckPer = 90;
        //[Tooltip("If you don't for example curve segments to be use every segment but with separation\n\nWhen = 1 then this segment must wait for other segment to be spawned to be used again, when = 2 two other type segments must be spawned for this segment to be used again")]
        //public int AllowUsePerSegments = 0;
        [Tooltip("Not allowing algorithm to use this segment in the first steps of creating segments-path towards target point")]
        public int CanBeUsedSinceIteration = 0;

        [Tooltip("Very important join points which have to customized for your segment model")]
        [Space(5)] public JoinPoint[] JoinPoints = new JoinPoint[1];


        //[Space(5)]
        //public Vector3 up = Vector3.up;
        //public Vector3 forward = Vector3.forward;

        //[Space(5)]
        //public bool AutoParams = true;
        //public float length = 0.2f;
        //public float thickness = 0.2f;

        [HideInInspector] public float ReferenceScale = 0.25f;
        [HideInInspector] public Mesh PreviewMesh;

        public PipeSegmentSetup Copy()
        {
            PipeSegmentSetup cpy = (PipeSegmentSetup)MemberwiseClone();
            return cpy;
        }

        public void Refresh()
        {
            SetPrefab(Prefab);
            ReferenceScale = PreviewMesh.bounds.size.magnitude;

            //if (AutoParams)
            //{
            //    if (PreviewMesh)
            //    {
            //        length = Vector3.Scale(PreviewMesh.bounds.max, forward).magnitude;
            //        length += Vector3.Scale(PreviewMesh.bounds.min, forward).magnitude;

            //        thickness = PreviewMesh.bounds.max.x * 1.5f;
            //    }
            //}
        }

        public void SetPrefab(GameObject pf)
        {
            Prefab = pf;

            if (pf == null) return;
            MeshFilter f = pf.GetComponent<MeshFilter>();
            if (!f) f = pf.GetComponentInChildren<MeshFilter>();
            if (f) PreviewMesh = f.sharedMesh;
        }

        [System.Serializable]
        public class JoinPoint
        {
            public Vector3 origin = Vector3.zero;
            public Vector3 outAxis = Vector3.forward;
        }

        public float BoundsSizeOnAxis(Vector3 normalized)
        {
            return Vector3.Scale(PreviewMesh.bounds.size, normalized).magnitude;
        }
    }
}