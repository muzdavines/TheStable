using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class FTIE01_ClickToSpawn : MonoBehaviour {

	public GameObject camObject;
	public GameObject lightObject;
	public GameObject robotObject;
	//public GameObject Prefab;
	public Text prefabName;
	public GameObject[] particlePrefab;
	public int particleNum = 0;

	GameObject effectPrefab;
	bool checkEffect = false;
	bool checkChara = true;
	bool checkLight = true;
	bool checkCamera = true;
	Animator camAnim;
	Vector3 clickPosition;

	void Start () {
		camAnim = camObject.GetComponent<Animator>();
	
	}
	

	void Update () {

		//Physics.Raycast(ray, out hit) 
		if ( Input.GetMouseButtonDown(0) ){ 
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(ray, out hit, 100.0f)){
				if(particleNum >= 0 && particleNum <= 3){
					effectPrefab = (GameObject)Instantiate(particlePrefab[particleNum],
						new Vector3(hit.point.x, hit.point.y + 0.2f, hit.point.z), Quaternion.Euler(0,0,0));
				}
				if(particleNum >= 4 && particleNum <= 10){
					effectPrefab = (GameObject)Instantiate(particlePrefab[particleNum],
						new Vector3(hit.point.x, hit.point.y + 1.0f, hit.point.z), Quaternion.Euler(0,0,0));
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)){
			particleNum -= 1;
			if( particleNum < 0) {
				particleNum = particlePrefab.Length-1;
			}		
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)){
			particleNum += 1;
			if(particleNum >(particlePrefab.Length - 1)) {
				particleNum = 0;
			}
		}
		
		prefabName.text= particlePrefab[particleNum].name;


	}

	public void OnClick_cam() {
		if(checkCamera == true){
			camAnim.speed = 0f;
			checkCamera = false;
			return;
		}
		if(checkCamera == false){
			camAnim.speed = 1f;
			checkCamera = true;
			return;
		}
	}

	public void OnClick_light() {
		if(checkLight == true){
			lightObject.SetActive(false);
			checkLight = false;
			return;
		}
		if(checkLight == false){
			lightObject.SetActive(true);
			checkLight = true;
			return;
		}
	}

	public void OnClick_chara() {
		if(checkChara == true){
			robotObject.SetActive(false);
			checkChara = false;
			return;
		}
		if(checkChara == false){
			robotObject.SetActive(true);
			checkChara = true;
			return;
		}
	}
}
