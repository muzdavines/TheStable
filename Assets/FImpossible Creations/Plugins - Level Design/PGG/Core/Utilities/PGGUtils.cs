using FIMSpace.Generating.Checker;
using FIMSpace.Generating.PathFind;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Rules;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif

namespace FIMSpace.Generating
{
    public static class PGGUtils
    {

        #region Extensions

        public static List<Bounds> GeneratePathFindBounds(this SimplePathGuide guide)
        {
            return SimplePathGuide.GeneratePathFindBounds(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), guide.PathThickness, guide.ChangeDirCost);
        }

        public static List<Bounds> GeneratePathFindBounds(this SimplePathGuide guide, List<Vector2> pathPoints)
        {
            return SimplePathGuide.GeneratePathFindBounds(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), pathPoints, guide.PathThickness, guide.ChangeDirCost);
        }


        public static List<Vector2> GeneratePathFindPoints(this SimplePathGuide guide)
        {
            return SimplePathGuide.GeneratePathFindPoints(guide.Start, guide.End, guide.StartDir.GetDirection2D(), guide.EndDir.GetDirection2D(), guide.ChangeDirCost);
        }

        public static bool Compare(float value, ESR_DistanceRule variableMustBe, float thisValue)
        {
            if (variableMustBe == ESR_DistanceRule.Equal)
            {
                return value == thisValue;
            }
            else if (variableMustBe == ESR_DistanceRule.Greater)
            {
                return value > thisValue;
            }
            else if (variableMustBe == ESR_DistanceRule.Lower)
            {
                return value < thisValue;
            }

            return false;
        }

        public static Vector2 GetProgessPositionOverLines(List<Vector2Int> pathPoints, float progress)
        {
            float fullLength = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
                fullLength += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

            float progressLength = fullLength * progress;

            float checkProgr = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
            {
                float currProgr = checkProgr;
                checkProgr += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

                if (currProgr <= progressLength && checkProgr >= progressLength)
                {
                    float progr = Mathf.InverseLerp(currProgr, checkProgr, progressLength);
                    return Vector2.Lerp(pathPoints[p], pathPoints[p + 1], progr);
                }
            }

            return Vector2.zero;
        }

        public static Vector2 GetDirectionOver(List<Vector2Int> pathPoints, int startId, int endId)
        {
            if (endId < pathPoints.Count)
                return ((Vector2)pathPoints[startId + 1] - (Vector2)pathPoints[startId]).normalized;
            else
                return ((Vector2)pathPoints[startId] - (Vector2)pathPoints[startId - 1]).normalized;
        }

        public static Vector2 GetDirectionOverLines(List<Vector2Int> pathPoints, float progress)
        {
            float fullLength = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
                fullLength += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

            float progressLength = fullLength * progress;

            float checkProgr = 0f;
            for (int p = 0; p < pathPoints.Count - 1; p++)
            {
                float currProgr = checkProgr;
                checkProgr += Vector2.Distance(pathPoints[p], pathPoints[p + 1]);

                if (currProgr <= progressLength && checkProgr >= progressLength)
                {
                    return ((Vector2)pathPoints[p + 1] - (Vector2)pathPoints[p]).normalized;
                }
            }

            return Vector2.zero;
        }

        #endregion


        #region Vectors

        /// <summary> V2Int ToBound V3 </summary>
        public static Vector3 V2toV3Bound(this Vector2Int v, float y = 0f)
        {
            if (y == 0f) return new Vector3(v.x - 0.5f, y, v.y - 0.5f);
            return new Vector3(v.x, y, v.y);
        }

        public static Vector3 V2toV3(this Vector2Int v, float y = 0f)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector3 V2toV3(this Vector2 v, float y = 0f)
        {
            return new Vector3(v.x, y, v.y);
        }

        public static Vector2Int V2toV2Int(this Vector2 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        public static Vector3Int V2toV3Int(this Vector2Int v, int y = 0)
        {
            return new Vector3Int(v.x, y, v.y);
        }

        public static Vector2 V3toV2(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        /// <summary>
        /// Resetting local position, rotation, scale to zero on 1,1,1 (defaults)
        /// </summary>
        public static void ResetCoords(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static Vector2Int V3toV2Int(this Vector3 v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.z));
        }

        public static Vector3Int V3toV3Int(this Vector3 v)
        {
            return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

        public static Vector3Int V3toV3IntC(this Vector3 v)
        {
            return new Vector3Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
        }

        public static Vector3Int V3toV3IntF(this Vector3 v)
        {
            return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }

        public static Vector3 V3IntToV3(this Vector3Int v)
        {
            return new Vector3((float)(v.x), (float)(v.y), (float)(v.z));
        }

        public static Vector2Int V3IntToV2Int(this Vector3Int v)
        {
            return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        /// <summary> Just to avoid errors in unity 2018.4 </summary>
        public static Vector3Int InverseV3Int(this Vector3Int v)
        {
            return new Vector3Int(-v.x, -v.y, -v.z);
        }

        public static int ToInt(this float v)
        {
            return Mathf.RoundToInt(v);
        }


        /// <summary>
        /// If direction is left/right then size is y * cellSize etc.
        /// </summary>
        public static Vector2 GetDirectionalSize(Vector2Int dir, int cellsSize)
        {
            if (cellsSize <= 1) return Vector2.one;
            if (dir.x != 0) return new Vector2(1, cellsSize);
            else return new Vector2(cellsSize, 1);
        }

        /// <summary>
        /// Getting   1,0  -1,0   0,1   0,-1
        /// </summary>
        public static Vector2Int GetFlatDirectionFrom(Vector2Int vect)
        {
            if (vect.x != 0) return new Vector2Int(vect.x, 0);
            else return new Vector2Int(0, vect.y);
        }


        public static Vector3Int GetRandomDirection()
        {
            int r = FGenerators.GetRandom(0, 5);
            if (r == 0) return new Vector3Int(1, 0, 0);
            else if (r == 1) return new Vector3Int(0, 0, 1);
            else if (r == 2) return new Vector3Int(-1, 0, 0);
            else return new Vector3Int(0, 0, -1);
        }


        /// <summary>
        /// Getting   1,0  -1,0   0,1   0,-1
        /// </summary>
        public static Vector2Int GetRotatedFlatDirectionFrom(Vector2Int vect)
        {
            if (vect.x != 0) return new Vector2Int(0, vect.x);
            else return new Vector2Int(vect.y, 0);
        }

        public static Vector3Int GetRotatedFlatDirectionFrom(Vector3Int vect)
        {
            if (vect.x != 0) return new Vector3Int(0, 0, vect.x);
            else return new Vector3Int(vect.z, 0, 0);
        }

        public static void TransferFromListToList<T>(List<T> from, List<T> to, bool checkForDuplicates = false)
        {
            if (to == null) return;

            if (!checkForDuplicates)
            {
                for (int i = 0; i < from.Count; i++)
                    to.Add(from[i]);
            }
            else
            {
                for (int i = 0; i < from.Count; i++)
                    if (!to.Contains(from[i]))
                        to.Add(from[i]);
            }
        }

#if UNITY_EDITOR
        public static List<T> GetAllSelected<T>(T ignore) where T : MonoBehaviour
        {
            List<T> selected = new List<T>();

            for (int i = 0; i < UnityEditor.Selection.gameObjects.Length; i++)
            {
                T p = UnityEditor.Selection.gameObjects[i].GetComponent<T>();
                if (p != ignore) if (p) selected.Add(p);
            }

            return selected;
        }
#endif

        /// <summary>
        /// Generating spawn instruction in desired direction of checker field
        /// After all set definition and add to guides list
        /// </summary>
        public static SpawnInstruction GenerateInstructionTowards(CheckerField checker, Vector2Int start, Vector3Int dir, int centerRange = 0, bool findAlways = true)
        {
            SpawnInstruction instr = new SpawnInstruction();
            instr.desiredDirection = dir;
            instr.useDirection = true;

            Vector2Int dir2D = new Vector2Int(dir.x, dir.z);
            Vector2Int targetSquare;

            if (centerRange > 0)
            {
                targetSquare = checker.FindEdgeSquareInDirection(start - Vector2Int.one, dir2D);
                targetSquare = checker.GetCenterOnEdge(targetSquare, dir2D, centerRange + 1);
            }
            else
            {
                targetSquare = checker.FindEdgeSquareInDirection(start - new Vector2Int(0, 0), dir2D);
                if (findAlways) targetSquare = checker.GetCenterOnEdge(targetSquare, dir2D, 1);
            }

            instr.helperCoords = targetSquare.V2toV3Int();
            instr.gridPosition = checker.FromWorldToGridPos(targetSquare).V2toV3Int();

            return instr;
        }

        public static SpawnInstruction GenerateInstructionTowards(CheckerField checker, Vector2Int start, Vector3Int dir, SingleInteriorSettings settings)
        {
            SpawnInstruction instr = GenerateInstructionTowards(checker, start, dir, settings.GetCenterRange());

            if (FGenerators.CheckIfExist_NOTNULL(settings))
                if (settings.FieldSetup != null)
                    if (settings.DoorHoleCommandID < settings.FieldSetup.CellsCommands.Count)
                        instr.definition = settings.FieldSetup.CellsCommands[settings.DoorHoleCommandID];

            return instr;
        }

        public static SpawnInstruction GenerateInstructionTowards(CheckerField start, CheckerField other, SingleInteriorSettings settings)
        {
            SpawnInstruction instr = GenerateInstructionTowards(start, other, settings.GetCenterRange());

            if (FGenerators.CheckIfExist_NOTNULL(settings))
                if (settings.FieldSetup != null)
                    if (settings.DoorHoleCommandID < settings.FieldSetup.CellsCommands.Count)
                        instr.definition = settings.FieldSetup.CellsCommands[settings.DoorHoleCommandID];

            return instr;
        }

        public static SpawnInstruction GenerateInstructionTowardsSimple(CheckerField start, CheckerField other, int centerRange)
        {
            SpawnInstruction instr = GenerateInstructionTowards(start, other, centerRange);
            return instr;
        }

        /// <summary>
        /// Generating spawn instruction on edge wall to other checker field
        /// </summary>
        /// <param name="helperCoords"> When you create hole using GenerateInstructionTowards then instruction saves used square in instr.helperCoords variable which can be useful when creating counter-door hole </param>
        public static SpawnInstruction GenerateInstructionTowards(CheckerField start, CheckerField other, int centerRange = 0, Vector2Int? helperCoords = null)
        {
            Vector2Int nearestOwn;
            Vector2Int nearestOther;

            if (helperCoords == null)
            {
                nearestOwn = start.NearestPoint(other);
                nearestOther = other.NearestPoint(nearestOwn);
            }
            else // Used for generating counter-door-holes
            {
                nearestOwn = start.NearestPoint(helperCoords.Value);
                nearestOther = helperCoords.Value;
            }

            // Centering
            nearestOwn = start.GetCenterOnEdge(nearestOwn, nearestOther - nearestOwn, centerRange, other);
            nearestOther = other.NearestPoint(nearestOwn);

            SpawnInstruction instr = new SpawnInstruction();
            instr.helperCoords = nearestOwn.V2toV3Int();
            instr.desiredDirection = (nearestOther - nearestOwn).V2toV3Int();
            instr.useDirection = true;
            instr.gridPosition = start.FromWorldToGridPos(nearestOwn).V2toV3Int();

            return instr;
        }


        public enum ERoundingMode
        {
            Floor, Round, Ceil
        }

        public static Vector3Int Round(Vector3 toRound, ERoundingMode mode)
        {
            if (mode == ERoundingMode.Floor) return toRound.V3toV3IntF();
            else if (mode == ERoundingMode.Round) return toRound.V3toV3Int();
            else return toRound.V3toV3IntC();
        }


        #endregion


        #region Core Utilities


        #region Rules Copy Paste Clipboard

        static List<SpawnRuleBase> ruleBaseClipboard = new List<SpawnRuleBase>();
        public static void CopyProperties(SpawnRuleBase spawnRuleBase)
        {
            bool replaced = false;
            for (int i = 0; i < ruleBaseClipboard.Count; i++)
                if (spawnRuleBase.GetType() == ruleBaseClipboard[i].GetType())
                {
                    ruleBaseClipboard[i] = spawnRuleBase;
                    replaced = true;
                    break;
                }

            if (!replaced) ruleBaseClipboard.Add(spawnRuleBase);
        }

        public static bool CopyProperties_FindTypeInClipboard(SpawnRuleBase spawnRuleBase)
        {
            if (spawnRuleBase._CopyPasteSupported() == false) return false;
            for (int i = 0; i < ruleBaseClipboard.Count; i++)
                if (spawnRuleBase.GetType() == ruleBaseClipboard[i].GetType())
                {
                    if (ruleBaseClipboard[i] == spawnRuleBase) return false;
                    return true;
                }
            return false;
        }

        public static void CopyProperties_PasteTo(SpawnRuleBase spawnRuleBase, bool force)
        {
            if (spawnRuleBase._CopyPasteSupported() == false) return;
            for (int i = 0; i < ruleBaseClipboard.Count; i++)
                if (spawnRuleBase.GetType() == ruleBaseClipboard[i].GetType())
                {
                    spawnRuleBase.PasteOtherProperties(ruleBaseClipboard[i], force);
                }
        }

        #endregion


        public static void CheckForNulls<T>(List<T> classes)
        {
            for (int i = classes.Count - 1; i >= 0; i--)
            {
                if (classes[i] == null) classes.RemoveAt(i);
            }
        }

        public static void CheckForNullsO<T>(List<T> objects) where T : UnityEngine.Object
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] == null)
                {
                    objects.RemoveAt(i);
                }
            }
        }

        public static void AdjustCount<T>(List<T> list, int targetCount, bool addNulls = false) where T : class, new()
        {
            if (list.Count == targetCount) return;

            if (list.Count < targetCount)
            {
                if (addNulls)
                {
                    while (list.Count < targetCount) list.Add(null);
                }
                else
                {
                    while (list.Count < targetCount) list.Add(new T());
                }
            }
            else
            {
                while (list.Count > targetCount) list.RemoveAt(list.Count - 1);
            }

            //if (list.Count < targetCount)
            //{
            //    for (int i = 0; i < targetCount - list.Count; i++)
            //    {
            //        if (addNulls)
            //            list.Add(null);
            //        else
            //            list.Add(new T());
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < list.Count - targetCount; i++)
            //    {
            //        list.RemoveAt(list.Count - 1);
            //    }
            //}
        }

        public static void AdjustUnityObjCount<T>(List<T> list, int targetCount, T defaultObj = null) where T : UnityEngine.Object
        {
            if (list.Count == targetCount) return;

            //if (list.Count < targetCount)
            //{
            //    for (int i = 0; i < targetCount - list.Count; i++)
            //    {
            //        if (defaultObj == null)
            //            list.Add(null);
            //        else
            //            list.Add(GameObject.Instantiate(defaultObj) );
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < list.Count - targetCount; i++)
            //    {
            //        list.RemoveAt(list.Count - 1);
            //    }
            //}

            if (list.Count < targetCount)
            {
                if (defaultObj == null)
                {
                    while (list.Count < targetCount) list.Add(null);
                }
                else
                {
                    while (list.Count < targetCount) list.Add(GameObject.Instantiate(defaultObj));
                }
            }
            else
            {
                while (list.Count > targetCount) list.RemoveAt(list.Count - 1);
            }
        }

        public static void AdjustStructsListCount<T>(List<T> list, int targetCount, T add) where T : struct
        {
            if (list.Count == targetCount) return;

            if (list.Count < targetCount)
            {
                while (list.Count < targetCount) list.Add(add);
            }
            else
            {
                while (list.Count > targetCount) list.RemoveAt(list.Count - 1);
            }

            //if (list.Count < targetCount)
            //{
            //    for (int i = 0; i < targetCount - list.Count; i++)
            //    {
            //        list.Add(add);
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < list.Count - targetCount; i++)
            //    {
            //        list.RemoveAt(list.Count - 1);
            //    }
            //}
        }

        #endregion


        #region Grid related

        /// <summary>
        /// World Position to Grid Cell number Position
        /// </summary>
        public static Vector3Int WorldToGridCellPosition(Transform generator, FieldSetup fieldS, Vector3 worldPosition, int YLevel = 0, bool is2D = false)
        {
            worldPosition = WorldToGridLocalPositionInWorld(generator, fieldS, worldPosition, is2D);

            Vector3Int gridPos;
            float unitSize = fieldS.GetCellUnitSize().x;

            if (is2D)
            {
                float x = worldPosition.x / unitSize;
                float y = (worldPosition.y - unitSize * 0.5f) / unitSize;
                gridPos = new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), YLevel);
            }
            else
            {
                float x = (worldPosition.x) / unitSize;
                float z = (worldPosition.z - (unitSize * 0.25f)) / unitSize;
                gridPos = new Vector3Int(Mathf.RoundToInt(x), YLevel, Mathf.RoundToInt(z));
            }

            return gridPos;
        }

        public static Vector3Int WorldToGridLocalPositionInWorld(Transform generator, FieldSetup fieldS, Vector3 worldPosition, bool is2D = false)
        {
            if (fieldS == null) return Vector3Int.zero;

            Vector3 cSize = fieldS.GetCellUnitSize();

            if (is2D == false) // Flat top down
            {
                Vector3 mousePos = generator.InverseTransformPoint(worldPosition);
                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos, cSize);
                return (gridPos).V3toV3Int();
            }
            else // 2D - Flat Side
            {
                Vector3 mousePos = generator.InverseTransformPoint(worldPosition);
                Vector3 gridPos = FVectorMethods.FlattenVector(mousePos - new Vector3(0f, cSize.y * 0.5f, 0f), cSize);
                return (gridPos).V3toV3Int();
            }
        }


        /// <summary>
        /// From world position to cell position in world
        /// </summary>
        public static Vector3 GetWorldPositionOfCellAt(Transform generator, FieldSetup fieldS, Vector3 worldPosition, bool is2D = false)
        {
            return generator.TransformPoint(WorldToGridLocalPositionInWorld(generator, fieldS, worldPosition, is2D));
        }

        #endregion


        #region Editor Textures and Icons

#if UNITY_EDITOR
        public static Texture2D Tex_Selector { get { if (__texSelctr != null) return __texSelctr; __texSelctr = Resources.Load<Texture2D>("SPR_MultiSelect"); return __texSelctr; } }
        private static Texture2D __texSelctr = null;

        public static Texture TEX_FolderDir { get { if (__Tex_folderdir != null) return __Tex_folderdir; __Tex_folderdir = EditorGUIUtility.IconContent("Folder Icon").image; return __Tex_folderdir; } }
        private static Texture __Tex_folderdir = null;

        public static Texture TEX_CellInstr { get { if (__texCellInstr != null) return __texCellInstr; __texCellInstr = Resources.Load<Texture2D>("SPR_CellCommand"); return __texCellInstr; } }
        private static Texture __texCellInstr = null;

        public static Texture TEX_MenuIcon { get { if (__Tex_menuDir != null) return __Tex_menuDir; __Tex_menuDir = FGUI_Resources.Tex_MoreMenu; return __Tex_menuDir; } }
        private static Texture __Tex_menuDir = null;

        public static Texture TEX_PrintIcon { get { if (__texPrnt != null) return __texPrnt; __texPrnt = Resources.Load<Texture2D>("SPR_PlanPrint"); return __texPrnt; } }
        private static Texture __texPrnt = null;
        public static Texture TEX_ModGraphIcon { get { if (__texMdGr != null) return __texMdGr; __texMdGr = Resources.Load<Texture2D>("SPR_ModNodeSmall"); return __texMdGr; } }
        private static Texture __texMdGr = null;
        public static Texture TEX_Prepare { get { if (__texPrepr != null) return __texPrepr; __texPrepr = Resources.Load<Texture2D>("SPR_Prepare"); return __texPrepr; } }
        private static Texture __texPrepr = null;


        //public static GUIContent _PlannerIconOld { get { if (tex_plannerIconOld == null) tex_plannerIconOld = new GUIContent(Resources.Load<Texture2D>("SPR_PGG")); return tex_plannerIconOld; } }
        //private static GUIContent tex_plannerIconOld = null;

        public static Texture _Tex_ModPackSmall { get { if (__texModPackSml == null) __texModPackSml = (Resources.Load<Texture2D>("PR_ModPackSmall")); return __texModPackSml; } }
        private static Texture __texModPackSml = null;
        public static Texture _Tex_ModsSmall { get { if (__texModsSml == null) __texModsSml = (Resources.Load<Texture2D>("SPR_ModificationSmall")); return __texModsSml; } }
        private static Texture __texModsSml = null;

        public static GUIContent _PlannerIcon { get { if (tex_plannerIcon == null) tex_plannerIcon = new GUIContent(Resources.Load<Texture2D>("SPR_PGGSmall")); return tex_plannerIcon; } }
        private static GUIContent tex_plannerIcon = null;

        private static GUIContent tex_cellIcon = null;
        public static GUIContent _CellIcon
        {
            get
            {
                if (tex_cellIcon == null) tex_cellIcon = new GUIContent(Resources.Load<Texture2D>("SPR_FieldCell"));
                return tex_cellIcon;
            }
        }

        public static void SetDarkerBacgroundOnLightSkin()
        {
            if (EditorGUIUtility.isProSkin == false)
            {
                Color preCol = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 1f);
                EditorGUILayout.BeginVertical(FGUI_Resources.FrameBoxStyle);
                GUI.color = preCol;
            }
        }

        public static void EndVerticalIfLightSkin()
        {
            if (EditorGUIUtility.isProSkin == false)
            {
                EditorGUILayout.EndVertical();
            }
        }

#endif

        #endregion


        #region Editor GUI walkarounds


#if UNITY_EDITOR

        private static int[] _editor_ignoredExceptionCodes = null;
        public static int[] _Editor_GetIgnoredExceptions()
        {
            if (_editor_ignoredExceptionCodes == null)
            {
                _editor_ignoredExceptionCodes = new int[]
                    {-2147024809, -2146233088 };
            }

            return _editor_ignoredExceptionCodes;
        }

        public static bool _Editor_IsExceptionIgnored(System.Exception exc)
        {
            if (exc == null) return true;

            var codes = _Editor_GetIgnoredExceptions();
            for (int i = 0; i < codes.Length; i++)
            {
                if (exc.HResult == codes[i]) return true;
            }

            return false;
        }

#endif

        #endregion

    }
}