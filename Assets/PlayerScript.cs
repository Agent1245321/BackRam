using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    //Object References
    public GameObject pBody; //player body
    public GameObject pCam; //player camera

    //Camera
        //settings
        public float hSens; //horizontal sensitivity set in editor
        public float vSens; //verticle sensitivity set in editor
        public bool invY; //toggle to invert Y axis when looking

    //values
    private Vector3 pForward;
        private Vector3 pForwardFlat;
    private Vector3 pRight;
    private Vector3 pRightFlat;
        private float invYval; //convert the invY bool into an float 1/-1
        private Vector2 lookInput; //pull the Vector 2 from the input system
    public float lookXClamped; //clamped x value when looking so you cant turn while going fast
    public float lookYClamped; //clamped y value when looking so you cant turn while going fast
        private float vLookAngle; //independant Y axis for camera control

    //Movement
        //settings
    public float maxVelocity;
    public float acceleration;
    public float maxStrafeSpeed;
    public float drag;
    public float stoppingPower;
    //values
    private Vector2 moveInput; //pull the Vector 2 from the input system
    private float xVelocity;
    private float zVelocity;

    private bool isMoving;
    private bool isMovingx;



    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        invYval = invY ? -1 : 1;  //pull value from invY bool, -1 if true, 1 if false
    }

    public void OnLook(InputValue v) //Method calls whenever look input is given (mouse or right stick)
    {
        lookInput = v.Get<Vector2>(); //pulls value from input
    }


    public void OnMove(InputValue v)
    {
        moveInput = v.Get<Vector2>();
      
    }

    

    // Update is called once per frame
    void Update()
    {
        //Camera Control
        lookYClamped = Mathf.Clamp(lookInput.y * vSens, -30 / (Mathf.Abs(zVelocity) * Mathf.Abs(zVelocity) + 1), 30 / (Mathf.Abs(zVelocity) * Mathf.Abs(zVelocity) + 1));
        vLookAngle += lookYClamped * vSens; //adjust verticle angle
        vLookAngle = Mathf.Clamp(vLookAngle, -80f, 80f); //restrain angle from -90 to 90 (so we dont break our neck)
        lookXClamped = Mathf.Clamp(lookInput.x * hSens, -60/(Mathf.Abs(zVelocity) * Mathf.Abs(zVelocity) + 1), 60/(Mathf.Abs(zVelocity) * Mathf.Abs(zVelocity) + 1));
        
        pBody.transform.eulerAngles += new Vector3(0, lookXClamped, 0); // rotate whole player horizontally
        pCam.transform.eulerAngles = pBody.transform.eulerAngles + new Vector3(vLookAngle * invYval, 0, 0); //rotate cam vertically
        pForward = pCam.transform.forward;
        pForwardFlat = new Vector3(pForward.x, 0, pForward.z).normalized;

        pRight = pCam.transform.right;
        pRightFlat = new Vector3(pRight.x, 0, pRight.z).normalized;

        //Movement Control 
        if (Mathf.Abs(moveInput.y) > .5f) { isMoving = true;} else { isMoving = false;}
        if (Mathf.Abs(moveInput.x) > .5f) { isMovingx = true; } else { isMovingx = false; }

        //put state check here later please
        if (isMoving) 
        { 
            if(Mathf.Abs(zVelocity) < maxVelocity) zVelocity += (acceleration * moveInput.y); //if going less than top speed add speed
            else if (zVelocity - maxVelocity > .01f) zVelocity = ((zVelocity - maxVelocity) * .99f ) + maxVelocity; //if exceeding top speed by large margin slow gradually
            else if (zVelocity + maxVelocity < -.01f) zVelocity = ((zVelocity + maxVelocity) * .99f) - maxVelocity; // for moving backwards fast
            else if (moveInput.y > 0 && zVelocity > 0 && zVelocity - maxVelocity <= .01f) zVelocity = maxVelocity;
            else if (moveInput.y < 0 && zVelocity < 0 && zVelocity + maxVelocity >= -.01f) zVelocity = -maxVelocity;// if exceeding top speed by shallow margin set speed to top speed
            else zVelocity *= .99f; //this is a strange failsafe incase a computer player changes the input from -1 to 1 or viceversa in a single frame while at top speed
        }
        else 
        {
            if (Mathf.Abs(zVelocity) > .1f) zVelocity *= stoppingPower; //if going faster than snail slow down by a lot
            else zVelocity = 0; // if going slower than snail stop completely
        }

        if (isMovingx)
        {
            if (Mathf.Abs(xVelocity) < maxStrafeSpeed) xVelocity += (acceleration * moveInput.x); //if going less than top speed add speed
            else if (xVelocity - maxStrafeSpeed > .01f) xVelocity = ((xVelocity - maxStrafeSpeed) * .99f) + maxStrafeSpeed; //if exceeding top speed by large margin slow gradually
            else if (xVelocity + maxStrafeSpeed < -.01f) xVelocity = ((xVelocity + maxStrafeSpeed) * .99f) - maxStrafeSpeed; // for moving backwards fast
            else if (moveInput.x > 0 && xVelocity > 0 && xVelocity - maxStrafeSpeed <= .01f) xVelocity = maxStrafeSpeed; // if exceeding top speed by shallow margin set speed to top speed
            else if (moveInput.x < 0 && xVelocity < 0 && + maxStrafeSpeed >= -.01f) xVelocity = -maxStrafeSpeed;
            else xVelocity *= .99f; //this is a strange failsafe incase a computer player changes the input from -1 to 1 or viceversa in a single frame while at top speed
        }
        else
        {
            if (Mathf.Abs(xVelocity) > .1f) xVelocity *= stoppingPower; //if going faster than snail slow down by a lot
            else xVelocity = 0; // if going slower than snail stop completely
        }
        
    }

    void FixedUpdate()
    {
       pBody.GetComponent<Rigidbody>().linearVelocity = pForwardFlat * zVelocity + pRightFlat * xVelocity + new Vector3(0, pBody.GetComponent<Rigidbody>().linearVelocity.y, 0);
    }
}
