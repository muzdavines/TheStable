using FIMSpace.Generating.Rules;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace FIMSpace.Generating.NotUsed
{

//    public class SR_StampTile_PlaneCheck : SpawnRuleBase, ISpawnProcedureType
//    {
//        public override string TitleName() { return "Stamp Tile Plane Fit"; }
//        public EProcedureType Type => EProcedureType.Rule;

//        public int TileCheckId = 3;
//        public ESR_Space TargetState = ESR_Space.Empty;
//        public string OccupiedTags = "";
//        [Tooltip("If you negate 'OutOfGrid' then 'Empty' or 'Occupied' will be treated as correct")]
//        public bool NegateTargetState = false;

//        [HideInInspector] public List<float> CustomRotors = new List<float>() { 0, 90, 180, 270 };

//        #region Editor Window

//#if UNITY_EDITOR
//        public override void NodeFooter(SerializedObject so, FieldModification mod)
//        {
//            SR_StampTile.DrawStampTileInfo = true;

//            base.NodeFooter(so, mod);

//            if (CustomRotors == null || CustomRotors.Count == 0) CustomRotors = new List<float>() { 0, 90, 180, 270 };
//            {
//                EditorGUILayout.BeginHorizontal();
//                GUILayout.Label("Angles To Check", GUILayout.Width(130));

//                int line = 0;
//                for (int i = 0; i < CustomRotors.Count; i++)
//                {
//                    CustomRotors[i] = EditorGUILayout.FloatField(GUIContent.none, CustomRotors[i], GUILayout.Width(38));

//                    if (i == CustomRotors.Count - 1 && line == 0)
//                    {
//                        if (GUILayout.Button("-", GUILayout.Width(22)))
//                        {
//                            CustomRotors.RemoveAt(CustomRotors.Count - 1);
//                            EditorUtility.SetDirty(this);
//                            EditorUtility.SetDirty(mod);
//                        }

//                        if (GUILayout.Button("+", GUILayout.Width(22)))
//                        {
//                            CustomRotors.Add(0);
//                            EditorUtility.SetDirty(this);
//                            EditorUtility.SetDirty(mod);
//                        }
//                    }

//                }

//                EditorGUILayout.EndHorizontal();
//            }

//        }
//#endif

//        #endregion


//        protected List<StamperTileTag.TileFitInfo> tilesChecked = new List<StamperTileTag.TileFitInfo>();

//        public override void CheckRuleOn(FieldModification mod, ref SpawnData spawn, FieldSetup preset, FieldCell cell, FGenGraph<FieldCell, FGenVertex> grid, Vector3? restrictDirection = null)
//        {
//            CellAllow = false;

//            if (OwnerSpawner.StampPrefabID < 0)
//            {
//                int mostCorrectPf = -1;
//                StamperTileTag.TileFitInfo mostCorrectTile = new StamperTileTag.TileFitInfo();
//                mostCorrectTile.JoinPoints = 0;
//                mostCorrectTile.Connected = -1;

//                for (int t = 0; t < mod.PrefabsList.Count; t++) // Check fit on every prefab with tile tag
//                {
//                    bool most = CheckPrefabTile(cell, grid, mod.PrefabsList[t].Prefab, ref mostCorrectTile);
//                    if (most) mostCorrectPf = t;
//                }

//                if (mostCorrectPf != -1)
//                {
//                    spawn.Prefab = mod.PrefabsList[mostCorrectPf].Prefab;
//                    spawn.PreviewMesh = mod.PrefabsList[mostCorrectPf].GetMesh();
//                    spawn.RotationOffset = mostCorrectTile.Rotation.eulerAngles; //mostCorrectRot;
//                    CellAllow = true;
//                }
//                else
//                {
//                    CellAllow = false;
//                }

//            }
//            else
//            {
//                StamperTileTag tile = mod.PrefabsList[OwnerSpawner.StampPrefabID].Prefab.GetComponent<StamperTileTag>();
//                if (tile == null) return;

//                StamperTileTag.TileFitInfo mostCorrectTile = new StamperTileTag.TileFitInfo();
//                bool most = CheckPrefabTile(cell, grid, tile.gameObject, ref mostCorrectTile);

//                if (most)
//                {
//                    if (mostCorrectTile.Connected == mostCorrectTile.JoinPoints)
//                    {
//                        spawn.Prefab = mod.PrefabsList[OwnerSpawner.StampPrefabID].Prefab;
//                        spawn.PreviewMesh = mod.PrefabsList[OwnerSpawner.StampPrefabID].GetMesh();
//                        spawn.RotationOffset = mostCorrectTile.Rotation.eulerAngles; //mostCorrectRot;
//                        CellAllow = true;
//                    }
//                    else
//                    {
//                        CellAllow = false;
//                    }
//                }
//                else
//                {
//                    CellAllow = false;
//                }
//            }
//        }

//        protected virtual bool CheckPrefabTile(FieldCell cell, FGenGraph<FieldCell, FGenVertex> grid, GameObject prefab, ref StamperTileTag.TileFitInfo mostCorrectTile)
//        {
//            StamperTileTag tile = prefab.GetComponent<StamperTileTag>();
//            bool checkOn = false;

//            if (tile == null) return false;

//            #region Each rotation setup
//            for (int r = 0; r < CustomRotors.Count; r++)
//            {
//                Quaternion rot = Quaternion.Euler(0f, CustomRotors[r], 0f);
//                #endregion

//                tilesChecked.Clear();

//                #region Each connections setup in tile tag
//                for (int c = 0; c < tile.ConnectionCubes.Count; c++)
//                {
//                    var cube = tile.ConnectionCubes[c];
//                    #endregion

//                    StamperTileTag.TileFitInfo fitInfo = new StamperTileTag.TileFitInfo();
//                    fitInfo.OwnerTile = tile;
//                    fitInfo.OwnerCube = cube;
//                    fitInfo.Rotation = rot;

//                    fitInfo = CheckTileOnNeightbours(fitInfo, cell, grid);

//                    tilesChecked.Add(fitInfo);

//                    // Check if this prefab is most correct one
//                    if (c == tile.ConnectionCubes.Count - 1)
//                    {
//                        int allConnections = 0;
//                        int connected = 0;

//                        for (int f = 0; f < tilesChecked.Count; f++)
//                        {
//                            allConnections += tilesChecked[f].JoinPoints;
//                            connected += tilesChecked[f].Connected;
//                        }

//                        if (connected > 0)
//                        {
//                            if (allConnections == connected)
//                            {
//                                if (mostCorrectTile.Connected != mostCorrectTile.JoinPoints)
//                                {
//                                    mostCorrectTile = fitInfo;
//                                    mostCorrectTile.Connected = connected;
//                                    mostCorrectTile.JoinPoints = allConnections;
//                                    checkOn = true;
//                                }
//                                else
//                                {
//                                    if (connected > mostCorrectTile.Connected)
//                                    {
//                                        mostCorrectTile = fitInfo;
//                                        mostCorrectTile.Connected = connected;
//                                        mostCorrectTile.JoinPoints = allConnections;
//                                        checkOn = true;
//                                    }
//                                }
//                            }
//                        }

//                    }
//                }
//            }

//            return checkOn;
//        }


//        protected virtual StamperTileTag.TileFitInfo CheckTileOnNeightbours(StamperTileTag.TileFitInfo baseInfo, FieldCell cell, FGenGraph<FieldCell, FGenVertex> grid)
//        {
//            StamperTileTag.TileFitInfo mostCorrectTile = new StamperTileTag.TileFitInfo();
//            mostCorrectTile.JoinPoints = 0;
//            mostCorrectTile.Connected = -1;

//            var cells = SpawnRules.GetTargetNeightboursAround(cell, grid, TargetState, OccupiedTags);
//            for (int i = 0; i < cells.Count; i++)
//            {
//                FieldCell nCell = cells[i];
//                Vector3 normalTo = new Vector3((cell.Pos.x - nCell.Pos.x), cell.Pos.y, (cell.Pos.z - nCell.Pos.z)).normalized;
//                var fitInfo = baseInfo.OwnerCube.TryFitAnyPlaneTo(baseInfo.OwnerTile, baseInfo.Rotation, normalTo, TileCheckId);

//                if (fitInfo.Connected > 0)
//                    if (fitInfo.JoinPoints == fitInfo.Connected)
//                    {
//                        if (mostCorrectTile.Connected != mostCorrectTile.JoinPoints)
//                        {
//                            mostCorrectTile = fitInfo;
//                        }
//                        else
//                        {
//                            if (fitInfo.Connected > mostCorrectTile.Connected)
//                                mostCorrectTile = fitInfo;
//                        }
//                    }
//            }

//            return mostCorrectTile;
//        }


//    }

}