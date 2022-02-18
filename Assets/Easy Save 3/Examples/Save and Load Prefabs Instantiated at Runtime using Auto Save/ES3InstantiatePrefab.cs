using UnityEngine;

/*
 * 
 * This class simply instantiates a prefab at a random position whenever the
 * CreateRandomPrefab() method is called.
 * 
 */
public class ES3InstantiatePrefab : MonoBehaviour 
{
	// The prefab we want to create.
	public GameObject prefab;

	/* Instantiates the prefab at a random position and with a random rotation. */
	public void CreateRandomPrefab()
	{
		var go = Instantiate(prefab, Random.insideUnitSphere * 5, Random.rotation);
	}
}
