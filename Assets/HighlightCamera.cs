using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightCamera : MonoBehaviour {
    private Camera cam;
    private static HighlightCamera _instance;

    public static HighlightCamera instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<HighlightCamera>();
            }

            return _instance;
        }
    }

    public enum HighlightCameraState {
        Idle,
        Active
    };

    public StableCombatChar focus;
    public HighlightCameraState state;
    void Start() {
        cam = GetComponent<Camera>();
        Idle();
    }

    public void Idle() {
        state = HighlightCameraState.Idle;
        cam.enabled = false;
    }

    public void Update() {
        if (state == HighlightCameraState.Active && focus !=null) {
           transform.position = focus.position + new Vector3(5, 5, 5);
            transform.LookAt(focus.position);
        }
    }

    public bool ShowHighlight(StableCombatChar _focus, float time) {
        if (state == HighlightCameraState.Active) {
            return false;}

        state = HighlightCameraState.Active;
        focus = _focus;
        transform.position = focus.position + new Vector3(5,5,5);
        transform.LookAt(focus.position);
        cam.enabled = true;
        return true;
    }

    IEnumerator HighlightEnd(float time) {
        yield return new WaitForSeconds(time);
        Idle();

    }
    
}
