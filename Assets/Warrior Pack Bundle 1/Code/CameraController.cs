using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour{
	public GameObject cameraTarget;
	public float rotateSpeed;
	float rotate;
	public float offsetDistance;
	public float offsetHeight;
	public float smoothing;
	public Vector3 offset;
	bool following = true;
	Vector3 lastPosition;
    public bool myControl = true;

	void Start(){
        StartCoroutine(DelayStart());
        if (cameraTarget == null) { return; }
		lastPosition = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + offsetHeight, cameraTarget.transform.position.z - offsetDistance);
		//offset = new Vector3(cameraTarget.transform.position.x, cameraTarget.transform.position.y + offsetHeight, cameraTarget.transform.position.z - offsetDistance);
	}
    public void SetTarget(Transform newTarget) {
        print("#Cam#Setting target: " + newTarget.name);
        cameraTarget = newTarget.gameObject;
    }
    IEnumerator DelayStart() {
        yield return new WaitForSeconds(1.0f);
        myControl = true;
    }
    float nextControlChange = Mathf.Infinity;
    bool nextControl;
    public void SetControl (bool control = true, float time = 0) {
        nextControlChange = Time.time + time;
        nextControl = control;
    }

	void Update(){
        if (Time.time >= nextControlChange) {
            myControl = nextControl;
            nextControlChange = Mathf.Infinity;
        }
        if (!myControl) { return; }
		if(Input.GetKeyDown(KeyCode.F)){
			if(following){
				following = false;
			} 
			else{
				following = true;
			}
		} 
		if(Input.GetKey(KeyCode.Q)){
			rotate = -1;
		} 
		else if(Input.GetKey(KeyCode.E)){
			rotate = 1;
		} 
		else{
			rotate = 0;
		}
		if(following){
            if (cameraTarget == null) { return; }
			offset = Quaternion.AngleAxis(rotate * rotateSpeed, Vector3.up) * offset;
			transform.position = cameraTarget.transform.position + offset; 
			transform.position = new Vector3(Mathf.Lerp(lastPosition.x, cameraTarget.transform.position.x + offset.x, smoothing * Time.deltaTime), 
				Mathf.Lerp(lastPosition.y, cameraTarget.transform.position.y + offset.y, smoothing * Time.deltaTime), 
				Mathf.Lerp(lastPosition.z, cameraTarget.transform.position.z + offset.z, smoothing * Time.deltaTime));
		} 
		else{
			transform.position = lastPosition; 
		}
		transform.LookAt(cameraTarget.transform.position);
	}

	void LateUpdate(){
		lastPosition = transform.position;
	}
}