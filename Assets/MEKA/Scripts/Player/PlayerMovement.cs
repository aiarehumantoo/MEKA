using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;  // Utility scripts

//=============================================

// TODO:

// BUG; dodging off ledge sometimes slams player down on the ground below

// Dodge:
// give speed + stop fiction?
// Constant slowdown vs slowing after dodging

// Boosting deacceleration based on degree of turn?


//=============================================

//Notes:
// Unity documentation recommends calling charactercontroller.Move() only once per frame -> void Update() 
// But FixedUpdate() is not linked to framerate and therefore should be more consistent

// Serialized variables are visible in the inspector just like public values (regardless if public or not)

//=============================================



// Contains the command the user wishes upon the character
struct MovementInputs
{
    public float forwardMove;
    public float rightMove;

    public bool wishJump;
    public bool thrusterMove;
    public bool dodgeMove;
}

//**************************************************
public class PlayerMovement : MonoBehaviour
{
    #region MouseControls
    //Camera
    private Camera playerView; // Player camera
    private float playerViewYOffset = 0.6f; // The height at which the camera is bound to
    private float xMouseSensitivity = 20.0f; // Horizontal sensitivity
    private float yMouseSensitivity = 20.0f; // Vertical sensitivity
    private float mouseYaw = 0.022f; //mouse yaw/pitch. Overwatch = 0.0066, Quake 0.022

    // Camera rotations
    private float mouseY = 0.0f;
    private float mouseX = 0.0f;
    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    private float torsoAngle = 0.0f; // Angle between mech torso and legs. Used to limit turnrate
    private float turnrateAngleThreshold = 45.0f; // At what angle turnrate cap kicks in
    private float legsResetRate = 1.5f; // How fast legs reset towards torso
    #endregion

    #region HeadBob
    [SerializeField] private float stepInterval;
    //[SerializeField] private bool useFovKick;
    //[SerializeField] private FOVKick fovKick = new FOVKick();
    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
    private Vector3 originalCameraPosition;
    #endregion

    #region MovementVariables
    float walkSpeed = 7.0f; // Walking movement speed
    float thrusterSpeed = 12.5f; // Movement speed while boosting
    float walkAcceleration = 14.0f; // Acceleration
    float walkDeacceleration = 10.0f;
    float jumpSpeed = 8.0f; // The speed at which the character's up axis gains when hitting jump

    float gravity = 25.0f; // Gravity
    float groundFriction = 6; // Ground friction
    #endregion

    private float dodgeTimer = 1.5f; // Starts without cooldown

    private MovementInputs movementInputs; // Player commands
    private CharacterController characterController; // Player controller

    [HideInInspector]
    public GUIStyle style; // Debug; for displaying values on screen

    public float debugAngle = 0.0f; // Debug variable

    //**************************************************
    private void CameraControls()
    {
        // Ensure that the cursor is locked into the screen
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }

        // Get horizontal mouse input
        var wishTurn = Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw;
        var wishAngle = torsoAngle + wishTurn;

        if (Mathf.Abs(wishAngle) <= turnrateAngleThreshold)
        {
            // Turn does not go over threshold
            mouseX += wishTurn;
            torsoAngle += wishTurn;
        }
        else
        {
            bool changedDirection = ((torsoAngle > 0.0f) && (wishTurn < 0.0f)) || ((torsoAngle < 0.0f) && (wishTurn > 0.0f)) ? true : false;

            // Torso cannot exceed max turn angle
            if (Mathf.Abs(torsoAngle) < turnrateAngleThreshold || changedDirection)
            {
                // Calc correct angle & direction for the turn
                float sameSide = turnrateAngleThreshold - Mathf.Abs(torsoAngle);
                float diffSide = turnrateAngleThreshold + Mathf.Abs(torsoAngle);

                var turnRange = ((wishTurn > 0.0f && torsoAngle > 0.0f) || (wishTurn < 0.0f && torsoAngle < 0.0f)) ? sameSide : diffSide;
                mouseX += wishTurn > 0.0f ? turnRange : -turnRange;
                torsoAngle += wishTurn > 0.0f ? turnRange : -turnRange;
            }
        }

        // Turn legs towards 0.0 angle
        debugAngle = torsoAngle; // Save debug value
        float resetDirection = torsoAngle < 0.0 ? 1.0f : -1.0f;
        torsoAngle += (Mathf.Abs(torsoAngle) > legsResetRate) ? (resetDirection * legsResetRate) : -torsoAngle;

        // Get vertical mouse input
        mouseY -= Input.GetAxisRaw("Mouse Y") * yMouseSensitivity * mouseYaw;

        // Clamp the vertical rotation
        if (mouseY < -90)
            mouseY = -90;
        else if (mouseY > 90)
            mouseY = 90;

        // X,Y,Z // Vertical, Horizontal, Tilt
        this.transform.rotation = Quaternion.Euler(0, mouseX, 0); // Rotates the collider
        playerView.transform.rotation = Quaternion.Euler(mouseY, mouseX, 0); // Rotates the camera
    }

    //**************************************************
    private void CameraBobbing()
    {
        var newCameraPosition = playerView.transform.localPosition;
        if (characterController.velocity.magnitude > 0 && characterController.isGrounded)
        {
            playerView.transform.localPosition = headBob.DoHeadBob(characterController.velocity.magnitude);
            newCameraPosition.y = playerView.transform.localPosition.y - jumpBob.Offset();
        }
        else
        {
            newCameraPosition.y = originalCameraPosition.y - jumpBob.Offset();
        }
        playerView.transform.localPosition = newCameraPosition;
    }

    //**************************************************
    private void GetMovementInputs()
    {
        movementInputs.forwardMove = Input.GetAxisRaw("Vertical");
        movementInputs.rightMove = Input.GetAxisRaw("Horizontal");

        movementInputs.thrusterMove = (characterController.isGrounded && Input.GetButton("Boost") && Input.GetAxisRaw("Vertical") > 0.01f) ? true : false;
        movementInputs.dodgeMove = (characterController.isGrounded && Input.GetButton("Boost") && Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.01f && Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f) ? true : false;

        movementInputs.wishJump = Input.GetButtonDown("Jump") ? true : false;
    }

    //**************************************************
    private void Start()
    {
        // Enable camera / audio listener
        playerView = Camera.main;
        playerView.enabled = true;
        playerView.GetComponent<AudioListener>().enabled = true;

        // Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Headbob
        originalCameraPosition = playerView.transform.localPosition;
        headBob.Setup(playerView, stepInterval);

        // Put the camera inside the capsule collider
        playerView.transform.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);

        characterController = GetComponent<CharacterController>();

        // Debug text
        style.normal.textColor = Color.green;
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    //**************************************************
    private void Update()           
    {
        CameraControls();
        MovementControls();
    }

    //**************************************************
    private void MovementControls()
    {
        GetMovementInputs();
        DodgeMove();

        // Boosting
        if (movementInputs.thrusterMove)
        {
            ThrusterMove();
        }
        else
        {
            WalkMove();
        }

        // Add downforce on slopes to fix bouncing
        if (characterController.isGrounded)
        {
            RaycastHit hit; // A raycast hit to get information about what was hit
            var groundLayer = LayerMask.GetMask("Environment");
            var raycastDistance = characterController.height / 1.5f;

            // Raycast downwards
            Ray groundRay = new Ray();
            groundRay.origin = transform.position;
            groundRay.direction = -transform.up;

            if (Physics.Raycast(groundRay, out hit, raycastDistance, groundLayer))
            {
                // Not on flat ground
                var slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (Mathf.Abs(slopeAngle) <= 35.0f) // Max slope movement will stick to
                {
                    playerVelocity.y -= 5000.0f * Time.deltaTime;
                    //Debug.Log(slopeAngle);
                }
            }
        }

        if (characterController.isGrounded && movementInputs.wishJump)
        {
            playerVelocity.y = jumpSpeed;
            movementInputs.wishJump = false;
        }

        // Move the controller
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    //**************************************************
    private void DodgeMove()
    {
        const float dodgeSpeed = 60.0f;
        const float dodgeCooldown = 1.5f;

        dodgeTimer += Time.deltaTime;

        if (movementInputs.dodgeMove && dodgeTimer >= dodgeCooldown)
        {
            if (movementInputs.rightMove > 0.01f)
            {
                playerVelocity += transform.right * dodgeSpeed;
                dodgeTimer = 0;
            }
            if (movementInputs.rightMove < -0.01f)
            {
                playerVelocity -= transform.right * dodgeSpeed;
                dodgeTimer = 0;
            }
        }
    }

    //**************************************************
    private void ThrusterMove()
    {
        const float thrusterAcceleration = 0.05f;

        var inputDir = new Vector3(movementInputs.rightMove, 0, movementInputs.forwardMove);
        inputDir = transform.TransformDirection(inputDir);
        inputDir.Normalize();
        inputDir *= thrusterAcceleration;

        var playerVel = playerVelocity; // Copy movement vector
        playerVel.y = 0.0f; // Ignore vertical movement
        playerVel += inputDir;
        if (playerVel.magnitude > thrusterSpeed)
        {
            playerVel.Normalize();
            playerVel *= thrusterSpeed;
        }
        else
        {
            playerVel += inputDir;
        }

        // Reset the gravity velocity
        //playerVel.y = -gravity * Time.deltaTime; // Zero gravity caused some jitter with earlier system. Fixed now?
        playerVel.y = 0.0f;

        // Copy
        playerVelocity = playerVel;
    }

    //**************************************************
    private void WalkMove()
    {
        // Apply friction
        float frictionScale = characterController.isGrounded ? 1.0f : 0.5f;
        ApplyFriction(frictionScale);

        var wishdir = new Vector3(movementInputs.rightMove, 0, movementInputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= walkSpeed;

        if (characterController.isGrounded)
        {
            Accelerate(wishdir, wishspeed, walkAcceleration);

            // Reset the gravity velocity           
            //playerVelocity.y = -gravity * Time.deltaTime;
            playerVelocity.y = 0.0f;

            Vector3 velocityVector = playerVelocity;
            velocityVector.y = 0;
            float horizontalVelocity = velocityVector.magnitude;
            if (horizontalVelocity <= walkSpeed * 1.2f)
            {
                // Apply walking effect to camera
                CameraBobbing();
            }
        }
        else
        {
            Accelerate(wishdir, wishspeed, walkAcceleration / 2);

            // Apply gravity
            playerVelocity.y -= gravity * Time.deltaTime;
        }
    }

    //**************************************************
    private void ApplyFriction(float t)
    {
        var vec = playerVelocity;
        vec.y = 0.0f;
        var speed = vec.magnitude;
        float drop = 0.0f;

        var control = speed < walkDeacceleration ? walkDeacceleration : speed;
        drop = control * groundFriction * Time.deltaTime * t;

        var newspeed = speed - drop;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    //**************************************************
    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        var currentspeed = Vector3.Dot(playerVelocity, wishdir);
        var addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        var accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    //**************************************************
    void OnControllerColliderHit(ControllerColliderHit hit)
    {

#if false
        // DEBUG/TESTING;

        // Get ground normal
        var hitNormal = hit.normal;

        var startpos = hit.point;
        var normal = hit.normal;
        //Debug.DrawLine(startpos, startpos + normal, Color.green, 2.0f); // Draw ground normal
        //Debug.DrawLine(startpos, startpos + playerVelocity, Color.red, 2.0f); // Draw player movement vector
        //Debug.DrawLine(startpos, startpos + transform.forward * 10, Color.red, 2.0f); // Draw forward vector

        //***************************
        // Slope downwards vector
        var groundParallel = Vector3.Cross(transform.up, normal);
        var slopeParallel = Vector3.Cross(groundParallel, normal);
        Debug.DrawLine(startpos, startpos + slopeParallel, Color.red, 2.0f);

        // Angle of the slope player is standing on
        float currentSlope = Mathf.Round(Vector3.Angle(hit.normal, transform.up));
        Debug.Log(currentSlope);

        // If the slope is on a slope too steep and the player is Grounded the player is pushed down the slope.
        if (currentSlope >= 25f && characterController.isGrounded )
        {
            //isSliding = true;
            transform.position += slopeParallel.normalized / 50;
        }

        // If the player is standing on a slope that isn't too steep, is grounded, as is not sliding anymore we start a function to count time
        /*else if (currentSlope < 45 && MaintainingGround() && isSliding)
        {
            TimePassed();

            // If enough time has passed the sliding stops. There's no need for these last two if statements, the thing works already, but it's nicer to have the player slide for a little bit more once they get back on the ground
            if (currentSlope < 45 && MaintainingGround() && isSliding && timePassed > 1f)
            {
                isSliding = false;
            }
        }*/
        //***************************
#endif
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(10, 250, 400, 100), "Torso/legs angle: " + debugAngle, style);
        
        if (Mathf.Abs(debugAngle) >= turnrateAngleThreshold)
            GUI.Label(new Rect(10, 270, 400, 100), "Turn speed restricted", style);
    }
}