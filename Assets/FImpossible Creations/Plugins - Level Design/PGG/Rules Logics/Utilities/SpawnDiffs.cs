using System.Collections.Generic;

namespace FIMSpace.Generating
{
    public struct SpawnDiffs
    {
        public FieldCell Cell;

        public List<SpawnData> SpawnsBackup;

        public List<SpawnData> ToSpawn;
        public List<SpawnData> ToDestroy;
    }
}
