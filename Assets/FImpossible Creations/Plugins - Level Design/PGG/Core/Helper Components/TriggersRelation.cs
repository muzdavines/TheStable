using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggersRelation : MonoBehaviour
{
    public Collider Trigger;
    public List<TriggersRelation> Neightbours;
    //[SerializeField] [HideInInspector] private List<Vector3> IntersectLocalPoints;
    //public Vector3 GetIntersectionPoint(TriggersRelation other)
    //{
    //    if (Neightbours != null)
    //        for (int i = 0; i < Neightbours.Count; i++)
    //        {
    //            if (Neightbours[i] == other)
    //                return transform.TransformPoint(IntersectLocalPoints[i]);
    //        }

    //    return transform.position;
    //}

    private void Reset()
    {
        TryGetTrigger();
    }

    public Collider GetRandomNeightbour()
    {
        if (Neightbours.Count == 0) return Trigger;
        
        if ( Neightbours.Count == 1)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
                return Trigger;
            else
                return Neightbours[0].Trigger;
        }

        float val = (float)Neightbours.Count;

        if (UnityEngine.Random.Range(0f, 1f) < 1f / val)
            return Trigger;

        return Neightbours[UnityEngine.Random.Range(0, Neightbours.Count)].Trigger;
    }

    public Collider GetBiggestNeightbour()
    {
        Collider biggest = Trigger;

        for (int i = 0; i < Neightbours.Count; i++)
        {
            Collider c = Neightbours[i].Trigger;

            if (c.bounds.size.magnitude > biggest.bounds.size.magnitude)
            {
                biggest = c;
            }
        }

        return biggest;
    }

    private void OnValidate()
    {
        TryGetTrigger();
    }

    public void AddNeightbour(TriggersRelation rel)
    {
        if (rel == null) return;
        if (Neightbours.Contains(rel) == false)
        {
            if (Neightbours == null) Neightbours = new List<TriggersRelation>();
            Neightbours.Add(rel);
        }
    }

    public void TryGetTrigger()
    {
        Trigger = GetComponent<Collider>();
        if (Trigger) if (Trigger.isTrigger == false) Trigger = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (Trigger == null) return;

        Color bc = Gizmos.color;

        Gizmos.color = Color.green;
        for (int i = 0; i < Neightbours.Count; i++)
        {
            if (Neightbours[i] == null) continue;
            if (Neightbours[i].Trigger == null) continue;
            Gizmos.DrawLine(Trigger.bounds.center, Neightbours[i].Trigger.bounds.center);
        }

        Gizmos.color = bc;
    }

    internal void Refresh()
    {
        if (Neightbours == null) Neightbours = new List<TriggersRelation>();
        TryGetTrigger();
    }

    public List<BoxCollider> FindRandomNearestAreas(Vector3 distanceFrom, float maxDistance, bool excludeInside, List<BoxCollider> areas)
    {
        List<BoxCollider> Nearests = new List<BoxCollider>();
        Nearests.Clear();

        float nearest = float.MaxValue;

        for (int i = 0; i < areas.Count; i++)
        {
            BoxCollider area = areas[i];

            bool isInside = false;
            if (IsInsideBounds(distanceFrom, area)) isInside = true;

            if (isInside)
            {
                if (excludeInside) continue;
                Nearests.Insert(0, areas[i]);
                continue;
            }

            Vector3 nPoint = GetNearestPointToBox(distanceFrom, area);
            float dist = Vector3.Distance(nPoint, distanceFrom);

            if (dist > maxDistance) continue;
            if (dist < nearest) Nearests.Insert(0, area); else Nearests.Add(area);
        }

        return Nearests;
    }

    public static bool IsInsideBounds(Vector3 worldPos, BoxCollider box)
    {
        Vector3 localPos = box.transform.InverseTransformPoint(worldPos);
        Vector3 delta = localPos - box.center + box.size * 0.5f;
        return Vector3.Max(Vector3.zero, delta) == Vector3.Min(delta, box.size);
    }

    public static Vector3 GetRandomPositionOnArea(BoxCollider boxArea, float offsetFromEdges = 0.25f)
    {
        Vector3 pos = boxArea.transform.TransformPoint(boxArea.center);

        Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));
        offset = Vector3.Scale(offset, boxArea.size / 2f - new Vector3(offsetFromEdges, 0f, offsetFromEdges));

        pos += boxArea.transform.TransformVector(offset);

        return pos;
    }

    public static Vector3 GetNearestPointToBox(Vector3 from, BoxCollider box)
    {
        Ray fromTo = new Ray(from, (box.center - from).normalized);
        RaycastHit hit;

        if (box.Raycast(fromTo, out hit, 100000f))
            return hit.point;
        else
            return box.ClosestPointOnBounds(from);
    }
}
