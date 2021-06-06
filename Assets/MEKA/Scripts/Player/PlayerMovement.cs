using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;  // Utility scripts

//=============================================

// TODO:

// Optimize raycasting (every usage. not just movement)

// BUG; dodging off ledge sometimes slams player down on the ground below
    // Should be fixed now that vertical velocity is reset

// Dodge:
// give speed + stop fiction?
// Constant slowdown vs slowing after dodging

// Boosting deacceleration based on degree of turn?


//=============================================

//Notes:
// Unity documentation recommends calling charactercontroller.Move() only once per frame -> void Update() 
// But FixedUpdate() is not linked to framerate and therefore should be more consistent
// * Time.deltatime does fix framerate dependency but consistency is also important

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
    private float xMouseSensitivity = 37.0f; // Horizontal sensitivity
    private float yMouseSensitivity = 37.0f; // Vertical sensitivity
    private float mouseYaw = 0.022f; //mouse yaw/pitch. Overwatch = 0.0066, Quake 0.022

    // Camera rotations
    private float mouseY = 0.0f;
    private float mouseX = 0.0f;
    // private Vector3 moveDirectionNorm = Vector3.zero; //unused
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    private float torsoAngle = 0.0f; // Angle between mech torso and legs. Used to limit turnrate
    private float turnrateAngleThreshold = 45.0f; // At what angle turnrate cap kicks in
    private float legsResetRate = 200.0f; // How fast legs reset towards torso
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
    //float walkSpeed = 7.0f; // Walking movement speed
    //float thrusterSpeed = 12.5f; // Movement speed while boosting
    //float walkAcceleration = 14.0f; // Acceleration
    //float walkDeacceleration = 10.0f;
    float jumpSpeed = 8.0f; // The speed at which the character's up axis gains when hitting jump

    float gravity = 25.0f; // Gravity
    float groundFriction = 6; // Ground friction
    #endregion

    private float dodgeTimer = 1.5f; // Starts without cooldown

    private MovementInputs movementInputs; // Player commands
    private CharacterController characterController; // Player controller
    private bool wasOnGround = false; // Was controller on ground last tick?

    [HideInInspector]
    public GUIStyle style; // Debug; for displaying values on screen

    private Debugger debugger;
    public float debugAngle = 0.0f; // Debug variable
    private float debugTurnSpeed = 0.0f;

    //***
    private Vector3 prevPos;

    private Vector3 downForce = Vector3.zero;
    private bool isJumping = false;

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
        var wishTurn = Input.GetAxisRaw("Mouse X") * xMouseSensitivity * Time.deltaTime;
        var wishAngle = torsoAngle + wishTurn;

        if (Mathf.Abs(wishAngle) <= turnrateAngleThreshold)
        {
            // Turn does not go over threshold
            mouseX += wishTurn;
            torsoAngle += wishTurn;

            debugTurnSpeed = wishTurn / Time.deltaTime; // Save debug value
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

                debugTurnSpeed = turnRange / Time.deltaTime; // Save debug value
            }
        }

        // Update debug graphs
        debugTurnSpeed = Mathf.Round(Mathf.Abs(debugTurnSpeed));
        debugger.UpdateDebugTurnRate(debugTurnSpeed);
        debugger.UpdateDebugLegsAngle(Mathf.Abs(torsoAngle));
        debugAngle = torsoAngle; // Save debug value

        // Turn legs towards 0.0 angle
        float resetDirection = torsoAngle < 0.0 ? 1.0f : -1.0f;
        float scaledResetRate = legsResetRate * Time.deltaTime; // Scale reset rate // Note: Forgot to scale angle comparison -> slow turning did not register
        torsoAngle += (Mathf.Abs(torsoAngle) > scaledResetRate) ? (resetDirection * scaledResetRate) : -torsoAngle;

        // Get vertical mouse input
        mouseY -= Input.GetAxisRaw("Mouse Y") * yMouseSensitivity * Time.deltaTime;

        // Clamp the vertical rotation
        mouseY = Mathf.Clamp(mouseY, -90.0f, 90.0f);

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

        movementInputs.wishJump = Input.GetButtonDown("Jump") ? true : movementInputs.wishJump /* false -> jumps are not queued*/;
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

        debugger = GetComponent<Debugger>();
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

#if false
        // Add downforce to fix bouncing
        if (characterController.isGrounded)
        {
            RaycastHit hit; // A raycast hit to get information about what was hit
            var groundLayer = LayerMask.GetMask("Environment");
            var raycastDistance = characterController.height / 1.5f;

            // Raycast downwards
            Ray groundRay = new Ray();
            groundRay.origin = transform.position;
            groundRay.direction = -transform.up;

            /* Doesnt work if ground isnt directly below player
            if (Physics.Raycast(groundRay, out hit, raycastDistance, groundLayer))
            {
                var slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (Mathf.Abs(slopeAngle) <= 35.0f) // Max slope movement will stick to
                {
                    playerVelocity.y = -5000.0f;
                    //Debug.Log(slopeAngle);
                }
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 2.0f); // Draw ground normal
            }*/

            // Gets normal of edge -> incorrect angle
            if(Physics.SphereCast(transform.position, characterController.radius, -transform.up, out hit, characterController.height / 1.5f, groundLayer))
            {
                var slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (Mathf.Abs(slopeAngle) <= 35.0f) // Max slope movement will stick to
                {
                    playerVelocity.y = -5000.0f;
                    //Debug.Log(slopeAngle); // Angle is wrong when pixelwalking

                    /*       /      <-- normal
                     * ____ /    
                     *     |
                     *     |     <-- platform cross-section
                     */
                }
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 2.0f); // Draw ground normal

#if false
                // obtain the normals from the Mesh
                Mesh mesh = hit.transform.GetComponent<MeshFilter>().mesh;
                Vector3[] normals = mesh.normals;
                foreach (var normal in normals)
                {
                    Debug.DrawLine(transform.position, transform.position + normal, Color.green, 2.0f); // Draw ALL normals
                }
#endif
            }
        }

                // Reset vertical velocity when dropping off a platform
        if (!characterController.isGrounded && wasOnGround && playerVelocity.y <= 0.0f)
        {
            playerVelocity.y = -gravity * Time.deltaTime;
        }
#endif

#if false
        // Rotate movement vector to be parallel with the ground
        if (characterController.isGrounded && playerVelocity.magnitude > 5.0f)
        {
            RaycastHit hit; // A raycast hit to get information about what was hit
            var groundLayer = LayerMask.GetMask("Environment");
            var raycastDistance = characterController.height / 1.5f;

            if (Physics.SphereCast(transform.position, characterController.radius, -transform.up, out hit, characterController.height / 1.5f, groundLayer))
            {
                //var slopevec = Vector3.Cross(hit.normal, -transform.right); //fwd dir along slope, not move vec

                var temp = playerVelocity;
                temp.Normalize();
                temp = Quaternion.Euler(0, -90, 0) * temp; // left
                var slopevec = Vector3.Cross(hit.normal, temp);
                var slopeAngle = 90 - Vector3.Angle(Vector3.up, slopevec);
                //if (Mathf.Abs(slopeAngle) <= 35.0f && Mathf.Abs(slopeAngle) >= 5.0f)
                {
                    GetComponent<Debugger>().slopeAngle = slopeAngle;
                    //Debug.DrawLine(transform.position, transform.position + slopevec * 2, Color.red, 2.0f);

                    // Horizontal velocity slows down since total speed stays the same.
                    // TODO; Test if constant horizontal vel feels better
                    //playerVelocity = slopevec * playerVelocity.magnitude;

                    //playerVelocity.y = -1000.0f; // flat -> slope; briefly off the ground

                // Add downforce to motion calcs instead?
                }
            }

            //playerVelocity.y = -1000.0f;
        }
#endif

        //***
        /*if (characterController.isGrounded && !movementInputs.wishJump)
        {
            downForce.y = -100.0f; // Always applying the downforce is smoother. No bump when entering a slope
            // TODO; theres still 1 frame reset delay? obvious with higher force
        }
        // Reset vertical velocity when dropping off a platform
        if (!characterController.isGrounded && downForce.y <= 0.0f)
        {
            downForce.y = 0.0f;
        }*/
        //

        //downForce.y = 0.0f;
        if (characterController.isGrounded)
        {
            //downForce.y = -100.0f; //  See vertical vel graph; sudden dip when walking off a ledge

            // !!! works otherwise fine but theres a dip when walkng off a ledge
        }
        RaycastHit hit; // A raycast hit to get information about what was hit
        var groundLayer = LayerMask.GetMask("Environment");
        var raycastDistance = characterController.height / 1.5f;

        // Raycast downwards
        Ray groundRay = new Ray();
        groundRay.origin = transform.position;
        groundRay.direction = -transform.up;

        // charcontroller check has delay or is too lenient?
        var rayVec = new Vector3(0, raycastDistance, 0);
        Debug.DrawLine(transform.position, transform.position - rayVec, Color.red, 5.0f);
        if (Physics.Raycast(groundRay, out hit, raycastDistance, groundLayer) && characterController.isGrounded)
        {
            //downForce.y = -100.0f;

            // !!! this version bugs only when going down slope when raycast does not touch the ground
            // !! some bugs with dodging off ledges?
            // could try spherecasting predicted position to check if player will stay on the ground
        }
        if (Physics.SphereCast(transform.position, characterController.radius / 1.5f, -transform.up, out hit, characterController.height / 1.5f, groundLayer))
        {
            // same with spherecast of charcontroller size
            // smaller spherecast -> smaller dip
            //if (characterController.isGrounded)
            //downForce.y = -100.0f;
        }

        // spherecast predicted pos // what if in a wall?
        /*if (Physics.SphereCast(transform.position + ((playerVelocity + downForce) * Time.deltaTime), characterController.radius, -transform.up, out hit, characterController.height, groundLayer))
        {
            downForce.y = -100.0f;
        }*/

        //enable on first touch?
        if (characterController.isGrounded && !wasOnGround)
        {
            //downForce.y = -100.0f; //  See vertical vel graph; sudden dip when walking off a ledge
        }
        if (!characterController.isGrounded)
        {
            //downForce.y = 0.0f;
        }

        // Jumps can be queued
        if (characterController.isGrounded && movementInputs.wishJump && dodgeTimer >= 0.25f)
        {
            //playerVelocity.y = jumpSpeed;
            const float jumpHeight = 1.8f;
            playerVelocity.y = Mathf.Sqrt(jumpHeight * 2.0f * gravity);
            movementInputs.wishJump = false;

            downForce.y = 0.0f;
            isJumping = true;
        }
        //Debug.Log(playerVelocity.y);
        //Debug.Log(downForce.y);
        //GetComponent<Debugger>().downForce = downForce.y;

        // Save previous ground state
        wasOnGround = characterController.isGrounded;

        // Move the controller
        characterController.Move((playerVelocity /*+ downForce*/) * Time.deltaTime);

        //***
        // Stick to slopes
        downForce.y = 0.0f;
        RaycastHit hit2; // A raycast hit to get information about what was hit
        var groundLayer2 = LayerMask.GetMask("Environment");
        var slopeForceRayLength = 1.5f; // times distance from origin to ground
        if (!isJumping)
        {
            // w/ raycast; the usual edge bug
            // w/ spherecast; off edge dip bug
            if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength, groundLayer2))
            //if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength, groundLayer))
            {
                
                var temp = playerVelocity;
                temp.Normalize();
                temp = Quaternion.Euler(0, -90, 0) * temp; // left
                var slopevec = Vector3.Cross(hit.normal, temp);
                var slopeAngle = 90 - Vector3.Angle(Vector3.up, slopevec);
                GetComponent<Debugger>().slopeAngle = slopeAngle;
                if (slopeAngle <= -5.0f) // Max slope movement will stick to
                //if (hit.normal != Vector3.up)
                {
                    downForce.y = -100.0f; // down * 100?
                    characterController.Move(downForce * Time.deltaTime);

                    // "It is recommended that you make only one call to Move or SimpleMove per frame."
                    // But slope check after moving works better than downforce
                    // since downforce would have to be 0 before player goes over the edge

                    // camera bob bugs when walking down a slope
                    // platform edges are still buggy
                }
            }
        }
        GetComponent<Debugger>().downForce = downForce.y;

        if (characterController.isGrounded)
        {
            isJumping = false;
        }

        // Debug velocity vector
        // Vel vector and movement should match
        Debug.DrawLine(transform.position, transform.position + playerVelocity * Time.deltaTime, Color.blue, 5.0f);
        Debug.DrawLine(prevPos, transform.position, Color.red, 5.0f);
        prevPos = transform.position;
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
    float thrusterSpeed = 12.5f; // Movement speed while boosting

#if false
    const float thrusterAcceleration = 14.5f;
    float walkDeacceleration = 10.0f; // Deacceleration

    var inputDir = new Vector3(movementInputs.rightMove, 0, movementInputs.forwardMove);
    inputDir = transform.TransformDirection(inputDir);
    inputDir.Normalize();
    inputDir *= thrusterAcceleration * Time.deltaTime;

    var playerVel = playerVelocity; // Copy movement vector
    playerVel.y = 0.0f; // Ignore vertical movement
    /*var wishVector = playerVel + inputDir;
    if (wishVector.magnitude > thrusterSpeed)
    {
        //ApplyFriction(0.25f); // TODO: clean this
        {
            var t = 0.25f; // Scale

            var vec = playerVel;
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

            playerVel.x *= newspeed;
            playerVel.z *= newspeed;
        }
    }
    else
    {
        playerVel += inputDir;
    }*/

        //
        if (playerVel.magnitude > thrusterSpeed)
        {
            //ApplyFriction(0.25f); // TODO: clean this
            {
                var t = 0.25f; // Scale

                var vec = playerVel;
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

                playerVel.x *= newspeed;
                playerVel.z *= newspeed;
            }
        }
        else
        {
            playerVel += inputDir;
            if (playerVel.magnitude > thrusterSpeed)
            {
                playerVel.Normalize();
                playerVel *= thrusterSpeed;
            }
        }
        //

        // Reset the gravity velocity
        playerVel.y = -gravity * Time.deltaTime;

        // Copy
        playerVelocity = playerVel;
#endif

//cant change direction until speed drops below boosting speed
//speed drops slower if boosters are on?
//maybe bit weird for boosting in wrong direction to maintain speed longer
// req correct direction to maintain speed longer?
//or currentvector + wishvector, but speed cant increase if > thrusterspeed
//or reduce speed at constant rate if speed is over thrusterspeed (dodging)

//#if false
        //TEST: direction changes are too immediate?
        const float thrusterAcceleration = 14.5f;

        ApplyFriction(0.25f); // decrease speed
        // ^^ slower deacceleration after dodging (when boosting)

        var inputDir = new Vector3(movementInputs.rightMove, 0, movementInputs.forwardMove);
        inputDir = transform.TransformDirection(inputDir);
        inputDir.Normalize();

        var wishSpeed = inputDir.magnitude; //pointless? idea was * 0 if inputs cancel out?
        wishSpeed *= thrusterSpeed; // but it is * direction anyway. does skip few lines of code tho

        AccelerateTest(inputDir, wishSpeed /*thrusterSpeed*/, thrusterAcceleration); // increase speed till cap

        // Reset the gravity velocity
        playerVelocity.y = -gravity * Time.deltaTime;
//#endif

        //vector + vector
        // angle acceleration penalty?

    }

    //**************************************************
    private void WalkMove()
    {
        float walkSpeed = 7.0f; // Walking movement speed
        float acceleration = 14.0f; // Acceleration

        // Apply friction
        float frictionScale = characterController.isGrounded ? 1.0f : 0.5f;
        ApplyFriction(frictionScale);

        var wishdir = new Vector3(movementInputs.rightMove, 0, movementInputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();

        //***
        // without addspeed is walkspeed, but no speed is added because wishdir is still 0. does skip some calculations tho. REMOVE + RETURN FROM ACCELERATE IF WISHDIR.MAG IS 0
        var wishspeed = wishdir.magnitude; // 0 or 1. pointless? accelerate increases speed if speed is below wishspeed
        wishspeed *= walkSpeed; // 0 or walkSpeed

        if (characterController.isGrounded)
        {
            Accelerate(wishdir, wishspeed, acceleration); // Direction, speed, acceleration

            // Reset the gravity velocity           
            playerVelocity.y = -gravity * Time.deltaTime;

            Vector3 velocityVector = playerVelocity;
            velocityVector.y = 0;
            float horizontalVelocity = velocityVector.magnitude;
            if (horizontalVelocity <= walkSpeed)
            {
                // Apply walking effect to camera
                CameraBobbing();
            }
        }
        else
        {
            Accelerate(wishdir, wishspeed, acceleration / 2);

            // Apply gravity
            playerVelocity.y -= gravity * Time.deltaTime;
        }
    }

    //**************************************************
    private void ApplyFriction(float t)
    {
        float deacceleration = 10.0f; // Deacceleration

        var vec = playerVelocity;
        vec.y = 0.0f;
        var speed = vec.magnitude;

        //***
        // TEST: dodges deaccelerate at static rate, regardless of player inputs
        float frictionScale = characterController.isGrounded ? 1.0f : 0.5f;
        t = speed > 12.5f ? frictionScale : t; // Horizontal velocity > thruster velocity
        //***

        var control = speed < deacceleration ? deacceleration : speed;
        var drop = control * groundFriction * Time.deltaTime * t;

        var newspeed = speed - drop; // Speed after friction
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed; // % of old speed

        playerVelocity.x *= newspeed; // Scale
        playerVelocity.z *= newspeed;
    }

    //**************************************************
    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        //var currentspeed = Vector3.Dot(playerVelocity, wishdir); // Circle strafing
        var movementVector = playerVelocity; // "Normal" acceleration
        movementVector.y = 0;
        var currentspeed = movementVector.magnitude;

        var addspeed = wishspeed - currentspeed; // How much there is room to accelerate
        if (addspeed <= 0)
            return;
        var accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    //**************************************************
    private void AccelerateTest(Vector3 wishdir, float wishspeed, float accel) //TEST
    {
#if false
        var horizontalMovement = playerVelocity;
        horizontalMovement.y = 0;

        var newVector = horizontalMovement + wishdir * accel;
        if (newVector.magnitude > wishspeed)
        {
            newVector.Normalize();
            newVector *= wishspeed;
        }

        newVector.y = playerVelocity.y; // copy vertical vel
        playerVelocity = newVector;
#endif

#if false
        var vec = playerVelocity + wishdir;
        vec.y = 0.0f;
        var speed = vec.magnitude;

        var test = playerVelocity;
        test.y = 0;
        test.Normalize();

        // Or use torso/legs angle for calculating speed penalty

        //var dropScale = Vector3.Angle(test, wishdir) / 10;
        //var drop =  dropScale * accel * Time.deltaTime; // Speed penalty
        //Debug.Log(Vector3.Angle(test, wishdir));
        //var newspeed = (speed + accel * Time.deltaTime) - drop;

        var penalty = Mathf.Abs(torsoAngle / turnrateAngleThreshold); //* accel; // % of acceleration
        Debug.Log(penalty);
        //var newspeed = (speed + accel * Time.deltaTime) - penalty;
        var newspeed = (speed + accel * Time.deltaTime * penalty);
        if (newspeed > wishspeed)
            newspeed = wishspeed;
        newspeed /= speed; // % of old speed

        //vec += wishdir; // New direction

        vec.x *= newspeed; // Scale
        vec.z *= newspeed;

        playerVelocity.x = vec.x;
        playerVelocity.z = vec.z;
#endif
        var movementVector = playerVelocity; // "Normal" acceleration
        movementVector.y = 0;
        var currentspeed = movementVector.magnitude;

        var addspeed = wishspeed - currentspeed; // How much there is room to accelerate
        if (addspeed <= 0)
            return;
        var accelspeed = accel * Time.deltaTime * wishspeed;

        var penalty = Mathf.Abs(torsoAngle) / turnrateAngleThreshold; //* accel; // % of acceleration
        accelspeed -= accelspeed * penalty; // Penalty = % turn limit // NOTE: decreases speed but only at max angle?
        accelspeed = Mathf.Max(0, accelspeed); // just penalty to acceleration
        //accelspeed *= 20.0f; // cant go into negative

        if (accelspeed > addspeed)
            accelspeed = addspeed;

        //Debug.Log(accelspeed); //doesnt work? should reduce acceleration at higher angles // needed abs(angle)

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }

    //**************************************************
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        return;

        // Display ground normal (enable gizmos)
        //var slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
        //Debug.Log(slopeAngle);
        //Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green, 2.0f); // Draw ground normal

//#if false
        // DEBUG/TESTING;

        // Get ground normal
        var hitNormal = hit.normal;

        var startpos = hit.point;
        var normal = hit.normal;
        //Debug.DrawLine(startpos, startpos + normal, Color.green, 2.0f); // Draw ground normal
        //Debug.DrawLine(startpos, startpos + playerVelocity, Color.red, 2.0f); // Draw player movement vector
        //Debug.DrawLine(startpos, startpos + transform.forward * 10, Color.red, 2.0f); // Draw forward vector

        // **********
        // TODO; DEBUG THIS
        var temp = Vector3.Cross(hit.normal, -transform.right); // NOTE; forward slope vec should be V3.Cross(normal, forward)
        Debug.DrawLine(startpos, startpos + temp * 2, Color.red, 2.0f); // camera and player have different forward?
        // *********

        //***************************
        // Slope downwards vector
        var groundParallel = Vector3.Cross(transform.up, normal);
        var slopeParallel = Vector3.Cross(groundParallel, normal);
        //Debug.DrawLine(startpos, startpos + slopeParallel, Color.red, 2.0f);

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
//#endif
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(10, 230, 400, 100), "Turn speed: " + debugTurnSpeed, style);

        GUI.Label(new Rect(10, 250, 400, 100), "Torso/legs angle: " + debugAngle, style);
        
        if (Mathf.Abs(debugAngle) >= turnrateAngleThreshold)
            GUI.Label(new Rect(10, 270, 400, 100), "Turn speed restricted", style);
    }
}