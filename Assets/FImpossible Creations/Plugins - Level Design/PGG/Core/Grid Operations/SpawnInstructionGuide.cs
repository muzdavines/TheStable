using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class SpawnInstructionGuide
    {
        [NonSerialized] public FieldCell HelperCellRef;
        public Vector3Int pos;
        public Quaternion rot;
        public bool WorldRot = false;
        public int Id;
        public InstructionDefinition CustomDefinition;
        public bool UseDirection = false;

        internal SpawnInstruction GenerateGuide(FieldSetup preset, SpawnRestrictionsGroup group)
        {
            SpawnInstruction i = new SpawnInstruction();

            if (CustomDefinition == null)
            {
                i.definition = group.Restriction.GetSpawnInstructionDefinition(preset);
            }
            else
            {
                i.definition = CustomDefinition;
            }

            i.gridPosition = pos;

            i.desiredDirection = PGGUtils.V3toV3Int(rot * Vector3.forward);

            i.useDirection = UseDirection;

            return i;
        }

        public void DrawHandles(Matrix4x4 mx)
        {
#if UNITY_EDITOR
            if (UseDirection)
            {
                Vector3 origin = pos + Vector3.up * 0.05f;
                Vector3 dir;

                if (WorldRot)
                    dir = (mx.inverse.rotation * rot) * Vector3.forward;
                else
                    dir = rot * Vector3.forward;

                Vector3 rotDir = PGGUtils.GetRotatedFlatDirectionFrom(dir.V3toV3Int());

                Color preH = Handles.color;

                Handles.color = preH * 2f;
                Vector3 arrowTip = origin + dir * 0.45f;
                Handles.DrawLine(origin + dir * 0.1f, arrowTip);
                Handles.DrawLine(arrowTip, origin + dir * 0.3f + rotDir * 0.08f);
                Handles.DrawLine(arrowTip, origin + dir * 0.3f - rotDir * 0.08f);
                Handles.color = preH;
            }
#endif
        }

    }
}