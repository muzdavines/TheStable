using FIMSpace.Generating.Checker;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning
{
    // Settings class
    [System.Serializable]
    public class SingleInteriorSettings
    {
        public string CustomName = "";
        public int ID = 0;

        public FieldSetup FieldSetup;
        public int Duplicates = 1;
        public int DoorHoleCommandID = 0;
        public bool CenterFitDoor = false;
        public int GetCenterRange() { if (CenterFitDoor) return 5; else return 0; }

        [Space(4)]
        public bool JustOneDoor = false;
        public MinMax DoorConnectionsCount = new MinMax(2, 3);

        [Space(4)]
        //public Vector2Int Size = new Vector2Int(3, 2);
        public GenerationShape.GenerationSetup InternalSetup;
        public GenerationShape OptionalShapePreset;

        [Space(4)]
        public List<InjectionSetup> InjectMods;
        [Tooltip("Making room area filled with string data which can be used to restrict position for other rooms")]
        public string SpreadCheckerData = "";
        public List<string> NotAllowDoorConnectionWithNamed = new List<string>();
        public List<string> OverrideAllowDoorConnectionWithNamed = new List<string>();

        public enum ETargetType { GuideStart, GuideEnd }
        public bool RestrictPosition = false;
        [Tooltip("Use comma ',' for multiple restrictions")]
        public string DistanceToCheckerData = "Start Area";
        //public ETargetType DistanceTo = ETargetType.GuideStart;
        public enum EDistanceRule { Lower, Greater }
        public EDistanceRule DistanceMustBe = EDistanceRule.Greater;
        public float DistanceUnits = 0;

        #region Utilities

        [HideInInspector] public bool _editorAdvancedFoldout = false;

        public SingleInteriorSettings Copy()
        {
            SingleInteriorSettings cpy = (SingleInteriorSettings)MemberwiseClone();
            cpy.InternalSetup = new GenerationShape.GenerationSetup();

            cpy.InjectMods = new List<InjectionSetup>();

            if (InjectMods == null)
            {
                InjectMods = new List<InjectionSetup>();
            }
            else
            {
                for (int i = 0; i < InjectMods.Count; i++) cpy.InjectMods.Add(InjectMods[i]);
            }

            return cpy;
        }

        public string GetName()
        {
            if (CustomName == "")
            {
                if (FieldSetup != null) return FieldSetup.name; else return "Null";
            }
            else
                return CustomName;
        }

        #endregion

        public int GetIdIndex(BuildPlanPreset from)
        {
            for (int i = 0; i < from.Settings.Count; i++) if (from.Settings[i] == this) return i;
            return 0;
        }

        public CheckerField GetChecker(bool center)
        {
            if (OptionalShapePreset != null)
                return OptionalShapePreset.GetChecker(center);
            else
            {
                CheckerField checker = new CheckerField();
                checker.SetSize(InternalSetup.RectSetup.Width.Max, InternalSetup.RectSetup.Height.Max, center);
                //checker.SetSize(Size.x, Size.y, false);
                return checker;
            }
        }

        public InstantiatedFieldInfo GenerateOnGrid(FGenGraph<FieldCell, FGenPoint> grid, List<SpawnInstruction> guides, Transform parent, Vector3 offset)
        {
            if (FieldSetup == null)
            {
                Debug.LogError("No assigned Field Setup in " + GetName() + "!");
                return new InstantiatedFieldInfo();
            }

            return IGeneration.GenerateFieldObjectsWithContainer(GetName(), FieldSetup, grid, parent, guides, InjectMods, offset);
        }

        public InstantiatedFieldInfo GenerateOnGrid(GridPlanGeneratingHelper helper, Transform parent)
        {
            return IGeneration.GenerateFieldObjectsWithContainer(GetName(), FieldSetup, helper.grid, parent, helper.guides, InjectMods);
        }


        public bool CheckIfRestrictionAllows(CheckerField thisChecker, CheckerField other, bool checkPrecise = false)
        {
            if (RestrictPosition == false) return true;
            if (string.IsNullOrEmpty(DistanceToCheckerData)) return true;
            int dist = Mathf.RoundToInt(DistanceUnits);
            if (dist <= 0) return true;

            string[] toCheck = DistanceToCheckerData.Split(',');

            for (int i = 0; i < toCheck.Length; i++)
            {
                string check = toCheck[i];

                if (DistanceMustBe == SingleInteriorSettings.EDistanceRule.Greater)
                {
                    // If checker finds squares with target data, that means distance is lower then disallow
                    if (thisChecker.CheckerDataInRange(other, dist, check, checkPrecise)) return false;
                }
                else
                {
                    // If checker finds squares with target data, that means distance is lower so allow
                    if (thisChecker.CheckerDataInRange(other, dist, check, checkPrecise)) return true;
                }
            }

            // Distance must be lower but any checker in range not found
            if (DistanceMustBe == EDistanceRule.Lower) return false;
            return true;
        }


        public bool CheckIfRestrictionAllows(CheckerField thisChecker, List<CheckerField> other, bool checkPrecise = false)
        {
            if (RestrictPosition == false) return true;
            if (string.IsNullOrEmpty(DistanceToCheckerData)) return true;
            int dist = Mathf.RoundToInt(DistanceUnits);
            if (dist <= 0) return true;

            string[] toCheck = DistanceToCheckerData.Split(',');
            for (int i = 0; i < toCheck.Length; i++)
            {
                string check = toCheck[i];

                if (DistanceMustBe == SingleInteriorSettings.EDistanceRule.Greater)
                {
                    // If checker finds squares with target data, that means distance is lower then disallow
                    if (thisChecker.CheckerDataInRange(other, dist, check, checkPrecise)) return false;
                }
                else
                {
                    // If checker finds squares with target data, that means distance is lower so allow
                    if (thisChecker.CheckerDataInRange(other, dist, check, checkPrecise)) return true;
                }
            }


            if (DistanceMustBe == EDistanceRule.Lower) return false;
            return true;
        }

        public bool CheckIfRestrictionAllows(CheckerField thisChecker, List<BuildPlanInstance> other, bool checkPrecise = false)
        {
            if (RestrictPosition == false) return true;
            if (string.IsNullOrEmpty(DistanceToCheckerData)) return true;

            int dist = Mathf.RoundToInt(DistanceUnits);
            if (DistanceUnits <= 0) return true;

            string[] toCheck = DistanceToCheckerData.Split(',');
            for (int i = 0; i < toCheck.Length; i++)
            {
                string check = toCheck[i];

                for (int oi = 0; oi < other.Count; oi++)
                {
                    if (DistanceMustBe == EDistanceRule.Greater)
                    {
                        // If checker finds squares with target data, that means distance is lower
                        if (thisChecker.CheckerDataInRange(other[oi].Checker, dist, check, checkPrecise)) return false;
                    }
                    else
                    {
                        // If checker finds squares with target data, that means distance is lower
                        if (thisChecker.CheckerDataInRange(other[oi].Checker, dist, check, checkPrecise))
                        {
                            //if (check == "BossRoom") UnityEngine.Debug.Log("jooojejst " + other[oi].SettingsReference.GetName());
                            return true;
                        }
                    }
                }

            }

            if (DistanceMustBe == EDistanceRule.Lower) return false;
            return true;
        }

    }


    [System.Serializable]
    public class PlanCorridorSettings
    {
        public FieldSetup FieldSetup;
        public MinMax BranchLength = new MinMax(3, 5);
        public MinMax TargetBranches = new MinMax(1, 2);
        public MinMax CellsSpace = new MinMax(1, 1);
    }

}