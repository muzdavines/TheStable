using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;
public class Flythrough: MonoBehaviour
{
    float lookSpeed = 1.0f;
    float moveSpeed = 0.07f;

    float rotationX = 0.0f;
    float rotationY = 0.0f;
    public GameObject sun;
    public Camera mycamera;
    private Quaternion baseSunTransform;
    public float sunX;
    public float sunY;
    public float sunZ;
    public float defaultFov;
    //public PostProcessVolume ppVolume;
    public float baseDOFFocusDistance;
    public float baseAperture;
    public float baseFocalLength;
    private float depthFocus;
    private float apertureValue;
    private float focalLength;

    // Start is called before the first frame update
    void Start()
    {
        mycamera.fieldOfView = defaultFov;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        baseSunTransform = sun.transform.rotation;
        depthFocus = baseDOFFocusDistance;
        apertureValue = baseAperture;
        focalLength = baseFocalLength;
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey("e"))
        {
            sun.transform.Rotate(0, 0.1f, 0, Space.World);
        }
        if (Input.GetKey("q"))
        {
            sun.transform.Rotate(0, -0.1f, 0, Space.World);
        }
        if (Input.GetKey("z"))
        {
            sun.transform.Rotate(-0.1f, 0, 0, Space.World);
        }
        if (Input.GetKey("x"))
        {
            sun.transform.Rotate(0.1f, 0, 0, Space.World);
        }
        if (Input.GetKeyDown("t"))
        {
            sun.transform.eulerAngles = new Vector3(sunX, sunY, sunZ);
        }
        if (Input.GetKey("v"))
        {
            mycamera.fieldOfView -= 0.3f;
        }
        if (Input.GetKey("b"))
        {
            mycamera.fieldOfView += 0.3f;
        }
        if (Input.GetKeyDown("n"))
        {
            mycamera.fieldOfView = defaultFov;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = 0.3f;
        }
        else
        {
            moveSpeed = 0.07f;
        }

        if (Input.GetKey("["))
        {
            depthFocus += 0.1f;
        }
        if (Input.GetKey("]"))
        {
            depthFocus -= 0.1f;
        }
        if (Input.GetKey(";"))
        {
            apertureValue += 0.1f;
        }
        if (Input.GetKey("'"))
        {
            apertureValue -= 0.1f;
        }
        if (Input.GetKey("."))
        {
            focalLength += 1.0f;
        }
        if (Input.GetKey("/"))
        {
            focalLength -= 1.0f;
        }
        if (Input.GetKey("="))
        {
            focalLength = baseFocalLength;
            apertureValue = baseAperture;
            depthFocus = baseDOFFocusDistance;
        }
        /*if (ppVolume)
        {
            DepthOfField pr;
            if (ppVolume.sharedProfile.TryGetSettings<DepthOfField>(out pr))
            {
                pr.focusDistance.value = depthFocus;
                pr.aperture.value = apertureValue;
                pr.focalLength.value = focalLength;
            }
        }*/


        //rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        //rotationY += Input.GetAxis("Mouse Y") * lookSpeed;
        // rotationY = Mathf.Clamp(rotationY, -90, 90);

        //transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        //transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        if (Input.GetAxis("Vertical") != 0 && Input.GetAxis("Horizontal") != 0)
        {
            transform.position += transform.forward * moveSpeed / 1.5f * Input.GetAxis("Vertical");
            transform.position += transform.right * moveSpeed / 1.5f * Input.GetAxis("Horizontal");
        }
        else
        {
            transform.position += transform.forward * moveSpeed * Input.GetAxis("Vertical");
            transform.position += transform.right * moveSpeed * Input.GetAxis("Horizontal");
        }

        if (Input.GetKey("space"))
        {
            transform.position += transform.up * moveSpeed * 0.07f;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += transform.up * moveSpeed * -0.06f;
        }
    }
}
