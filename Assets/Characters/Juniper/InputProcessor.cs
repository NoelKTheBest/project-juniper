using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputProcessor : MonoBehaviour
{
    // PREPARE TO MAKE A REMAPPING FEATURE THAT CAN WORK FOR BOTH CURRENT PROJECTS

    public float mouseOffset;

    // Serialized Properties
    public string directionForAttack, attackDirectionOverride = "none";
    public string directionForDash, dashDirectionOverride = "none";
    public string directionForStep, stepDirectionOverride = "none";

    [HideInInspector] public int attackDirection, dashDirection, stepDirection, 
        moveDirection, fxAnimationDirection;
    [HideInInspector] public float hDir, vDir, hDir2, vDir2;
    private float xPoint, yPoint, xPoint2, yPoint2;
    [HideInInspector] public float moveH, moveV, lookX, lookY;
    private bool attack, attack2, proj, proj2, dash, dash2;
    [HideInInspector] public bool abutton, pbutton, dbutton;
    [HideInInspector] public static bool pressedPause;
    [HideInInspector] public Vector2 movement;
    [HideInInspector] public Vector2 lookV;
    [HideInInspector] public Vector3 tempV;
    private Camera cam;
    [HideInInspector] public int controller;
    Vector2 JoyVector;
    
    void Awake()
    {
        //cam = FindObjectOfType<Camera>().GetComponent<Camera>();
        cam = Camera.main;
    }
    
    void Update()
    {
        tempV = CalculatePlayerMouseVector();
        ProcessInputs();

        if (controller == 0)
        {
            if (attackDirectionOverride == "none") directionForAttack = "look";
            else if (attackDirectionOverride != "none") directionForAttack = attackDirectionOverride;

            if (dashDirectionOverride == "none") directionForDash = "move";
            else if (dashDirectionOverride != "none") directionForDash = dashDirectionOverride;

            if (stepDirectionOverride == "none") directionForStep = "look";
            else if (stepDirectionOverride != "none") directionForStep = stepDirectionOverride;
        }
        else if (controller == 1)
        {
            if (attackDirectionOverride == "none") directionForAttack = "move";
            else if (attackDirectionOverride != "none") directionForAttack = attackDirectionOverride;

            if (dashDirectionOverride == "none") directionForDash = "move";
            else if (dashDirectionOverride != "none") directionForDash = dashDirectionOverride;

            if (stepDirectionOverride == "none") directionForStep = "move";
            else if (stepDirectionOverride != "none") directionForStep = stepDirectionOverride;
        }

        DetermineLookDirection();
        if (controller == 1) DetermineMoveDirection();
    }

    // REVIEW THIS FUNCTION. It would be interesting to know how this funciton works.
    // method to calculate mouse direction relative to the player
    Vector3 CalculatePlayerMouseVector()
    {
        Vector3 playerScreenCoord = cam.WorldToScreenPoint(transform.position);

        //Debug.Log("player: " + playerScreenCoord);
        //Debug.Log("mouse: " + Input.mousePosition);

        //will not need z axis
        float mx = Input.mousePosition.x + mouseOffset;
        float my = Input.mousePosition.y;
        //float mz = Input.mousePosition.z;
        float px = playerScreenCoord.x;
        float py = playerScreenCoord.y;
        //float pz = playerScreenCoord.z;
        float xDif, yDif;//, zDif;
        float x, y;//, z;
        float ax, ay;
        float nx, ny;

        Vector3 newMousePos = new Vector3(mx, my, 0);

        //Debug.Log("new mouse: " + newMousePos);

        //using abs value tells us how far away the cursor is from the player in terms of x and y positions
        xDif = mx - px;
        yDif = my - py;
        //zDif = mz - pz;

        ax = Mathf.Abs(mx - px);
        ay = Mathf.Abs(my - py);

        x = transform.position.x + xDif;
        y = transform.position.y + yDif;
        //z = transform.position.z + zDif;

        nx = transform.position.x + ax;
        ny = transform.position.y + ay;

        //joyAngle = Mathf.Atan(lookX / lookY) * (180 / Mathf.PI);

        return new Vector3(x, y, 0);
    }
    
    void ProcessInputs()
    {
        // These if statements are for determining whether we are dealing with a 
        //  controller or keyboard in single player mode or online multiplayer
        if (Input.GetAxisRaw("Horizontal") == 1 || Input.GetAxisRaw("Horizontal") == -1
            || Input.GetAxisRaw("Vertical") == 1 || Input.GetAxisRaw("Vertical") == -1)
        {
            controller = 0;
        }
        else if (Input.GetButton("Fire1"))
        {
            controller = 0;
        }
        else if (Input.GetButton("Fire2"))
        {
            controller = 0;
        }

        if (Input.GetAxis("HJoystick") > 0 || Input.GetAxis("HJoystick") < 0
            || Input.GetAxis("VJoystick") > 0 || Input.GetAxis("VJoystick") < 0)
        {
            controller = 1;
        }
        else if (Input.GetButton("F1Joystick"))
        {
            controller = 1;
        }
        else if (Input.GetButton("F2Joystick"))
        {
            controller = 1;
        }

        hDir = Input.GetAxisRaw("Horizontal");
        vDir = Input.GetAxisRaw("Vertical");

        // Debug.Log(hDir + " : " + vDir);
        hDir2 = Input.GetAxis("HJoystick");
        vDir2 = Input.GetAxis("VJoystick");

        //tempV = tempV / tempV.magnitude;

        xPoint = tempV.normalized.x;
        yPoint = tempV.normalized.y;

        xPoint2 = Input.GetAxis("RSXJoystick");
        yPoint2 = Input.GetAxis("RSYJoystick");
        
        attack = Input.GetButtonDown("Fire1");
        attack2 = Input.GetButtonDown("F1Joystick");
        
        proj = Input.GetButtonDown("Fire3");
        proj2 = Input.GetButtonDown("F3Joystick");

        dash = Input.GetButtonDown("Dash");
        dash2 = Input.GetButtonDown("DJoystick");
        
        // movement and look vectors normalized and tested
        if (controller == 0 && !PauseMenu.isGamePaused)
        {
            moveH = new Vector2(hDir, vDir).normalized.x;
            moveV = new Vector2(hDir, vDir).normalized.y;
            lookX = xPoint;
            lookY = yPoint;
            abutton = attack;
            pbutton = proj;
            dbutton = dash;

            // The components of these vectors can be used interchangeably with each other, 
            //  but not with the variables that make up the vectors.

            // base movement vector
            // vector components already normalized
            movement = new Vector2(moveH, moveV);
            
            // base look vector
            // vector components already normalized
            lookV = new Vector2(lookX, lookY);
        }
        // movement and look vectors normalized and tested
        else if (controller == 1 && !PauseMenu.isGamePaused)
        {
            moveH = new Vector2(hDir2, vDir2).normalized.x;
            moveV = new Vector2(hDir2, vDir2).normalized.y;
            lookX = new Vector2(xPoint2, yPoint2).normalized.x;
            lookY = new Vector2(xPoint2, yPoint2).normalized.y;
            abutton = attack2;
            pbutton = proj2;
            dbutton = dash2;
            //JoyVector = new Vector2(lookY, lookX).normalized; - i don't know what this is for
            
            // base movement vector
            // vector components already normalized
            movement = new Vector2(moveH, moveV);

            // base look vector
            // vector components already normalized
            lookV = new Vector2(lookX, lookY);
        }
    }

    void DetermineLookDirection()
    {
        
        //firepath.right = (IP.tempV - transform.position) + transform.position;

        // The step mechanic will use the same direction as the attack direction. If attack direction is look, then
        //  step direction will use look as well and vise versa.

        // The dash mechanic could use either movement direction or look direction. By default it uses movement
        //  direction.

        // Determine direciton for attacking
        {
            bool rangeX = false;
            bool rangeY = false;

            if (directionForAttack == "look")
            {
                rangeX = (lookX > -0.7f) && (lookX < 0.7f); //can do 0.71 or 0.707 for more precision
                rangeY = (lookY > -0.7f) && (lookY < 0.7f);

                if (lookY > 0 && rangeX)
                {
                    attackDirection = 1;
                }

                if (lookY < 0 && rangeX)
                {
                    attackDirection = 2;
                }

                if (lookX > 0 && rangeY)
                {
                    attackDirection = 3;
                }

                if (lookX < 0 && rangeY)
                {
                    attackDirection = 4;
                }
            }
            else if (directionForAttack == "move")
            {
                rangeX = (moveH > -0.7f) && (moveH < 0.7f);
                rangeY = (moveV > -0.7f) && (moveV < 0.7f);

                if (moveV > 0 && rangeX)
                {
                    attackDirection = 1;
                }

                if (moveV < 0 && rangeX)
                {
                    attackDirection = 2;
                }

                if (moveH > 0 && rangeY)
                {
                    attackDirection = 3;
                }

                if (moveH < 0 && rangeY)
                {
                    attackDirection = 4;
                }
            }
        }

        // Determine direction for dashing
        {
            bool rangeX = false;
            bool rangeY = false;

            if (directionForDash == "look")
            {
                rangeX = (lookX > -0.7f) && (lookX < 0.7f); //can do 0.71 or 0.707 for more precision
                rangeY = (lookY > -0.7f) && (lookY < 0.7f);

                if (lookY > 0 && rangeX)
                {
                    dashDirection = 1;
                    fxAnimationDirection = 3;
                }

                if (lookY < 0 && rangeX)
                {
                    dashDirection = 2;
                    fxAnimationDirection = 4;
                }

                if (lookX > 0 && rangeY)
                {
                    dashDirection = 3;
                    fxAnimationDirection = 3;
                }

                if (lookX < 0 && rangeY)
                {
                    dashDirection = 4;
                    fxAnimationDirection = 4;
                }
            }
            else if (directionForDash == "move")
            {
                rangeX = (moveH > -0.7f) && (moveH < 0.7f);
                rangeY = (moveV > -0.7f) && (moveV < 0.7f);

                if (moveV > 0 && rangeX)
                {
                    dashDirection = 1;
                    fxAnimationDirection = 3;
                }

                if (moveV < 0 && rangeX)
                {
                    dashDirection = 2;
                    fxAnimationDirection = 4;
                }

                if (moveH > 0 && rangeY)
                {
                    dashDirection = 3;
                    fxAnimationDirection = 3;
                }

                if (moveH < 0 && rangeY)
                {
                    dashDirection = 4;
                    fxAnimationDirection = 4;
                }
            }

            
        }

        // Determine direction for stepping
        {
            bool rangeX = false;
            bool rangeY = false;

            if (directionForStep == "look")
            {
                rangeX = (lookX > -0.7f) && (lookX < 0.7f); //can do 0.71 or 0.707 for more precision
                rangeY = (lookY > -0.7f) && (lookY < 0.7f);

                if (lookY > 0 && rangeX)
                {
                    stepDirection = 1;
                }

                if (lookY < 0 && rangeX)
                {
                    stepDirection = 2;
                }

                if (lookX > 0 && rangeY)
                {
                    stepDirection = 3;
                }

                if (lookX < 0 && rangeY)
                {
                    stepDirection = 4;
                }
            }
            else if (directionForStep == "move")
            {
                rangeX = (moveH > -0.7f) && (moveH < 0.7f);
                rangeY = (moveV > -0.7f) && (moveV < 0.7f);

                if (moveV > 0 && rangeX)
                {
                    stepDirection = 1;
                }

                if (moveV < 0 && rangeX)
                {
                    stepDirection = 2;
                }

                if (moveH > 0 && rangeY)
                {
                    stepDirection = 3;
                }

                if (moveH < 0 && rangeY)
                {
                    stepDirection = 4;
                }
            }

        }
    }

    void DetermineMoveDirection()
    {
        bool rangeX = false;
        bool rangeY = false;

        rangeX = (moveH > -0.7f) && (moveH < 0.7f);
        rangeY = (moveV > -0.7f) && (moveV < 0.7f);

        //Debug.Log("in range x: " + rangeX + " in range y: " + rangeY);

        if (moveV > 0 && rangeX)
        {
            moveDirection = 1;
        }

        if (moveV < 0 && rangeX)
        {
            moveDirection = 2;
        }

        if (moveH > 0 && rangeY)
        {
            moveDirection = 3;
        }

        if (moveH < 0 && rangeY)
        {
            moveDirection = 4;
        }

        Debug.Log(moveDirection);
    }

    void OnDrawGizmos()
    {
        cam = Camera.main;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(cam.WorldToScreenPoint(transform.position), Input.mousePosition);
    }
}
