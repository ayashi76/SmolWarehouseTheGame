using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Notes for reading script.////////////////
//Anything stored within a slash box
//like this is complex code realated to each other
//and helps with organization.
//////////////////////////////////////////
public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    private float moveSpeed; //Stores our movement speed.
    public float walkSpeed;
    public float sprintSpeed;

    //These lines of code controll our drag in different situations.
    public float groundDrag; //Sets how much drag effects our player when they are moving on the ground.
    public float airDrag; //Sets how much drag effects our player when they are moving in the air.

    [Header("Jumping")]
    public float jumpForce; //Sets how much force is used to make the player jump.
    public float jumpCooldown; //Time before we can jump again.
    public float airMultiplier; //Changes movement speed in the air.
    public bool readyToJump; //Defines if we are ready to jump or not if conditions are met.
    public bool autoJumpEnabled; //Lets our player jump automatically if key is held.
    private bool exitingSlope; //This solves some problems with not being able to jump on slopes. Its like magic.

    [Header("Crouching")]
    public float crouchSpeed; //Controls our crouching speed.
    public float crouchYScale; //Controls our player size when crouching.
    float startYScale; //Controls what our hieght when crouching starts as.
    public float scaleSpeed = 2f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space; //Lets us use the spacebar as input for jumping.
    public KeyCode sprintKey = KeyCode.LeftShift; //Lets us use the LeftShift key for sprinting.
    public KeyCode crouchKey = KeyCode.LeftControl; //Luts us use the LeftCtrl key for crouching.

    [Header("Ground Check")]
    public float playerHieght; //simply keeps track of player height.
    public LayerMask Ground; //helps with telling if the player is on the ground when the player touches a object that has the ground layer. This might need more mask layers for other surfaces that dont explicitly use the Ground Layer.
    public LayerMask PickupMask;
    bool grounded; //if on or off dictates whether the player is on the ground or not.

    [Header("SlopeHandling")]
    public float maxSlopeAngle; //This sets how high a slope our player can move on effectively.
    private RaycastHit slopeHit; //Helps with storing information about a slope we are on.

    public Transform orientation; //assigns the orientation object to the script from our player.

    float horizontalInput; //These store input values.
    float verticalInput;

    Vector3 moveDirection; //does some wierd movement shit, I dont really remember, probably related to calculating movement direction.

    Rigidbody rb; //im to damn lazy to type rigidbody in a script.

    [Header("Movement State")]
    public MovementState state; //This stores what state is currently active by the MovementState.

    public enum MovementState //Set of states for movement our player will use.
    {
        walking,
        sprinting,
        crouching,
        air,
    }


    private void Start()
    {
        //contrains our rigid body so it doenst fall over.
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y; //This is used to store our player size after we crouch.
    }


    private void Update()
    {
        //If statements ran everyframe.


        //This shoots a raycast down a short distance from the center of our player to see if it is touching the ground or not.
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHieght * 0.3f + 0.2f, Ground);
        

        //This applies drag if the player is touching the ground/////
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = airDrag;
        /////////////////////////////////////////////////////////////

        

        //this are contains lines of code the run the asociated functions every frame.
        MyInput();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate()
    {
        
        //this runs code stored in the private void MovePlayer function and uses fixed update since we are handeling physics.
        MovePlayer();
    }


    private void MyInput()
    {
        //this sets our player input.
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //This allows us to crouch.
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z); //This causes our player to crouch.
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z);


            if (grounded)
            {
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse); //This moves our player down while crouching.
            }
            
        }

        //This stops us from crouching.
        if (Input.GetKeyUp(crouchKey))
        {
            //transform.localScale = Vector3.Lerp(transform.localScale, startYScale, scaleSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, startYScale, transform.localScale.z), scaleSpeed); //This resets the players size.

            if (grounded)
            {
                //This moves the player up a short distance along with the code above so that the player doesnt intersect with physics objects.
                //transform.position = new Vector3(transform.position.x, transform.position.y + 0.3f, transform.position.z); 
            }
            
        }

        //These if statements allows us to jump./////////////////////////////////////////////////////////////////////////
        if (Input.GetKey(jumpKey) && readyToJump && grounded && autoJumpEnabled) //Requirments needed to be cleared to start a jump with AutoJump.
        {
            readyToJump = false; //Sets readToJump false so we cant jump until it is true again.

            Jump(); //Causes our player to jump by running the Jump function.

            Invoke(nameof(ResetJump), jumpCooldown); //Allows us to call the resetjump function after a set time, this lets us enable autojumping when the space bar is held down.
        }

        if (!autoJumpEnabled) //Requirments needed to be cleared to start a jump with a normal Jump.
        {
            if(Input.GetKeyDown(jumpKey) && readyToJump && grounded && !autoJumpEnabled) //This is called to start a jump.
            {
                readyToJump = false; //Sets readToJump false so we cant jump until it is true again.

                Jump(); //Causes our player to jump by running the Jump function.
            }

            if (Input.GetKey(jumpKey) == false && grounded) //This is called to reset the jump when using autojump.
            {
                Invoke(nameof(ResetJump), jumpCooldown);
            }

        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        



    }


    private void StateHandler()
    {
        //Player Movement State Crouching
        if (grounded && Input.GetKey(KeyCode.LeftControl))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        //Player Movement State Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        //Player Movement State Walking
        else if (grounded && !Input.GetKey(crouchKey))
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //Player Movement State Air
        else if (grounded && !Input.GetKey(crouchKey))
        {
            state = MovementState.air;
        }


    }


    private void MovePlayer()
    {
        //This simply calculated movement direction so we always move in the direction the player is facing.
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //This is used for moving our player on slopes.
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force); //This lets our player move on a slope.

            //This ensures that when our player is going up a slope, at moments when gravity is disabled this keeps the player on the slope.
            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        //Runs if the player is on the ground.
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force); //This adds a force to our player so it can move when it is on the ground.

        //Runs if the player is in the air/ is not grounded ("not" represented in code by !).
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force); //This adds a force to our player so it can move when it is in the air.

        //Turns gravity off when on a slope.
        rb.useGravity = !OnSlope();



    }

    private void SpeedControl()
    {
        //Limiting our speed on slopes.
        if (OnSlope())
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }

        }
        else
        {
            //This lets us get the flat velocity of our player.
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            //This checks if movement speed is exceded then calculates what it should be then sets the speed to the calculated value.
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

    }

    private void Jump()
    {
        exitingSlope = true;

        //Resets y velocity so that the players jump remains consitent.
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);


        //This applys our jump force causing our player to jump.
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);


    }

    private void ResetJump()
    {
        readyToJump = true; //We use this function whenever we need to set readyToJump back to true.

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        //This checks if we are on a slope.
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHieght * 0.3f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0; //This checks if we are on a slope greater than our maxSlopeAngle.

        }

        return false; //This is returned if we are not on a slope.
    }

    //This does some math so that we can convert our movement direction and make it realtive to the slope we are on.
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized; //This runs said calculation to relativize the movement direction on slopes.
    }

    }
