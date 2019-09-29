using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;   // Networking namespace
using UnityStandardAssets.Utility;  // Utility scripts

// Quake 3 arena
// https://github.com/id-Software/Quake-III-Arena/blob/master/code/game/bg_pmove.c

// CPM dev diary
// http://games.linuxdude.com/tamaps/archive/cpm1_dev_docs/

// Original Quake 3 physics port
// https://github.com/Zinglish/quake3-movement-unity3d/blob/master/CPMPlayer.js

// Source engine movement code
// https://github.com/ValveSoftware/source-sdk-2013/blob/56accfdb9c4abd32ae1dc26b2e4cc87898cf4dc1/sp/src/game/shared/gamemovement.cpp


/*TODO:
 *  
 * Surfing:
 *      when touching ramp, 0 gravity, higher aircontrol airmove()
 *      might need to write source style physics to get rid of w+a/d strafing when surfing
 * 
 * Audio:
 *      proper audio system
 *      remove now unncessary temp. fixes. (since ground check works properly now)
 *      
 * Misc:
 *      Cleaning up unnecessary code / remaking badly made test bits
 */


// Notes:
// To fix playspeed bug, just open Edit>Project settings>Time. Problem with editor?


// Contains the command the user wishes upon the character
struct Inputs
{
    public float forwardMove;
    public float rightMove;
    public float upMove;
}

public class PlayerMovement : MonoBehaviour //NetworkBehaviour
{
    float gravity = 25.0f; //20      // Gravity
    float friction = 6; //6        // Ground friction

    // Q3: players can queue the next jump just before he hits the ground
    private bool wishJump = false;

    // Used to display real time friction values
    private float playerFriction = 0.0f;

    #region Audio
    [Header("Audio")]
    // An array of sounds that will be randomly selected from
    public AudioClip[] m_PlaySounds;        // Used to play sounds
    public AudioClip[] m_JumpSounds;
    public AudioClip[] m_LandingSounds;
    public AudioClip[] m_FootStepSounds;
    private AudioSource m_AudioSource;
    float audioTimer;       //Timer for footsteps
    float timeBetweenFootSteps = 0.3f;
    #endregion

    // Player commands
    private Inputs _inputs;

    #region MouseControls
    [Header("Mouse")]
    //Camera
    public Transform playerView;            // Camera
    public float playerViewYOffset = 0.6f; // The height at which the camera is bound to
    public float xMouseSensitivity = 20.0f;
    public float yMouseSensitivity = 20.0f;

    // Camera rotations
    private float mouseY = 0.0f;
    private float mouseX = 0.0f;
    private Vector3 moveDirectionNorm = Vector3.zero;
    private Vector3 playerVelocity = Vector3.zero;
    private float playerTopVelocity = 0.0f;

    float mouseYaw = 0.022f;     //mouse yaw/pitch. Overwatch = 0.0066, Quake 0.022

    float torsoAngle = 0.0f; // Angle between mech torso and legs. Used to limit turnrate
    float turnrateAngle = 45.0f; // At what point turnrate cap kicks in. (what was the word for this divider?)
    float cappedTurnRate = 1.5f; // Max turn speed (when capped)
    float legsResetRate = 1.5f; // How fast legs reset towards torso
                                
    // use double?

    public GUIStyle style;//for debug
    #endregion

    #region MovementVariables
    //Variables for movement

    // CPM / VQ3
    bool useCPM = false;                        // True = CPM, False = VQ3
    float moveSpeed = 7.0f; //7                     // Ground move speed
    float runAcceleration = 14.0f; //14         // Ground accel
    float runDeacceleration = 10.0f; //10       // Deacceleration that occurs when running on the ground
    float airAcceleration = 2.0f; //2          // Air accel
    float airDecceleration = 2.0f; //2         // Deacceleration experienced when ooposite strafing
    float airControl = 0.3f; //0.3                    // How precise air control is
    float sideStrafeAcceleration = 50.0f; //50  // How fast acceleration occurs to get up to sideStrafeSpeed when
    float sideStrafeSpeed = 1.0f; //1               // What the max speed to generate when side strafing
    float jumpSpeed = 8.0f; //8                // The speed at which the character's up axis gains when hitting jump

    // Source
    float maxVelGround = 7;
    float maxVelAir = 25;

    #endregion

    // Abilities
    float dodgeSpeed = 40.0f;
    float dodgeCooldown = 1.5f;
    float dodgeTimer;




    private CharacterController _controller;



    // TESTING
    //=================

    // Headbob
    bool useHeadBob = false;
    //[SerializeField] private bool useFovKick;                                         // Serialized variables are visible in the inspector just like public values (regardless if public or not)
    //[SerializeField] private FOVKick fovKick = new FOVKick();
    [SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
    [SerializeField] private float stepInterval;
    private Vector3 originalCameraPosition;

    // Doublejump
    float timer = 1.0f;                             // Timer.
    float doubleJumpWindow = 0.6f;                  // How long player has time to perform second,  higher jump.
    float doubleJumpSpeed = 15.0f;


    // Surfing
    private bool isSurfing = false; // is on a slope or not
    public float slideFriction = 0.3f; // ajusting the friction of the slope
    private Vector3 hitNormal; //orientation of the slope.
    float speed = 1f;

    float slideSpeed = 6f;

    //=================


    // Tag of the object player is standing on
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Print object name
        Debug.Log("Standing on: " + hit.collider.tag);

        // Get ground normal
        hitNormal = hit.normal;
    }

    void SurfMove()
    {
        // Sliding on slopes,no player input
        if (isSurfing)          //unncessary, already in update()
        {
            playerVelocity.x += (1f - hitNormal.y) * hitNormal.x * (speed - slideFriction);
            playerVelocity.z += (1f - hitNormal.y) * hitNormal.z * (speed - slideFriction);
        }

        // player input towards the ramp negates sliding down
        // ie looking directly forward + holding down left movement key while ramp is on the left side
        // then controlling direction with camera angle
        // surfing with upwards angle reduces speed, downwards angle accelerates


        //???
        // + aircontrol from airmove()
        // or just sliding on a slope + airmove when not touching it
        // or airmove() + add sliding calculations to end of it if player is touching a slope
    }

    private void UpdateCameraPosition()
    {
        Vector3 newCameraPosition;
        if (!useHeadBob)
        {
            return;
        }
        if (_controller.velocity.magnitude > 0 && _controller.isGrounded)
        {
            playerView.transform.localPosition = headBob.DoHeadBob(_controller.velocity.magnitude);
            newCameraPosition = playerView.transform.localPosition;
            newCameraPosition.y = playerView.transform.localPosition.y - jumpBob.Offset();
        }
        else
        {
            newCameraPosition = playerView.transform.localPosition;
            newCameraPosition.y = originalCameraPosition.y - jumpBob.Offset();
        }

        playerView.transform.localPosition = newCameraPosition;


        // + play walking sound on each step
    }




    private void Start()
    {
        /*if (!isLocalPlayer)
        {
            return;
        }*/

        // Headbob
        originalCameraPosition = playerView.transform.localPosition;
        headBob.Setup(playerView.GetComponent<Camera>(), stepInterval);

        // Enable camera / audio listener. This way these are active only on local player
        //playerView.gameObject.SetActive(true);
        playerView.gameObject.GetComponent<Camera>().enabled = true;
        playerView.gameObject.GetComponent<AudioListener>().enabled = true;

        // Hide the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Put the camera inside the capsule collider
        playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);

        m_AudioSource = GetComponent<AudioSource>();
        _controller = GetComponent<CharacterController>();

        Settings();
    }

    private void Update()           // Unity documentation recommends calling charactercontroller.move only once per frame -> void Update() but FixedUpdate() is not linked to framerate and therefore should be more consistent
    //private void FixedUpdate()
    {
        /*if (!isLocalPlayer)
        {
            return;
        }*/

        #region MouseControls

        /* Ensure that the cursor is locked into the screen */
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetButtonDown("Fire1"))
                Cursor.lockState = CursorLockMode.Locked;
        }

        /* Camera rotation stuff, mouse controls this shit */
        mouseY -= Input.GetAxisRaw("Mouse Y") * yMouseSensitivity * mouseYaw;
        //mouseX += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw

        float horizontalDifference = 0.0f;
        horizontalDifference += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw;



        // TODO
        // leg reset speed should be low enough that player will soon hit angle limit
        // if he keeps turning faster than capped turn speed

        // correct axis naming?


        //if capped ->
        if (Mathf.Abs(torsoAngle) < turnrateAngle) // Is within limits
        {
            mouseX += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw;
            torsoAngle += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw;
        }
        else // Capped turnrate
        {
            if (Mathf.Abs(horizontalDifference) < cappedTurnRate) // Is below maximum turning speed
            {
                mouseX += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw;
                torsoAngle += Input.GetAxisRaw("Mouse X") * xMouseSensitivity * mouseYaw;
            }
            else
            {
                if (horizontalDifference > 0) // cap turn speed
                {
                    mouseX += cappedTurnRate;
                    torsoAngle += cappedTurnRate;
                }
                if (horizontalDifference < 0)
                {
                    mouseX -= cappedTurnRate;
                    torsoAngle -= cappedTurnRate;
                }
            }
        }

        // Legs always turn towards torso at constant rate
        if (torsoAngle < 0.0)
        {
            if (torsoAngle < legsResetRate)
            {
                torsoAngle += legsResetRate;
            }
            else
            {
                torsoAngle = 0.0f;
            }
        }
        else if (torsoAngle > 0.0)
        {
            if (torsoAngle > legsResetRate)
            {
                torsoAngle -= legsResetRate;
            }
            else
            {
                torsoAngle = 0.0f;
            }
        }

        // Clamp the vertical rotation
        if (mouseY < -90)
            mouseY = -90;
        else if (mouseY > 90)
            mouseY = 90;

        // X,Y,Z // Vertical, Horizontal, Tilt
        this.transform.rotation = Quaternion.Euler(0, mouseX, 0); // Rotates the collider
        playerView.rotation = Quaternion.Euler(mouseY, mouseX, 0); // Rotates the camera

        #endregion

        #region Movement

        Dodge();
        QueueJump();

        // Add the time since Update was last called to the timer. Count up to 1 second.
        if (timer < 1.0f)
        {
            timer += Time.deltaTime;
        }

        if (audioTimer < timeBetweenFootSteps)
        {
            audioTimer += Time.deltaTime;
        }

        if (isSurfing)
        {
            SurfMove();
        }
        else if (_controller.isGrounded)
        {
            GroundMove();
        }
        else if (!_controller.isGrounded)
        {
            AirMove();
        }

        //Clean up timers,sounds,player state check^^^^^^^^^^^^^^^^^^


        // Move the controller
        _controller.Move(playerVelocity * Time.deltaTime);

        if (_controller.isGrounded) //ground
        {
            // On a slope, angle of ground normal
            isSurfing = Vector3.Angle(Vector3.up, hitNormal) >= _controller.slopeLimit;     // or use float slopeLimit instead of value set in charactercontroller
        }
        else //air
        {
            isSurfing = false;
        }

        //Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
        // Set the camera's position to the transform
        //playerView.position = new Vector3(transform.position.x, transform.position.y + playerViewYOffset, transform.position.z);

        UpdateCameraPosition();

        #endregion
    }

    public void KnockBack(Vector3 knockback)
    {
        // Apply knockback
        playerVelocity += knockback;
    }

    public void JumpPad(Vector3 dir)
    {
        // Change player velocity
        playerVelocity = dir;
    }

    // Dodge ability. Fast slide to current direction.
    // Should work well for singleplayer w/ melee/projectile/telegraphed,charged hitscan enemies (dodgeable attacks)
    // Playtest if feels okay for player vs player
    // Additional restrictions so that it cant be chained with strafe jumping? Could work well but also makes circle jumping pointless
    private void Dodge()
    {
        dodgeTimer += Time.deltaTime;

        if (_controller.isGrounded)
        {
            if (Input.GetButton("Fire2") && dodgeTimer >= dodgeCooldown)
            {
                // Current direction (normalized) * force
                playerVelocity = playerVelocity.normalized * dodgeSpeed;
                dodgeTimer = 0;
            }
        }
    }

    private void SetMovementDir()
    {
        _inputs.forwardMove = Input.GetAxisRaw("Vertical");
        _inputs.rightMove = Input.GetAxisRaw("Horizontal");
    }

    private void QueueJump()
    {
        if (Input.GetButtonDown("Jump") && !wishJump)
        {
            wishJump = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            wishJump = false;
        }
    }

    private void GroundMove()
    {
        //GroundMoveSource(playerVelocity); // Current velocity of the player, before any calculations
        //return;

        Vector3 wishdir;

        // Do not apply friction if the player is queueing up the next jump
        if (!wishJump)
            ApplyFriction(1.0f);
        else
            ApplyFriction(0);

        SetMovementDir();

        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        var wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        Accelerate(wishdir, wishspeed, runAcceleration);

        // Reset the gravity velocity           
        playerVelocity.y = -gravity * Time.deltaTime;

        if (wishJump)
        {
            /*
            if (timer <= doubleJumpWindow)
            {
                // Doublejump
                playerVelocity.y = doubleJumpSpeed;
            }
            else if (timer >= doubleJumpWindow)
            {
                // Reset timer
                timer = 0f;
                // Normal jump
                playerVelocity.y = jumpSpeed;
            }
            */
            playerVelocity.y = jumpSpeed;               // doublejump disabled for now

            PlayJumpSound();
            wishJump = false;
        }
    }

    private void AirMove()
    {
        //AirMoveVQ3();
        //AirMoveSource(playerVelocity);
        //return;

        Vector3 wishdir;
        float wishvel = airAcceleration;
        float accel;

        SetMovementDir();

        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;

        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        // CPM: Aircontrol
        if (useCPM)
        {
            float wishspeed2 = wishspeed;
            if (Vector3.Dot(playerVelocity, wishdir) < 0)
                accel = airDecceleration;
            else
                accel = airAcceleration;
            // If the player is ONLY strafing left or right
            if (_inputs.forwardMove == 0 && _inputs.rightMove != 0)
            {
                if (wishspeed > sideStrafeSpeed)
                    wishspeed = sideStrafeSpeed;
                accel = sideStrafeAcceleration;
            }

            Accelerate(wishdir, wishspeed, accel);
            if (airControl > 0)
                AirControl(wishdir, wishspeed2);
            // !CPM: Aircontrol
        }
        else // VQ3
        {
            Accelerate(wishdir, wishspeed, airAcceleration);
        }

        // Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    private void AirMoveVQ3() //Q3 PM_AirMove
    {
        Vector3 wishdir;

        SetMovementDir();

        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);

        // Changing the order here results in different acceleration!!!
        // merge both so that acceleration is the same and only difference is in air control
        // Different for VQ3 and CPM?
        // double check source codes for order of input calculations. Groundmove, Airmove CPM & VQ3
        wishdir.Normalize();
        moveDirectionNorm = wishdir;
        float wishspeed = wishdir.magnitude;
        wishspeed *= moveSpeed;
        //=============

        Accelerate(wishdir, wishspeed, airAcceleration);  

        // Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;

    }

    private void AirControl(Vector3 wishdir, float wishspeed)
    {
        float zspeed;
        float speed;
        float dot;
        float k;

        // Can't control movement if not moving forward or backward
        if (Mathf.Abs(_inputs.forwardMove) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
            return;
        zspeed = playerVelocity.y;
        playerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        speed = playerVelocity.magnitude;
        playerVelocity.Normalize();

        dot = Vector3.Dot(playerVelocity, wishdir);
        k = 32;
        k *= airControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down
        if (dot > 0)
        {
            playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
            playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
            playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

            playerVelocity.Normalize();
            moveDirectionNorm = playerVelocity;
        }

        playerVelocity.x *= speed;
        playerVelocity.y = zspeed; // Note this line
        playerVelocity.z *= speed;
    }

    private void ApplyFriction(float t)
    {
        Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();
        float speed;
        float newspeed;
        float control;
        float drop;

        vec.y = 0.0f;
        speed = vec.magnitude;
        drop = 0.0f;

        /* Only if the player is on the ground then apply friction */
        if (_controller.isGrounded)
        {
            control = speed < runDeacceleration ? runDeacceleration : speed;
            drop = control * friction * Time.deltaTime * t;
        }

        newspeed = speed - drop;
        playerFriction = newspeed;
        if (newspeed < 0)
            newspeed = 0;
        if (speed > 0)
            newspeed /= speed;

        playerVelocity.x *= newspeed;
        playerVelocity.z *= newspeed;
    }

    private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
    {
        float addspeed;
        float accelspeed;
        float currentspeed;

        currentspeed = Vector3.Dot(playerVelocity, wishdir);
        addspeed = wishspeed - currentspeed;
        if (addspeed <= 0)
            return;
        accelspeed = accel * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        playerVelocity.x += accelspeed * wishdir.x;
        playerVelocity.z += accelspeed * wishdir.z;
    }



    public void Settings()
    {
        //Read & copy settings file OnStart + when closing menu
    }


    #region Sounds

    // Change audio + play sound

    private void PlayJumpSound()
    {
        // Play random jump audio
        m_PlaySounds = m_JumpSounds;
        PlayRandomAudio();
    }

    private void PlayLandingSound()
    {
        return;

        //Debug.Log("landing sound");
        //m_PlaySounds = m_LandingSounds;       // no unique landing sounds yet
        m_PlaySounds = m_JumpSounds;
        //PlayRandomAudio();
    }

    private void PlayFootStepAudio()
    {
        return;

        //Debug.Log("footstep sound");
        m_PlaySounds = m_JumpSounds;
        //m_PlaySounds = m_FootStepSounds;      // no unique footstep sounds yet
        PlayRandomAudio();
    }

    private void PlayLadderAudio()
    {

    }

    private void PlayRandomAudio()
    {
        // pick & play a random jump sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_PlaySounds.Length);
        m_AudioSource.clip = m_PlaySounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_PlaySounds[n] = m_PlaySounds[0];
        m_PlaySounds[0] = m_AudioSource.clip;
    }

    #endregion



    // Source movement test
    //=============================
    // TODO;
    // VQ3/CPM variable names
    // Might need alternate movement variables instead of using VQ3 settings

    //private void GroundMoveSource(Vector3 accelDir, Vector3 prevVelocity)
    private void GroundMoveSource(Vector3 prevVelocity)
    {
        Vector3 wishdir;    // normalized direction that the player has requested to move (taking into account the movement keys and look direction)
        SetMovementDir();
        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        // Apply Friction
        float speed = prevVelocity.magnitude;
        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * friction * Time.fixedDeltaTime;
            prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }

        //AccelerateSource(accelDir, prevVelocity, runAcceleration, maxVelGround);
        AccelerateSource(wishdir, prevVelocity, runAcceleration, maxVelGround);

        // Reset the gravity velocity           
        playerVelocity.y = -gravity * Time.deltaTime;

        if (wishJump) //jump
        {
            playerVelocity.y = jumpSpeed;
            PlayJumpSound();
            wishJump = false;
        }
    }

    //private void AirMoveSource(Vector3 accelDir, Vector3 prevVelocity)
    private void AirMoveSource(Vector3 prevVelocity)
    {
        Vector3 wishdir;    // normalized direction that the player has requested to move (taking into account the movement keys and look direction)
        SetMovementDir();
        wishdir = new Vector3(_inputs.rightMove, 0, _inputs.forwardMove);
        wishdir = transform.TransformDirection(wishdir);
        wishdir.Normalize();
        moveDirectionNorm = wishdir;

        //AccelerateSource(accelDir, prevVelocity, airAcceleration, maxVelAir);
        AccelerateSource(wishdir, prevVelocity, airAcceleration, maxVelAir);

        // Apply gravity
        playerVelocity.y -= gravity * Time.deltaTime;
    }

    private void AccelerateSource(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity)
    {
        float projVel = Vector3.Dot(prevVelocity, accelDir); // Vector projection of Current velocity onto accelDir.
        float accelVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

        // If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
        if (projVel + accelVel > max_velocity)
        {
            accelVel = max_velocity - projVel;
        }

        //return prevVelocity + accelDir * accelVel;

        //Set player velocity
        //playerVelocity.x += accelVel * accelDir.x;
        //playerVelocity.z += accelVel * accelDir.z;
        playerVelocity = prevVelocity + accelVel * accelDir;
    }

    //=============================

    private void OnGUI()
    {

        GUI.Label(new Rect(10, 250, 400, 100), "Torso/legs angle: " + torsoAngle, style);
        
        if (Mathf.Abs(torsoAngle) > turnrateAngle)
            GUI.Label(new Rect(10, 270, 400, 100), "Turn speed restricted", style);


    }

}