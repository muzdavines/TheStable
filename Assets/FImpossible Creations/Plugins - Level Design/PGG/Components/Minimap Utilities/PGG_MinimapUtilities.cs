using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.Generating
{
    public enum EMinimapLayer { Background, Middle, Front }
    public enum EGenerateMode { TargetTextureResolution, EachCellIsPixel }

    public static class PGG_MinimapUtilities
    {
        /// <summary> Data structure which helps controling minimap generation </summary>
        public struct MinimapGeneratingSetup
        {

            public Texture2D LatestPixelmap;

            public Bounds LatestBounds;
            public Bounds LatestBakeBounds;
            public Vector2 LatestPivotForUI;
            public Vector2 LatestRatioTexToWorld;
            public bool RestrictAxisValue;

            public Color PaintColor;

            public FieldSetup Setup;
            public FGenGraph<FieldCell, FGenPoint> Grid;
            public Transform GeneratorTransform;

            public float BorderPaddingOffset;
            public EGenerateMode GenerateMode;
            public int TargetResolution;
            public float ScaleInitialBounds;

            public Func<Vector3, float> SecondaryAxis;
            public Func<Vector3, float, Vector3> SetSecAxis;
            public Func<Vector3, float> HeightAxis;

            public Bounds GetWorldBounds
            {
                get { if (wasGeneratingPixelmap) return LatestBounds; else return ComputeWorldGridBounds(Setup, Grid, GeneratorTransform).Value; }
            }

            bool wasGeneratingPixelmap;

            /// <summary>
            /// Prepare to be filled with info and texture.
            /// After prepare call 'ApplyFunctions' and then use GenerateFieldMinimap to generate texture.
            /// </summary>
            public void Prepare(Color paintColor, float borderOffset = 0f, EGenerateMode generateMode = EGenerateMode.TargetTextureResolution, int targetResolution = 128)
            {
                PaintColor = paintColor;

                Setup = null;
                Grid = null;
                GeneratorTransform = null;

                SecondaryAxis = null;
                SetSecAxis = null;
                HeightAxis = null;
                ScaleInitialBounds = 1f;

                BorderPaddingOffset = borderOffset;
                GenerateMode = generateMode;
                TargetResolution = targetResolution;

                RestrictAxisValue = false;
                LatestPixelmap = null;
                LatestBounds = new Bounds();
                LatestBakeBounds = new Bounds();
                LatestPivotForUI = Vector2.zero;
                LatestRatioTexToWorld = Vector2.one;
            }


            /// <summary>
            /// Apply references to functions to call generating in desired space
            /// </summary>
            /// <param name="secAxis"> Secondary axis - if it's top down then secondary axis is Z axis. If it's sidescroller then secondary axis is Y axis. </param>
            /// <param name="setSecAxis"> Apply vector value to the secondary axis. </param>
            /// <param name="heightAxis"> Height axis - if it's top down then height axis is Y axis. If it's sidescroller then height axis is Z axis. </param>
            public void ApplyFunctions(Func<Vector3, float> secAxis,
                Func<Vector3, float, Vector3> setSecAxis,
                Func<Vector3, float> heightAxis
                )
            {
                SecondaryAxis = secAxis;
                SetSecAxis = setSecAxis;
                HeightAxis = heightAxis;
            }




            #region Generate Texture Map Methods


            /// <summary>
            /// !!! Preparation constructor and ApplyFunctions required for this method to work !!!
            /// </summary>
            public void GenerateFieldMinimap(PGGGeneratorRoot root)
            {
                GenerateFieldMinimap(root.PGG_Setup, root.PGG_Grid, root.transform);
            }

            /// <summary>
            /// !!! Preparation constructor and ApplyFunctions required for this method to work !!!
            /// </summary>
            public void GenerateFieldMinimap(FieldSetup fs, FGenGraph<FieldCell, FGenPoint> grid, Transform t)
            {
                Setup = fs;
                Grid = grid;
                GeneratorTransform = t;

                if (LatestPixelmap != null) FGenerators.DestroyObject(LatestPixelmap);

                if (fs == null) { UnityEngine.Debug.Log("[PGG Minimap] No FieldSetup in " + t.name + "!"); return; }
                if (grid == null) { UnityEngine.Debug.Log("[PGG Minimap] No Grid in " + t.name + "!"); return; }

                Vector3 cellUnitSize = fs.GetCellUnitSize();
                Bounds? gridWorldBounds = PGG_MinimapUtilities.ComputeWorldGridBounds(fs, grid, t, Vector3.Scale(cellUnitSize, GetAxisOffset()));

                if (gridWorldBounds == null) return;

                Color[] pixels;

                if (GenerateMode == EGenerateMode.EachCellIsPixel)
                {

                    #region Generating 1 cell = 1 pixel texture

                    Bounds gridCellSpaceBounds = PGG_MinimapUtilities.ComputeGridCellSpaceBounds(grid, false);

                    // Cell Offset caused by center - origin cells
                    gridCellSpaceBounds.Encapsulate(gridCellSpaceBounds.max + new Vector3(1f, 0f, 1f));
                    gridCellSpaceBounds.size = gridCellSpaceBounds.size;
                    LatestBounds = gridCellSpaceBounds;

                    Vector2Int maxPixel = GetMaxPixel(gridCellSpaceBounds);

                    // Prepare texture to paint on
                    LatestPixelmap = GenerateTexture2D(maxPixel.x, maxPixel.y);
                    pixels = PGG_MinimapUtilities.GenerateColorArrayFor(maxPixel.x, maxPixel.y);

                    LatestRatioTexToWorld = new Vector2(cellUnitSize.x, cellUnitSize.z);

                    LatestBounds = PGG_MinimapUtilities.ScaleBounds(LatestBounds, cellUnitSize);
                    LatestBakeBounds = LatestBounds;

                    LatestPivotForUI = GetUIPivot(GetMinToZeroCell(fs, grid, cellUnitSize, 0f), LatestBakeBounds);

                    for (int c = 0; c < grid.AllApprovedCells.Count; c++)
                    {
                        var cell = grid.AllApprovedCells[c];
                        Vector2Int pixPos;

                        pixPos = GetPixelPos(gridCellSpaceBounds, cell.Pos, Vector2.one);
                        PGG_MinimapUtilities.PaintPx(pixPos, PaintColor, pixels, maxPixel);
                    }
                    
                    #endregion

                }
                else // Generating greater grid texture which supports rooms rotation better
                {

                    Bounds gridSpaceBounds = PGG_MinimapUtilities.ComputeGridCellSpaceBounds(grid);
                    gridSpaceBounds = PGG_MinimapUtilities.ScaleBoundsWithSetup(gridSpaceBounds, fs);

                    LatestBounds = gridSpaceBounds;

                    Bounds bakeBounds = gridSpaceBounds;
                    bakeBounds = PGG_MinimapUtilities.ApplyBoundsBorderOffset(bakeBounds, BorderPaddingOffset);
                    LatestBakeBounds = bakeBounds;


                    #region Prepare helper ratio parameters

                    Vector3 boundsSize = bakeBounds.size;
                    float maxSize = GetBoundsMaxDimension(boundsSize);

                    float pxRatio = (maxSize) / TargetResolution;
                    Vector2 fromTexToWorldSpace = new Vector2(pxRatio, pxRatio);

                    Vector2Int maxPixel = GetPixelPos(bakeBounds, bakeBounds.max, fromTexToWorldSpace);

                    #endregion


                    LatestPivotForUI = GetUIPivot(GetMinToZeroCell(fs, grid, cellUnitSize, BorderPaddingOffset), LatestBakeBounds);

                    LatestRatioTexToWorld = fromTexToWorldSpace;
                    LatestPixelmap = GenerateTexture2D(maxPixel.x, maxPixel.y);
                    pixels = PGG_MinimapUtilities.GenerateColorArrayFor(maxPixel.x, maxPixel.y);

                    Vector2Int rectPaintSize = new Vector2Int(0, 0);
                    rectPaintSize.x = Mathf.RoundToInt((cellUnitSize.x / fromTexToWorldSpace.x) / 2f);
                    Vector3 xyYPivotOff = new Vector3(0f, 0f, 0f);

                    ModifyRectPaintSize(ref rectPaintSize, ref xyYPivotOff, cellUnitSize, fromTexToWorldSpace);

                    if (RestrictAxisValue == false)
                    {
                        bool xo = rectPaintSize.x % 2 != 0;
                        bool yo = rectPaintSize.y % 2 != 0;
                        
                        for (int c = 0; c < grid.AllApprovedCells.Count; c++)
                        {
                            var cell = grid.AllApprovedCells[c];
                            Vector2Int pixPos;

                            int nx = 0;
                            if (xo) nx = grid.IsEmpty(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z) ? -1 : 0;
                            //int px = grid.IsEmpty(cell.Pos.x + 1, cell.Pos.y, cell.Pos.z) ? -1 : 0;
                            int ny = 0;
                            if (yo) ny = grid.IsEmpty(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1) ? -1 : 0;
                            //int py = grid.IsEmpty(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1) ? -1 : 0;

                            pixPos = GetPixelPos(bakeBounds, fs.GetCellWorldPosition(cell) + xyYPivotOff, fromTexToWorldSpace);
                            PGG_MinimapUtilities.PaintRect(pixPos, rectPaintSize, PaintColor, pixels, maxPixel, nx, ny/*, nx, px, ny, py*/);
                        }
                    }
                    else
                    {
                        bool xo = rectPaintSize.x % 2 != 0;
                        bool yo = rectPaintSize.y % 2 != 0;

                        for (int c = 0; c < grid.AllApprovedCells.Count; c++)
                        {
                            var cell = grid.AllApprovedCells[c];
                            Vector3 wPos = (fs.GetCellWorldPosition(cell) + xyYPivotOff);
                            Vector2Int pixPos;

                            if (IsPositionRestricted(t.TransformPoint(wPos), cellUnitSize)) continue;

                            int nx = 0;
                            if (xo) nx = grid.IsEmpty(cell.Pos.x - 1, cell.Pos.y, cell.Pos.z) ? -1 : 0;
                            int ny = 0;
                            if (yo) ny = grid.IsEmpty(cell.Pos.x, cell.Pos.y, cell.Pos.z + 1) ? -1 : 0;

                            pixPos = GetPixelPos(bakeBounds, wPos, fromTexToWorldSpace);
                            PGG_MinimapUtilities.PaintRect(pixPos, rectPaintSize, PaintColor, pixels, maxPixel, nx, ny);
                        }
                    }

                }


                if (GenerateMode == EGenerateMode.EachCellIsPixel) LatestPixelmap.filterMode = FilterMode.Point;
                LatestPixelmap.wrapMode = TextureWrapMode.Clamp;
                LatestPixelmap.SetPixels(pixels);
                LatestPixelmap.Apply();

            }

            public static Texture2D GenerateTexture2D(int width, int height)
            {
                return new Texture2D(width, height, TextureFormat.RGBA32, false);
            }

            /// <summary>
            /// !!! Preparation constructor and ApplyFunctions required for this method to work !!!
            /// </summary>
            public void GenerateFieldsPixelmap(List<PGGGeneratorRoot> GenerateOutOf, Vector2? forceAspectRatio = null)
            {
                wasGeneratingPixelmap = true;

                if (LatestPixelmap != null) FGenerators.DestroyObject(LatestPixelmap);

                LatestBounds = new Bounds();

                FGenerators.CheckForNulls(GenerateOutOf);
                if (GenerateOutOf.Count == 0) return;

                Bounds worldBounds = new Bounds();
                worldBounds = PGG_MinimapUtilities.ComputeWorldGridsBounds(GenerateOutOf, GetUsedAxis());
                worldBounds = PGG_MinimapUtilities.ApplyBoundsBorderOffset(worldBounds, BorderPaddingOffset);
                worldBounds.size = worldBounds.size * ScaleInitialBounds;

                LatestBounds = worldBounds;


                #region Prepare helper ratio parameters


                Vector3 boundsSize = worldBounds.size;
                float maxSize = GetBoundsMaxDimension(boundsSize);

                if (forceAspectRatio != null && forceAspectRatio.Value.y != 0f)
                {
                    float targetRatio = forceAspectRatio.Value.x / forceAspectRatio.Value.y;

                    SetMapRatio(ref boundsSize, targetRatio);
                    worldBounds.size = boundsSize;
                }

                float pxRatio = maxSize / TargetResolution;
                Vector2 fromTexToWorldSpace = new Vector2(pxRatio, pxRatio);
                Vector2Int maxPixel;

                maxPixel = GetPixelPos(worldBounds, worldBounds.max, fromTexToWorldSpace);


                #endregion



                LatestRatioTexToWorld = fromTexToWorldSpace;
                LatestPixelmap = GenerateTexture2D(maxPixel.x, maxPixel.y);

                Color[] pixels = PGG_MinimapUtilities.GenerateColorArrayFor(LatestPixelmap);


                for (int i = 0; i < GenerateOutOf.Count; i++)
                {

                    FieldSetup fs = GenerateOutOf[i].PGG_Setup;
                    if (fs == null) continue;

                    FGenGraph<FieldCell, FGenPoint> grid = GenerateOutOf[i].PGG_Grid;
                    if (grid == null) continue;


                    Transform t = GenerateOutOf[i].transform;
                    Vector3 cellUnitSize = fs.GetCellUnitSize();

                    Vector2Int rectPaintSize = new Vector2Int(0, 0);
                    rectPaintSize.x = Mathf.RoundToInt((cellUnitSize.x / fromTexToWorldSpace.x) / 2f);
                    Vector3 xyYPivotOff = Vector3.zero;

                    ModifyRectPaintSize(ref rectPaintSize, ref xyYPivotOff, cellUnitSize, fromTexToWorldSpace);


                    if (RestrictAxisValue == false)
                    {
                        for (int c = 0; c < grid.AllApprovedCells.Count; c++)
                        {
                            var cell = grid.AllApprovedCells[c];
                            Vector2Int pixPos;

                            pixPos = GetPixelPos(worldBounds, t.TransformPoint(fs.GetCellWorldPosition(cell) + xyYPivotOff), fromTexToWorldSpace);
                            PGG_MinimapUtilities.PaintRect(pixPos, rectPaintSize, PaintColor, pixels, maxPixel);
                        }
                    }
                    else
                    {
                        for (int c = 0; c < grid.AllApprovedCells.Count; c++)
                        {
                            var cell = grid.AllApprovedCells[c];
                            Vector3 wPos = t.TransformPoint(fs.GetCellWorldPosition(cell) + xyYPivotOff);
                            Vector2Int pixPos;

                            if (IsPositionRestricted(wPos, cellUnitSize)) continue;

                            pixPos = GetPixelPos(worldBounds, wPos, fromTexToWorldSpace);
                            PGG_MinimapUtilities.PaintRect(pixPos, rectPaintSize, PaintColor, pixels, maxPixel);
                        }
                    }
                }

                LatestPixelmap.SetPixels(pixels);
                LatestPixelmap.wrapMode = TextureWrapMode.Clamp;
                LatestPixelmap.Apply();

            }


            #endregion



            #region Helper methods for generating minimap texture maps

            /// <summary> Gets pixel position out of world position based area </summary>
            Vector2Int GetPixelPos(Bounds b, Vector3 worldPos, Vector2 sizeRatio)
            {
                Vector2Int pos = new Vector2Int();
                pos.x = GetRoundValue(worldPos.x - b.min.x, sizeRatio.x);
                pos.y = GetRoundValue(SecondaryAxis(worldPos) - SecondaryAxis(b.min), sizeRatio.y);
                return pos;
            }

            void ModifyRectPaintSize(ref Vector2Int rectPaintSize, ref Vector3 xyYPivotOff, Vector3 cellUnitSize, Vector2 fromTexToWorldSpace)
            {
                rectPaintSize.y = Mathf.RoundToInt((SecondaryAxis(cellUnitSize) / fromTexToWorldSpace.y) / 2f);
                xyYPivotOff = new Vector3(0f, cellUnitSize.y * 0.5f);
            }


            Vector2 GetUISize(PGG_MinimapHandler minimap, Vector3 worldSize, Vector2 borderPaddingScaleRatio)
            {
                if (minimap == null) return Vector2.one;

                worldSize.x *= borderPaddingScaleRatio.x;
                SetSecAxis(worldSize, SecondaryAxis(worldSize) * borderPaddingScaleRatio.y);

                float ratio = minimap.DisplayRatio;
                return new Vector2(worldSize.x * ratio, SecondaryAxis(worldSize) * ratio);
            }



            Vector2 GetUIPivot(Vector3 minToZeroCell, Bounds bakeBounds)
            {
                return new Vector2(
                    minToZeroCell.x / bakeBounds.size.x,
                    SecondaryAxis(minToZeroCell) / SecondaryAxis(bakeBounds.size)
                    );
            }

            /// <summary> All textures origin is in left down corner so let's apply pivot basing on left down grid corner </summary>
            Vector3 GetMinToZeroCell(FieldSetup fs, FGenGraph<FieldCell, FGenPoint> grid, Vector3 unitSizeForScaling, float borderOffset, bool applyHalfCellOffset = true)
            {
                // Compute grid origin - bounds to zero cell relation
                Vector3 minToZeroCell = grid.GetMin().InverseV3Int();
                if (applyHalfCellOffset) minToZeroCell += new Vector3(.5f, 0f, .5f); // Cell Center offset
                minToZeroCell = Vector3.Scale(minToZeroCell, unitSizeForScaling);
                minToZeroCell += Vector3.one * borderOffset;
                return minToZeroCell;
            }

            Vector2Int GetMaxPixel(Bounds bounds)
            {
                return new Vector2Int(
                    Mathf.CeilToInt(bounds.size.x),
                    Mathf.CeilToInt(SecondaryAxis(bounds.size))
                    );
            }

            bool IsPositionRestricted(Vector3 wPos, Vector3 cellUnitSize)
            {
                if (Mathf.Abs(HeightAxis(wPos) - HeightAxis(GeneratorTransform.position)) > HeightAxis(cellUnitSize) * 0.5f) return true;
                return false;
            }

            Vector3 GetAxisOffset()
            {
                return new Vector3(0.5f, 0f, 0.5f);
            }

            float GetBoundsMaxDimension(Vector3 boundsSize)
            {
                if (boundsSize.x > SecondaryAxis(boundsSize)) return boundsSize.x; else return SecondaryAxis(boundsSize);
            }

            void SetMapRatio(ref Vector3 boundsSize, float targetRatio)
            {
                if (boundsSize.x > SecondaryAxis(boundsSize))
                    boundsSize = SetSecAxis(boundsSize, boundsSize.x / targetRatio);
                else
                    boundsSize.x = SecondaryAxis(boundsSize) / targetRatio;
            }

            public Vector3 GetUsedAxis()
            {
                Vector3 axis = new Vector3(1, 0, 0);
                axis = SetSecAxis(axis, 1f);
                return axis;
            }


            #endregion


        }



        /// <summary> [TOP DOWN] Gets pixel position out of world position </summary>
        public static Vector2Int GetPixelPosXZ(Bounds b, Vector3 worldPos, Vector2 sizeRatio)
        {
            Vector2Int pos = new Vector2Int();
            pos.x = GetRoundValue(worldPos.x - b.min.x, sizeRatio.x);
            pos.y = GetRoundValue(worldPos.z - b.min.z, sizeRatio.y);
            return pos;
        }


        /// <summary> [SIDESCROLLER] Gets pixel position out of world position </summary>
        public static Vector2Int GetPixelPosXY(Bounds b, Vector3 worldPos, Vector2 sizeRatio)
        {
            Vector2Int pos = new Vector2Int();
            pos.x = GetRoundValue(worldPos.x - b.min.x, sizeRatio.x);
            pos.y = GetRoundValue(worldPos.y - b.min.y, sizeRatio.y);
            return pos;
        }


        /// <summary> [SIDESCROLLER Z AXIS] Gets pixel position out of world position </summary>
        public static Vector2Int GetPixelPosZY(Bounds b, Vector3 worldPos, Vector2 sizeRatio)
        {
            Vector2Int pos = new Vector2Int();
            pos.x = GetRoundValue(worldPos.z - b.min.z, sizeRatio.x);
            pos.y = GetRoundValue(worldPos.y - b.min.y, sizeRatio.y);
            return pos;
        }


        /// <summary> Dividing value by ratio and round to int </summary>
        public static int GetRoundValue(float value, float sizeRatio)
        {
            return Mathf.RoundToInt(value / sizeRatio);
        }


        /// <summary> Painting pixels rectangle on texture pixel map </summary>
        public static void PaintRect(Vector2Int pxCenter, Vector2Int halfSizeInPx, Color toPaint, Color[] pixels, Vector2 dimensions, int ox = 0, int oy = 0/*, int nx = 0, int px = 0, int ny = 0, int py = 0*/)
        {
            if (halfSizeInPx == Vector2Int.zero)
            {
                pixels[GetPX(pxCenter, dimensions)] = toPaint;
            }
            else
            {
                for (int x = -halfSizeInPx.x; x <= halfSizeInPx.x + ox; x++)
                    for (int y = -halfSizeInPx.y; y <= halfSizeInPx.y + oy; y++)
                        pixels[GetPX(pxCenter.x + x, pxCenter.y + y, dimensions)] = toPaint;
            }
        }


        public static void PaintPx(Vector2Int pxCenter, Color toPaint, Color[] pixels, Vector2 dimensions)
        {
            pixels[GetPX(pxCenter, dimensions)] = toPaint;
        }


        /// <summary> Getting array index for position on texture </summary>
        public static int GetPX(Vector2Int xy, Vector2 dimensions)
        {
            return GetPX(xy.x, xy.y, dimensions);
        }


        /// <summary> Getting array index for position on texture </summary>
        public static int GetPX(int x, int y, Vector2 dimensions)
        {
            if (y < 0) y = 0;
            if (y >= dimensions.y) y = (int)dimensions.y - 1;

            if (x < 0) x = 0;
            if (x >= dimensions.x) x = (int)dimensions.x - 1;

            return (int)Mathf.Min(dimensions.x * dimensions.y - 1, y * dimensions.x + x);
        }


        /// <summary> Grid bounds without world space scale </summary>
        public static Bounds ComputeGridCellSpaceBounds(PGGGeneratorRoot root, bool applyCellCenterOffset = true)
        {
            if (root == null) return new Bounds();
            if (root.PGG_Grid == null) return new Bounds();
            return ComputeGridCellSpaceBounds(root.PGG_Grid, applyCellCenterOffset);
        }


        /// <summary> Grid bounds without world space scale </summary>
        public static Bounds ComputeGridCellSpaceBounds(FGenGraph<FieldCell, FGenPoint> grid, bool applyCellCenterOffset = true)
        {
            if (grid == null) return new Bounds();

            Vector3 min = grid.GetMin().V3IntToV3();
            Vector3 max = grid.GetMax().V3IntToV3();

            if (applyCellCenterOffset)
            {
                min -= new Vector3(0.5f, 0.0f, 0.5f);
                max += new Vector3(0.5f, 0.0f, 0.5f);
            }

            Bounds fBounds = new Bounds(Vector3.LerpUnclamped(min, max, 0.5f), Vector3.one);
            fBounds.Encapsulate(min);
            fBounds.Encapsulate(max);

            return fBounds;
        }


        public static Bounds ScaleBounds(Bounds b, Vector3 scale)
        {
            b.center = Vector3.Scale(scale, b.center);
            b.size = Vector3.Scale(scale, b.size);
            return b;
        }


        public static Bounds ScaleBoundsWithSetup(Bounds b, FieldSetup setup)
        {
            return ScaleBounds(b, setup.GetCellUnitSize());
        }


        /// <summary> Computing world space bounds for provided grid </summary>
        public static Bounds? ComputeWorldGridBounds(FieldSetup fs, FGenGraph<FieldCell, FGenPoint> grid, Transform t = null, Vector3? applyCellCenterOffset = null)
        {
            if (fs == null) return null;
            if (grid == null) return null;

            Vector3 min = fs.TransformCellPosition(grid.GetMin().V3IntToV3());
            Vector3 max = fs.TransformCellPosition(grid.GetMax().V3IntToV3());

            if (applyCellCenterOffset != null)
            {
                //min -= applyCellCenterOffset.Value;
                max += applyCellCenterOffset.Value;
                //min -= new Vector3(halfCellSize.x, 0.0f, halfCellSize.z);
                //max += new Vector3(halfCellSize.x, 0.0f, halfCellSize.z);
            }

            Bounds fBounds = new Bounds(Vector3.LerpUnclamped(min, max, 0.5f), Vector3.zero);
            fBounds.Encapsulate(min);
            fBounds.Encapsulate(max);

            if (t) fBounds = TransformBounding(fBounds, t);

            return fBounds;
        }

        public static Bounds TransformBounding(Bounds b, Transform by)
        {
            return TransformBounding(b, by.localToWorldMatrix);
        }

        public static Bounds TransformBounding(Bounds b, Matrix4x4 mx)
        {
            Vector3 min = mx.MultiplyPoint(b.min);
            Vector3 max = mx.MultiplyPoint(b.max);

            Vector3 minB = mx.MultiplyPoint(new Vector3(b.max.x, b.center.y, b.min.z));
            Vector3 maxB = mx.MultiplyPoint(new Vector3(b.min.x, b.center.y, b.max.z));

            b = new Bounds(min, Vector3.zero);

            b.Encapsulate(min);
            b.Encapsulate(max);
            b.Encapsulate(minB);
            b.Encapsulate(maxB);

            return b;
        }


        public static Bounds? ComputeWorldGridBounds(PGGGeneratorRoot root, bool applyRootTransform = true, Vector3? applyCellCenterOffset = null)
        {
            return ComputeWorldGridBounds(root.PGG_Setup, root.PGG_Grid, applyRootTransform ? root.transform : null, applyCellCenterOffset);
        }


        /// <summary> Computing world space bounds for provided grids </summary>
        public static Bounds ComputeWorldGridsBounds(List<PGGGeneratorRoot> GenerateOutOf, Vector3? applyCellCenterOffset = null)
        {
            Bounds worldBounds = new Bounds();
            int iter = 0;

            for (int i = 0; i < GenerateOutOf.Count; i++)
            {
                if (GenerateOutOf[i] == null) continue;

                Vector3 boundsOff = Vector3.zero;
                if (applyCellCenterOffset != null) if (GenerateOutOf[i].PGG_Setup != null)
                    {
                        boundsOff = Vector3.Scale(GenerateOutOf[i].PGG_Setup.GetCellUnitSize(), applyCellCenterOffset.Value);
                        boundsOff.x *= 0.5f;
                        // Grid Cell Y starts from 0 level, not from center
                        boundsOff.z *= 0.5f;
                    }

                Bounds? gridBounds = ComputeWorldGridBounds(GenerateOutOf[i], true, boundsOff);
                if (gridBounds == null) continue;

                if (iter == 0) { worldBounds = gridBounds.Value; }
                worldBounds.Encapsulate(gridBounds.Value);

                iter += 1;
            }

            return worldBounds;
        }


        /// <summary> Applying unit offset to borders of bounds </summary>
        public static Bounds ApplyBoundsBorderOffset(Bounds b, float units)
        {
            if (units > 0f)
            {
                b.Encapsulate(b.max + Vector3.one * units);
                b.Encapsulate(b.min - Vector3.one * units);
            }

            return b;
        }


        public static Color[] GenerateColorArrayFor(Texture2D tex)
        {
            return GenerateColorArrayFor(tex.width, tex.height);
        }


        public static Color[] GenerateColorArrayFor(int width, int height)
        {
            Color[] pixels = new Color[width * height];
            for (int p = 0; p < pixels.Length; p++) pixels[p] = Color.clear;
            return pixels;
        }


    }

}
