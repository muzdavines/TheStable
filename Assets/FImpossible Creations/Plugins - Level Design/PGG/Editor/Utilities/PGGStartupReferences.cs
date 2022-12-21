using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Hidden
{
    //[CreateAssetMenu(fileName = "PGGStartudRefs", menuName = "ScriptableObjects/PGGStartudRefs", order = 1)]
    public sealed class PGGStartupReferences : ScriptableObject
    {
        public Object DemosPackage;
        public Object QuickStartFile;
        public Object ManualFile;
        public Object PGGdirectory;
        public Object FSDraftsdirectory;
        public Object FSModPackDraftsDirectory;
        public Object FieldPlannerDraftsDirectory;
        public Texture2D StartupImage;
    }
}
