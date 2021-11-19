using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterChangeController : MonoBehaviour {

	public List<GameObject> characters;
	private int index =0;
	public SmoothFollow smoothFollow;
	// Use this for initialization
	void Start () {
		characters[index].SetActive(true);
		smoothFollow.target = characters[index].transform;
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetKeyDown (KeyCode.RightArrow)) {

			Vector3 initialCoordinate = characters[index].transform.position;
			Vector3 initialCoordinateRotation = characters[index].transform.eulerAngles;
			characters[index].SetActive(false);
			index ++;
			index = index> characters.Count -1 ? 0 : index;
			characters[index].SetActive(true);
			characters[index].transform.position = initialCoordinate;
			characters[index].transform.eulerAngles = initialCoordinateRotation;
			smoothFollow.target = characters[index].transform;
		}
	if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			Vector3 initialCoordinate = characters[index].transform.position;
			Vector3 initialCoordinateRotation = characters[index].transform.eulerAngles;
			characters[index].SetActive(false);
			index --;
			index = index < 0  ? characters.Count -1 : index;
			characters[index].SetActive(true);
			characters[index].transform.position = initialCoordinate;
			characters[index].transform.eulerAngles = initialCoordinateRotation;
			smoothFollow.target = characters[index].transform;
		}
	}
}
