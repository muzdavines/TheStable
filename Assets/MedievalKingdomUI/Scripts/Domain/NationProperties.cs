using System;
using UnityEngine;

namespace MedievalKingdomUI.Scripts.Domain
{
    [Serializable]
    public class NationProperties
    {
        public Nation nation;
        public string name;
        public GameObject character;
        public GameObject descriptionEmblem;
    }
}