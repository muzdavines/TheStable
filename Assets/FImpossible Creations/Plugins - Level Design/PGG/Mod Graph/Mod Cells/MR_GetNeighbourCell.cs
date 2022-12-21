using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Graph;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.ModNodes.Cells
{

    public class MR_GetNeighbourCell : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Get Cell Neighbour" : "Get Cell Neighbour"; }
        public override string GetNodeTooltipDescription { get { return "Get neighbour cell next to provided start cell or self cell.\nYou can rotate the cell offset with 'Check Rotation' port (like connecting spawn rotation for 'direct' check)"; } }
        public override Color GetNodeColor() { return new Color(0.64f, 0.9f, 0.0f, 0.9f); }
        public override bool IsFoldable { get { return true; } }
        public override Vector2 NodeSize { get { return new Vector2(210, _EditorFoldout ? 182 : 124); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }


        [Port(EPortPinType.Output)] public PGGModCellPort ResultCell;
        [HideInInspector] [Port(EPortPinType.Input, 1)] public PGGModCellPort OriginCell;
        [HideInInspector] [Port(EPortPinType.Input, EPortValueDisplay.HideValue)] public PGGVector3Port CheckRotation;
        [HideInInspector] [Port(EPortPinType.Input, EPortValueDisplay.HideValue)] public PGGVector3Port Offset;
        [SerializeField, HideInInspector] private List<Vector3Int> selectedOffsets = new List<Vector3Int>();

        public override void OnStartReadingNode()
        {
            //ResultCell.SetNull();
            if (Offset.IsConnected) Offset.TriggerReadPort(true); else { if (selectedOffsets.Count > 0) Offset.Value = selectedOffsets[0]; }

            OriginCell.TriggerReadPort(true);
            var cell = OriginCell.GetInputCellValue;
            if (FGenerators.IsNull(cell)) if (OriginCell.IsConnected == false) cell = MG_Cell;

            if (FGenerators.IsNull(cell)) return;

            Vector3 offset = Offset.GetInputValue;
            Vector3 rotate = Vector3.zero;
            Quaternion offsetRot = Quaternion.identity;

            if (CheckRotation.IsConnected)
            {
                CheckRotation.TriggerReadPort(true);
                rotate = CheckRotation.GetInputValue;

                if (rotate != Vector3.zero)
                {
                    offsetRot = Quaternion.Euler(rotate);
                    offset = offsetRot * offset;
                }
            }

            var originCell = cell;
            cell = MG_Grid.GetCell(cell.Pos + offset.V3toV3Int(), false);
            ResultCell.ProvideFullCellData(cell);

            if (selectedOffsets.Count > 1)
            {
                List<FieldCell> cells = new List<FieldCell>();

                for (int s = 0; s < selectedOffsets.Count; s++)
                {
                    if (rotate != Vector3.zero)
                        cell = MG_Grid.GetCell(originCell.Pos + (offsetRot * selectedOffsets[s]).V3toV3Int(), false);
                    else
                        cell = MG_Grid.GetCell(originCell.Pos + selectedOffsets[s], false);

                    cells.Add(cell);
                }

                ResultCell.ProvideCellsList(cells);
            }
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            GUILayout.Space(3);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(-8);
            if (!Offset.IsConnected) { if (selectedOffsets.Count > 0) Offset.Value = selectedOffsets[0]; }

            if (Offset.IsConnected)
            { GUILayout.Space(7); EditorGUILayout.LabelField("Using connected port", EditorStyles.centeredGreyMiniLabel); GUILayout.Space(-5); }
            else if (GUILayout.Button(new GUIContent("  Set Offset", PGGUtils.Tex_Selector)))
            { CheckCellsSelectorWindow.Init(selectedOffsets); }

            GUILayout.Space(-1);

            if (selectedOffsets.Count > 1)
                EditorGUILayout.LabelField("Using Multiple Cell Offsets (" + selectedOffsets.Count + ")", EditorStyles.centeredGreyMiniLabel);
            else
                EditorGUILayout.LabelField("Choosed Offset: " + Offset.GetInputValue.V3toV3Int().ToString(), EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);

            base.Editor_OnNodeBodyGUI(setup);
            baseSerializedObject.Update();

            if (_EditorFoldout)
            {
                if (sp == null) sp = baseSerializedObject.FindProperty("OriginCell");
                EditorGUILayout.PropertyField(sp);
                SerializedProperty spc = sp.Copy(); spc.Next(false);
                EditorGUILayout.PropertyField(spc); spc.Next(false);
                EditorGUILayout.PropertyField(spc);

                OriginCell.AllowDragWire = true;
                CheckRotation.AllowDragWire = true;
            }
            else
            {
                OriginCell.AllowDragWire = false;
                CheckRotation.AllowDragWire = false;
            }

            baseSerializedObject.ApplyModifiedProperties();
        }
#endif

    }
}