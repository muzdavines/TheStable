using UnityEngine;
using System.Collections;

public class CursorLockHide : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Cursor.visible = false;
	
	}
	// Use this for initialization
	void Update () {
		Cursor.lockState = CursorLockMode.Locked;
	}
}