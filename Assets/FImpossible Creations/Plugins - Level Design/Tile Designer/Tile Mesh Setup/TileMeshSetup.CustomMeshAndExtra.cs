using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class TileMeshSetup
    {
        public enum EExtraMesh { CustomMesh, CableGenerator }
        public EExtraMesh ExtraMesh = EExtraMesh.CustomMesh;

        public Mesh CustomMesh = null;


        void CustomAndExtraQuickUpdate()
        {

        }

    }
}