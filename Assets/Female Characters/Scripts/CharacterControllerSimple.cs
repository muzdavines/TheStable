using UnityEngine;
using System.Collections;

public class CharacterControllerSimple : MonoBehaviour
{

    public float speed = 3.0F;
    public float rotateSpeed = 3.0F;

    private float runningFloat = 2f;
    CharacterController controller;
    Animator animator;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        /*  transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);

          Vector3 forward = transform.TransformDirection(Vector3.forward);

         
          controller.SimpleMove(forward * curSpeed);*/

        if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion") ||
            this.animator.GetCurrentAnimatorStateInfo(0).IsName("Jump") ||
            this.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ) 
        {

            float curSpeed = Mathf.Abs(speed * Input.GetAxis("Vertical")) + Mathf.Abs(speed * Input.GetAxis("Horizontal"));
            animator.SetFloat("Speed", curSpeed);

            float angleMedian = 0;
            float arrowsPressed = 0;
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += new Vector3(-1, 0, 0) * speed * Time.deltaTime;
                //transform.eulerAngles = new Vector3(0,270,0);
                angleMedian += 270;
                arrowsPressed++;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += new Vector3(1, 0, 0) * speed * Time.deltaTime;
                //transform.eulerAngles = new Vector3(0, 90, 0);
                angleMedian += 90;
                arrowsPressed++;
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position += new Vector3(0, 0, 1) * speed * Time.deltaTime;
                //    transform.eulerAngles = new Vector3(0, 0, 0);
                if (Input.GetKey(KeyCode.A))
                {
                    angleMedian = 315;
                }
                else
                {
                    arrowsPressed++;
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position += new Vector3(0, 0, -1) * speed * Time.deltaTime;
                //transform.eulerAngles = new Vector3(0, 180, 0);

                angleMedian += 180;
                arrowsPressed++;
            }
            if (arrowsPressed > 0)
            {
                Debug.Log(new Vector3(0, angleMedian / arrowsPressed, 0) + " " + angleMedian + " " + arrowsPressed);
                transform.eulerAngles = new Vector3(0, angleMedian / arrowsPressed, 0);
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger("Attack_1");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Jump");
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            animator.SetTrigger("Attack_2");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            animator.SetTrigger("Death");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetTrigger("Use_1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.SetTrigger("Use_2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.SetTrigger("Use_3");
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("Damage");
        }
    }
}
