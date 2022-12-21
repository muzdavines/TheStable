using UnityEngine;

namespace FIMSpace.Generating
{
    /// <summary>
    /// Fundamental root component for accessing universal variables used by PGG System
    /// </summary>
    public abstract class PGGGeneratorRoot : MonoBehaviour
    {
        public abstract FieldSetup PGG_Setup { get; }
        public abstract FGenGraph<FieldCell, FGenPoint> PGG_Grid { get; }
    }
}