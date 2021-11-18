/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System.Collections;
using UnityEngine;

namespace EnergyBarToolkit {

public class RandomInstantiatedValueChanger : MonoBehaviour {
    private EnergyBarSpawnerUGUI spawner;

    #region Public Fields

    public float interval = 2;

    #endregion

    #region Private Fields
    #endregion

    #region Public Methods
    #endregion

    #region Unity Methods

    void Start() {
        spawner = GetComponent<EnergyBarSpawnerUGUI>();
        StartCoroutine(Work());
    }

    #endregion

    #region Private Methods

    private IEnumerator Work() {
        while (true) {
            float value = Random.Range(0f, 1f);
            var energyBar = spawner.instance.GetComponent<EnergyBar>();
            energyBar.ValueF = value;

            yield return new WaitForSeconds(interval);
        }
    }

    #endregion

    #region Inner and Anonymous Classes
    #endregion
}

} // namespace