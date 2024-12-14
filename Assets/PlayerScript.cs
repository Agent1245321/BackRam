using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{

    public enum States
    {
        standing,
        sliding,
        rising,
        falling
    }

    
    //Object References
    public GameObject pBody; //player body
    public GameObject pCam; //player camera

    //Camera
    //settings
        public float speedFovSkew;
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
    private float lookXClamped; //clamped x value when looking so you cant turn while going fast
    private float lookYClamped; //clamped y value when looking so you cant turn while going fast
        private float vLookAngle; //independant Y axis for camera control

    //Movement
    //settings
    public float maxVelocity;
    public float acceleration;
    public float maxStrafeSpeed;
    public float drag; // for really fast speeds
    public float stoppingPower; // for regular stopping
    //values
    private Vector2 moveInput; //pull the Vector 2 from the input system
    private float xVelocity;
    private float zVelocity;

    private bool isMoving;
    private bool isMovingx;

    //Sliding
    //settings
        public float slideLength;
        public float slideBoost;
        public float slideRecharge;

    //values
    private float slideCharge;
    private float sliding;

    //logic
    private States s;



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

    public void OnSlide(InputValue v)
    {
        sliding = v.Get<float>();   
        if (slideCharge > 0 && s == States.standing)
        {
            s = States.sliding;
            pCam.transform.position = new Vector3(0, 0.3f, 0) + pBody.transform.position;
            zVelocity *= slideBoost;
        }
       

    }
    

    // Update is called once per frame
    void Update()
    {
        pCam.transform.eulerAngles = pBody.transform.eulerAngles + new Vector3(vLookAngle * invYval, 0, 0); //rotate cam vertically -- horizontal in standing action / sliding action
        Camera.main.fieldOfView = 60f + zVelocity * speedFovSkew; // skew the fov based on speed

        //Camera Control
        lookYClamped = Mathf.Clamp(lookInput.y * vSens, -10 / (Mathf.Abs(zVelocity) + 1), 10 / (Mathf.Abs(zVelocity) + 1));
        vLookAngle += lookYClamped * vSens; //adjust verticle angle
        vLookAngle = Mathf.Clamp(vLookAngle, -80f, 80f); //restrain angle from -90 to 90 (so we dont break our neck)
        lookXClamped = Mathf.Clamp(lookInput.x * hSens, -10/(Mathf.Abs(zVelocity) + 1), 10/(Mathf.Abs(zVelocity) + 1));
        
        
        pForward = pCam.transform.forward;
        pForwardFlat = new Vector3(pForward.x, 0, pForward.z).normalized;

        pRight = pCam.transform.right;
        pRightFlat = new Vector3(pRight.x, 0, pRight.z).normalized;

        //Movement Control 
        if (Mathf.Abs(moveInput.y) > .5f) { isMoving = true;} else { isMoving = false;}
        if (Mathf.Abs(moveInput.x) > .5f) { isMovingx = true; } else { isMovingx = false; }

        //state check to call different actions
       switch (s)
        {
            case States.standing:
                StandingAction();
                break;

            case States.sliding:
                SlidingAction();
                break;

            case States.falling:

                break;

            case States.rising:

                break;

            default:
                StandingAction();
                break;
        }
        
    }

    void FixedUpdate()
    {
        //apply movement to the player
       pBody.GetComponent<Rigidbody>().linearVelocity = pForwardFlat * zVelocity + pRightFlat * xVelocity + new Vector3(0, pBody.GetComponent<Rigidbody>().linearVelocity.y, 0);
    }

    void SlidingAction()
    {
        pBody.transform.eulerAngles += new Vector3(0, lookXClamped * .5f, 0); // rotate whole player horizontally at half

        if (slideCharge > 0 && sliding == 1) //reduces slide charge while sliding
        {
            slideCharge -= .01f;
        }
        else
        {
            //stands the player back up after running out of slide/releasing shift
            s = States.standing;
            pCam.transform.position = new Vector3(0, 0.75f, 0) + pBody.transform.position;
            zVelocity /= slideBoost;
        }
    }
        void StandingAction()
    {
        pBody.transform.eulerAngles += new Vector3(0, lookXClamped, 0); // rotate whole player horizontally

        if (slideCharge < slideLength) slideCharge += slideRecharge;
        else slideCharge = slideLength;
       

        if (isMoving)
        {
            if (Mathf.Abs(zVelocity) < maxVelocity) zVelocity += (acceleration * moveInput.y); //if going less than top speed add speed
            else if (zVelocity - maxVelocity > .01f) zVelocity = ((zVelocity - maxVelocity) * .99f) + maxVelocity; //if exceeding top speed by large margin slow gradually
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
            else if (moveInput.x < 0 && xVelocity < 0 && +maxStrafeSpeed >= -.01f) xVelocity = -maxStrafeSpeed;
            else xVelocity *= .99f; //this is a strange failsafe incase a computer player changes the input from -1 to 1 or viceversa in a single frame while at top speed
        }
        else
        {
            if (Mathf.Abs(xVelocity) > .1f) xVelocity *= stoppingPower; //if going faster than snail slow down by a lot
            else xVelocity = 0; // if going slower than snail stop completely
        }
    }
}
