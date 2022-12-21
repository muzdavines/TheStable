using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    [CreateAssetMenu(fileName = "BuildingPlan_", menuName = "FImpossible Creations/Procedural Generation/Legacy/Building Plan (Legacy)", order = 4200)]
    public class BuildPlanPreset : ScriptableObject
    {
        public int Separating = 1;

        // Building plan interiors list
        public List<SingleInteriorSettings> Settings = new List<SingleInteriorSettings>();
        /// <summary> Returns 'Settings' list just with different variable name </summary>
        public List<SingleInteriorSettings> InteriorSettings { get { return Settings; } }

        public SingleInteriorSettings RootChunkSetup = new SingleInteriorSettings();
        /// <summary> Returns 'RootChunkSetup' list just with different variable name </summary>
        public SingleInteriorSettings CorridorSetup { get { return RootChunkSetup; } }

        [HideInInspector] public bool _EditorAdvancedBuildingFoldout = false;

        [Tooltip("Restrictions to be assigned onto rooms cells which have walls on both sides (useful to prevent spawning window wall/extensive wall when on opposite side is room!)")]
        public SpawnRestriction NeightbourWallsCellsRestrictions;
        [Tooltip("Restrictions to be assigned onto rooms cells which have walls on both sides but with some space, bigger than neightbour cells (useful to prevent spawning window wall when on opposite side is room!)")]
        public SpawnRestriction CounterWallsCellsRestrictions;
        [Tooltip("Restrictions to be assigned onto rooms cells which are outside building walls")]
        public SpawnRestriction OutsideWallsCellsRestrictions;

        public int GetToGenerateInteriorsCount()
        {
            int countOfRooms = 0;
            for (int i = 0; i < InteriorSettings.Count; i++) countOfRooms += InteriorSettings[i].Duplicates;
            return countOfRooms;
        }

        public InstructionDefinition GetDefinition(SpawnRestriction r, SingleInteriorSettings interSettings)
        {
            InstructionDefinition def = new InstructionDefinition();

            if (r.CustomDefinition.InstructionType != InstructionDefinition.EInstruction.None)
            {
                return r.CustomDefinition;
            }
            else
            {
                if (!r.UseRestrictSpawnForTags)
                {
                    for (int i = 0; i < interSettings.FieldSetup.CellsInstructions.Count; i++)
                    {
                        var instr = interSettings.FieldSetup.CellsInstructions[i];
                        if (instr.Title.ToLower().Contains(r.UsePresetsDefsByName.ToLower()))
                            return instr;
                    }
                }

                def.InstructionType = InstructionDefinition.EInstruction.PreventSpawnSelective;
                def.Tags = r.RestrictSpawnForTags;
                def.Title = "Custom";
            }

            return def;
        }

        public FieldSetup GetFieldSetupOfRoom(string roomName)
        {
            for (int i = 0; i < Settings.Count; i++)
            {
                if (Settings[i].CustomName == roomName)
                {
                    return Settings[i].FieldSetup;
                }
            }

            return null;
        }


        public bool _editorFoldout = true;
        public bool _editorRootFoldout = true;
        public int _editorSelected = 0;

        public Color GetIDColor(int id, float alpha)
        {
            if (id == -1) return new Color(1f, 1f, 1f, alpha);
            Color col = Color.HSVToRGB(id * (1f / (float)Settings.Count), 0.5f, 0.5f);
            col.a = alpha;
            return col;
        }

    }

}