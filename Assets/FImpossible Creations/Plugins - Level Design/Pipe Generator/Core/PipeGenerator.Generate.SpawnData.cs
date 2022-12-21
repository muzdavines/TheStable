using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        public class PipeSpawnData
        {
            public PipeSegmentSetup ToCreate;
            public Mesh PreviewMesh;

            public PipeSegmentSetup ParentSegment;
            public PipeSegmentSetup.JoinPoint Join;

            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale = Vector3.one;

            public PipeSpawnData Child;
            public PipeSpawnData Parent;

            public enum ESpawnState { Free, Wrong }
            public ESpawnState State = ESpawnState.Free;

            public Vector3 TransformVector(Vector3 offset)
            {
                return Rotation * Vector3.Scale(Scale, offset);
            }

            public Vector3 TransformPoint(Vector3 offset)
            {
                return Position + TransformVector(offset);
            }

            public Vector3 OutJoinPoint { get { return TransformPoint(Join.origin); } }
            public Vector3 OutJoinPointReverse { get { return TransformPoint(-Join.origin); } }
            public Vector3 JoinOutDir { get { return Rotation * Join.outAxis; } }
            public Bounds RotatedBounds { get { return RotateBounds(Rotation, PreviewMesh.bounds);  } }

            public bool Enabled = true;

            public Bounds RotateBounds(Quaternion rotation, Bounds b)
            {
                Matrix4x4 rot = Matrix4x4.Rotate(rotation);

                Bounds newB = new Bounds();
                Vector3 fr1 = rot.MultiplyPoint( new Vector3(b.max.x, b.min.y, b.max.z) );
                Vector3 br1 = rot.MultiplyPoint(new Vector3(b.max.x, b.min.y, b.min.z) );
                Vector3 bl1 = rot.MultiplyPoint(new Vector3(b.min.x, b.min.y, b.min.z) );
                Vector3 fl1 = rot.MultiplyPoint(new Vector3(b.min.x, b.min.y, b.max.z) );
                newB.Encapsulate(fr1);
                newB.Encapsulate(br1);
                newB.Encapsulate(bl1);
                newB.Encapsulate(fl1);

                Vector3 fr = rot.MultiplyPoint(new Vector3(b.max.x, b.max.y, b.max.z) );
                Vector3 br = rot.MultiplyPoint(new Vector3(b.max.x, b.max.y, b.min.z) );
                Vector3 bl = rot.MultiplyPoint(new Vector3(b.min.x, b.max.y, b.min.z) );
                Vector3 fl = rot.MultiplyPoint(new Vector3(b.min.x, b.max.y, b.max.z) );
                newB.Encapsulate(fr);
                newB.Encapsulate(br);
                newB.Encapsulate(bl);
                newB.Encapsulate(fl);

                return newB;
            }

            public Bounds ScaledBounds()
            {
                Bounds b = PreviewMesh.bounds;
                b.size = Vector3.Scale(b.size, Scale);
                return b;
            }

            public void SetToCreate(PipeSegmentSetup toCreate)
            {
                ToCreate = toCreate;
                if (toCreate == null) return;
                PreviewMesh = toCreate.PreviewMesh;
            }
        }
    }
}
