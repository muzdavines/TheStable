using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Same as SimpleFieldGenerator but with PGGGeneratorBase inheritance, this component is just example script
    /// </summary>
    [AddComponentMenu("FImpossible Creations/PGG/Simple Field Generator - Clean", 101)]
    public class SimpleFieldGenerator_GenImplemented : PGGGeneratorBase
    {
        [Space(3)]
        public FieldSetup FieldPreset;
        public Vector3Int FieldSizeInCells = new Vector3Int(5, 0, 4);
        public bool CenterOrigin = false;

        public void SetGuides(List<SpawnInstruction> guides) { this.guides = guides; }
        private List<SpawnInstruction> guides = null;

        public override FGenGraph<FieldCell, FGenPoint> PGG_Grid { get { return null; } }
        public override FieldSetup PGG_Setup { get { return null; } }

        public override void GenerateObjects()
        {
            if (FieldPreset == null) // If we didn't assign FieldSetup we can't generate anything
            {
                Debug.Log("Can't generate objects without FieldSetup!");
                return;
            }

            Prepare(); // Prepare seed
            ClearGenerated(); // Cleaning previous generated objects for re-generating


            Vector3Int origin = Vector3Int.zero;

            if (CenterOrigin) origin = new Vector3Int(-FieldSizeInCells.x / 2, 0, -FieldSizeInCells.z / 2);

            Generated.Add(IGeneration.GenerateFieldObjectsRectangleGrid(FieldPreset, FieldSizeInCells, Seed, transform, true, guides, true, origin));


            base.GenerateObjects(); // Triggering event if assigned
        }


        protected override void DrawGizmos()
        {
            if (FieldPreset == null) return;

            Vector3 origin = Vector3.zero;
            if (CenterOrigin) origin = new Vector3(-FieldSizeInCells.x / 2f, 0, -FieldSizeInCells.z / 2f);
            Vector3 presetSize = FieldPreset.GetCellUnitSize();

            for (int x = 0; x < FieldSizeInCells.x; x++)
            {
                for (int y = 0; y <= FieldSizeInCells.y; y++)
                    for (int z = 0; z < FieldSizeInCells.z; z++)
                    {
                        Vector3 genPosition = Vector3.Scale(presetSize, new Vector3(x, y, z)) + origin * presetSize.x;
                        Gizmos.DrawWireCube(genPosition, new Vector3(presetSize.x, presetSize.x * 0.2f, presetSize.x));
                    }
            }
        }

    }


    #region Editor Inspector Window

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(SimpleFieldGenerator_GenImplemented))]
    public class SimpleFieldGenerator_GenImplementedEditor : PGGGeneratorBaseEditor
    {
        protected override void DrawGUIBeforeDefaultInspector()
        {
            GUILayout.Space(3);
            base.DrawGUIBeforeDefaultInspector();
        }

        protected override void DrawGUIFooter()
        {
            GUILayout.Space(7);
            DrawGeneratingButtons(false);
            base.DrawGUIFooter();
        }
    }
#endif

    #endregion

}