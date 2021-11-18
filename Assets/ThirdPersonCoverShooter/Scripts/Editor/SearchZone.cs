﻿using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    public class SearchZoneEditor : Editor
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.Selected)]
        static void Gizmo(SearchZone zone, GizmoType type)
        {
            var isSelected = (type & GizmoType.Selected) != 0;
            var alpha = isSelected ? 0.8f : 0.4f;

            var collider = zone.GetComponent<BoxCollider>();
            if (collider != null)
            {
                var bounds = collider.bounds;

                Gizmos.color = new Color(0, 1, 0.6f, alpha);
                Gizmos.DrawCube(bounds.center, bounds.extents * 2);
            }
        }
    }
}
