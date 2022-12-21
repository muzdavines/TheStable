using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.Generating
{
    public partial class PipeGenerator
    {
        public void FindRandomDesiredPoint(int seed)
        {
            if (RFindMask == 0) return;

            FGenerators.SetSeed(seed);

            List<RaycastHit> findings = new List<RaycastHit>();
            int fails = 0;

            for (int t = 0; t < RFindTries; t++)
            {
                float collectDist = 0f;

                RaycastHit hit = new RaycastHit();
                Vector3 startPoint = transform.position;
                Vector3 goPoint = transform.position;

                for (int b = 0; b < RFindSteps; b++)
                {
                    float distance = RandomFindDistance();

                    if (WorldSpaceRFindDirs == false)
                        goPoint = startPoint + transform.TransformDirection(RandomFindDirection()) * distance;
                    else
                        goPoint = startPoint + (RandomFindDirection()) * distance;

                    //Vector3 dir = goPoint - startPoint;
                    hit = RandomFindCast(startPoint/* - dir * 0.15f*/, goPoint);

                    if (hit.transform)
                    {
                        if (collectDist >= RFindMinimumDistance)
                            break;
                        else
                        {
                            b--;
                            fails++;
                            if (fails > RFindTries) break;
                        }
                    }
                    else
                    {
                        collectDist += distance;
                        startPoint = goPoint;
                    }
                }

                if (hit.transform)
                {
                    //UnityEngine.Debug.DrawRay(hit.point - Vector3.forward * 0.3f, Vector3.forward * 0.6f, Color.blue, 1.1f);
                    //UnityEngine.Debug.DrawRay(hit.point - Vector3.right * 0.3f, Vector3.right * 0.6f, Color.blue, 1.1f);

                    if (collectDist > RFindMinimumDistance)
                        if (Vector3.Distance(hit.point, transform.position) > RFindMinimumDistance)
                        {
                            findings.Add(hit);
                        }
                }
            }

            if (findings.Count == 0) return;

            RaycastHit choosed = findings[FGenerators.GetRandom(0, findings.Count)];
            CustomEndingDirection = -GetRounded(choosed.normal);

            if (CustomEndingDirection.Value == Vector3.zero)
            {
                CustomEndingDirection = choosed.normal;
                FlattenNormal(Quaternion.LookRotation(CustomEndingDirection.Value));
            }
            else if (FlattendRFindNormal)
            {
                //Debug.DrawRay(choosed.point, CustomEndingDirection.Value.normalized * 10f, Color.red, 1.1f);
                //Debug.DrawRay(choosed.point, choosed.normal * 10f, Color.yellow, 1.1f);
                Vector3 flattened = FlattenNormal(Quaternion.LookRotation(CustomEndingDirection.Value));
                float angle = Vector3.Angle(CustomEndingDirection.Value, flattened);
                if (angle > 10) CustomEndingDirection = flattened;
            }

            CustomEndingPosition = choosed.point;
        }

        float RandomFindDistance()
        {
            return PresetData.Segments[0].ReferenceScale * (float)(FGenerators.GetRandom(1, 3));
            //return PresetData.Segments[Random.Range(0, PresetData.Segments.Count)].ReferenceScale * (float)(Random.Range(1,4));
        }

        Vector3 RandomFindDirection()
        {
            return RFindDirections[FGenerators.GetRandom(0, RFindDirections.Length)];
        }

        RaycastHit RandomFindCast(Vector3 start, Vector3 end)
        {
            RaycastHit hit;
            Physics.Linecast(start, end, out hit, RFindMask, QueryTriggerInteraction.Ignore);
            //UnityEngine.Debug.DrawLine(start, end, Color.red, 1.1f);
            //UnityEngine.Debug.DrawLine(end, Vector3.Lerp(start, end, 0.9f) + Vector3.one * 0.05f, Color.red, 1.1f);
            //UnityEngine.Debug.DrawLine(end, Vector3.Lerp(start, end, 0.9f) - Vector3.one * 0.05f, Color.red, 1.1f);
            return hit;
        }

        public Vector3 GetRounded(Vector3 dir)
        {
            return new Vector3(Mathf.Round(dir.x), Mathf.Round(dir.y), Mathf.Round(dir.z));
        }


    }
}
