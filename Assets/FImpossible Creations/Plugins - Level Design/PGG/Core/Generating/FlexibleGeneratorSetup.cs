using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public class FlexibleGeneratorSetup
    {
        [Tooltip("Helper parent object to et dirty for editor purposes")]
        public Object ParentObject;
        /// <summary> Target FieldSetup for generating (project file) </summary>
        public FieldSetup FieldPreset;
        /// <summary> Instante of field setup to avoid changes in project file </summary>
        public FieldSetup RuntimeFieldSetup { get { return Preparation.RuntimeFieldSetup; } }
        /// <summary> Initial preparation settings for the FieldSetup to be generated </summary>
        public GeneratingPreparation Preparation = null;
        /// <summary> Controlling grid cells updating with FieldSetup's rules </summary>
        public CellsController CellsController;
        /// <summary> Instantiating manager for controlled generating objects on the scene </summary>
        public InstantiatedFieldInfo InstantiatedInfo;

        /// <summary> Forwarded from PGGFlexibleGeneratorBase </summary>
        public FieldSetupComposition Composition { get; private set; }

        internal void Initialize(MonoBehaviour g, FieldSetupComposition compos)
        {
            ParentObject = g;
            Composition = compos;

            if (Preparation == null)
            {
                Preparation = new GeneratingPreparation();
                if (CellsController == null) CellsController = new CellsController();
                if (InstantiatedInfo == null) InstantiatedInfo = new InstantiatedFieldInfo();

                Preparation.Initialize(this);
                CellsController.Initialize(this);
                InstantiatedInfo.Initialize(this);
                InstantiatedInfo.SetupContainer(g.transform);
            }

            RefreshReferences(g);
        }

        public void RefreshReferences(MonoBehaviour g)
        {
            ParentObject = g;

            if (Preparation == null) Preparation = new GeneratingPreparation();
            if (CellsController == null) CellsController = new CellsController();
            if (InstantiatedInfo == null) InstantiatedInfo = new InstantiatedFieldInfo();

            Preparation.RefreshReferences(this);
            CellsController.RefreshReferences(this);
            InstantiatedInfo.RefreshReferences(this);
            InstantiatedInfo.SetupContainer(g.transform);
            RefreshRuntimeFieldSetup();
        }

        public void RefreshRuntimeFieldSetup()
        {
            if (FieldPreset != null)
            {
                if (Composition != null && Composition.Prepared && Composition.OverrideEnabled)
                {
                }
                else
                {
                    if (RuntimeFieldSetup == null || RuntimeFieldSetup.InstantiatedOutOf != FieldPreset)
                        Preparation.ReGenerateRuntimeFieldSetup();
                }
            }
        }

    }
}