using System.Collections.Generic;

namespace FIMSpace.Generating
{
    [System.Serializable]
    public struct SpawnRestrictionsGroup
    {
        public List<SpawnInstructionGuide> Cells;
        public SpawnRestriction Restriction;

        public SpawnRestrictionsGroup(SpawnRestriction restriction)
        {
            Restriction = restriction;
            Cells = new List<SpawnInstructionGuide>();
        }        
    }
}