using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ME_CustomLight : MonoBehaviour
{

    void Update()
    {
        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe(transform.position, null, out sh);
        var ambient = new Vector3(sh[0, 0] - sh[0, 6], sh[1, 0] - sh[1, 6], sh[2, 0] - sh[2, 6]);
        ambient = Vector3.Max(ambient, Vector3.zero);
        Shader.SetGlobalVector("ME_AmbientColor", ambient);

    }
}