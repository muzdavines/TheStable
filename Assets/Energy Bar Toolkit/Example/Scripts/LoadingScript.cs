/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnergyBarToolkit {

public class LoadingScript : MonoBehaviour {

    #region Public Fields

    public float animSpeed = 1;

    private FilledRenderer3D filled;

    #endregion

    #region Public Properties
    #endregion

    #region Slots

    void OnEnable() {
        filled = GetComponent<FilledRenderer3D>();
    }

    void Update() {
        filled.radialOffset += animSpeed * Time.deltaTime;
    }

    #endregion

    #region Public Static Methods
    #endregion

    #region Inner and Anonymous Classes
    #endregion
}

}