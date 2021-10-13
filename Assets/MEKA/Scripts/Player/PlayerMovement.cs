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

    //private Debugger debugger;
    public float debugAngle = 0.0f; // Debug variable
    private float debugTurnSpeed = 0.0f;

    //***
    private Vector3 prevPos;

    private Vector3 downForce = Vector3.zero;

    //**************************************************
    public void SetSensitivity(float sensitivity)
    {
        xMouseSensitivity = sensitivity;
        yMouseSensitivity = sensitivity;
    }

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
        Debugger.UpdateDebugTurnRate(debugTurnSpeed);
        Debugger.UpdateDebugLegsAngle(Mathf.Abs(torsoAngle));
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

        //debugger = GetComponent<Debugger>();
        Debugger.SetCharacterController(characterController);
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

        // Handling slopes & pixel walking;
        // #1; Apply downforce when player is grounded
        //      Player sticks to edges
        // #2; Grounded + raycast
        //      Pixelwalk on slopes does not work
        // #3; 1 + first air frame playerVelocity.y = characterController.velocity.y;
        //      Sticks to edges but transition to falling is smooth
        // #4; onground + raycast fails -> pixelwalking
        //      Get edge normal & apply force to that direction
        // #5; Raycast left/right sides
        //      Sticks to edges when walking sideways. #7 works way better

        // #6; Calc edge vector from its normal and compare with players direction vector
        //      Do not apply downforce if movement dir is towards the edge
        //      + Check for pixel walk first
        //      Dir = (normal.x, 0, normal.z)
        //      edge vector = dir.left //not needed?

        // #7; Raycast velocity vectors sides at predicted position
        //      Sticks to edges when aproaching at an angle
        //      Spherecast / charctrl ground check does not work well since player will
        //          always stick to edges
        //      Restrict player to hit`s height? Stops edge "sinking"

        // #8; Get edge normal
        //      add reversed slope normal to edges normal
        // --> slope dir

        // ##
        // Edge normal changes depending on what part of charctrl is touching it
        // -> accurate edgedir vector isnt possible?


        //***
        //var color = Color.blue;
        //downForce.y = 0.0f;
        /*if (characterController.isGrounded)
        {
            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(transform.position, Vector3.down, characterController.height, groundLayer))
            {
                color = Color.red;
                downForce.y = -9.81f;
            }
        }*/
        /*else if ( wasOnGround)
        {
            playerVelocity.y = characterController.velocity.y;
        }*/

        /*if (characterController.isGrounded)
        {
            // Raycast down from both sides to check for pixel walking
            var leftDir = Vector3.left;
            leftDir = transform.TransformDirection(leftDir);
            leftDir.Normalize();
            leftDir *= characterController.radius;

            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(transform.position + leftDir, Vector3.down, characterController.height, groundLayer) ||
               Physics.Raycast(transform.position - leftDir, Vector3.down, characterController.height, groundLayer))
            {
                color = Color.red;
                downForce.y = -9.81f;
            }
        }*/
        //***
        /*if (characterController.isGrounded)
        {
            color = Color.red;
            downForce.y = -9.81f;

            var groundLayer = LayerMask.GetMask("Environment");
            if (!Physics.Raycast(transform.position, Vector3.down, characterController.height, groundLayer))
            {
                // > Pixel walking
                GetComponent<Debugger>().pixelWalking = true;

                RaycastHit hit;
                if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                {
                    //var normal = hit.transform.TransformDirection(hit.normal);

                    //var dropDir = new Vector3(hit.normal.x, 0, hit.normal.z);
                    //var dropDir = new Vector3(normal.x, 0, normal.z);
                    //dropDir = hit.normal;
                    //dropDir = normal;
                    //dropDir = hit.transform.TransformDirection(dropDir);
                    //Debug.DrawLine(hit.point, hit.point + dropDir, color, 5.0f);

                    Vector3 dropDir = Vector3.zero;
                    dropDir.x = hit.normal.x;
                    //dropDir.y = hit.normal.y;
                    dropDir.z = hit.normal.z;
                    //dropDir.Normalize();
                    //Vector3 pos = new Vector3(0, 5, -20);
                    //Debug.DrawLine(pos, pos + dropDir, color, 5.0f);
                    Debug.DrawLine(hit.point, hit.point + dropDir, color, 5.0f);

                    // Remember; hit.point = point of collision; hit.transform.position = location of hit target

                    //var edge = Quaternion.Euler(0, -90, 0) * dropDir; // left
                    //Debug.DrawLine(hit.point, hit.point + edge, color, 5.0f);

                    // Moving towards the drop
                    var moveDir = playerVelocity;
                    moveDir.y = 0;
                    if (Vector3.Dot(dropDir, moveDir) > 0.1f)
                    {
                        color = Color.red;
                        downForce.y = 0.0f;
                    }

                    // TODO;
                    // dropdir is wrong on slopes

                    Vector3 dir = hit.normal;
                }
            }
            else
            {
                GetComponent<Debugger>().pixelWalking = false;

                // what if pixel walking with platform below
            }
        }*/

        //***
        /*if (characterController.isGrounded)
        {
            var groundLayer = LayerMask.GetMask("Environment");
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height, groundLayer))
            {
                var slopeParallel = Vector3.Cross(-transform.right, hit.normal);

                // Slope vector (player`s forward dir)
                Debug.DrawLine(transform.position, transform.position + slopeParallel, color, 5.0f);
            }
        }*/

        //***

        // Sticks to edges whem approacing at angle but otherwise no bugs?
        var horizontalVelocity = playerVelocity;
        horizontalVelocity.y = 0;
        if (characterController.isGrounded && horizontalVelocity.magnitude > 0.0f)
        {
            Vector3 fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            //fwdDir.Normalize(); //used for predicted pos calc > normalize just for this calc
            Vector3 sideDir = Vector3.Cross(fwdDir.normalized, Vector3.up);
            sideDir *= characterController.radius;

            var predictedPos = transform.position + fwdDir * Time.deltaTime;

            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(predictedPos + sideDir, Vector3.down, characterController.height, groundLayer) ||
                Physics.Raycast(predictedPos - sideDir, Vector3.down, characterController.height, groundLayer))
            {
                //color = Color.red;
                //downForce.y = -9.81f; // *delta
            }

            var test = Vector3.down * characterController.height / 2;
            //Debug.DrawLine(predictedPos + sideDir, predictedPos + sideDir +test, color, 5.0f);
            //Debug.DrawLine(predictedPos - sideDir, predictedPos - sideDir +test, color, 5.0f);
            //Debug.DrawLine(predictedPos - sideDir, predictedPos + sideDir, color, 5.0f);
        }

        //***
        if (characterController.isGrounded)
        {
            //color = Color.red;
            //downForce.y = -9.81f;

            var groundLayer = LayerMask.GetMask("Environment");
            if (!Physics.Raycast(transform.position, Vector3.down, characterController.height, groundLayer))
            {
                // > Pixel walking
                //GetComponent<Debugger>().pixelWalking = true;

                RaycastHit hit;
                Vector3 edgeNormal;
                Vector3 slopeNormal;
                Vector3 fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z);
                Vector3 sideDir = Vector3.Cross(fwdDir.normalized, Vector3.up);
                sideDir *= characterController.radius;
                sideDir *= 1.5f;
                /*if (Physics.Raycast(transform.position + sideDir, Vector3.down, out hit, characterController.height, groundLayer))
                {
                    edgeNormal = hit.normal;
                    var hitpos = hit.point;

                    // TESTING; not the best way to get the slope normal
                    if (Physics.Raycast(transform.position + sideDir *1.5f, Vector3.down, out hit, characterController.height, groundLayer))
                    {
                        // Edge`s normal - slope`s normal
                        var edgeDir = edgeNormal - hit.normal;

                        Debug.DrawLine(hitpos, hitpos + edgeDir * 2, color, 5.0f);
                    }

                }
                else if (Physics.Raycast(transform.position - sideDir, Vector3.down, out hit, characterController.height, groundLayer))
                {
                    edgeNormal = hit.normal;

                    // TESTING; not the best way to get the slope normal
                    if (Physics.Raycast(transform.position - sideDir * 1.5f, Vector3.down, out hit, characterController.height, groundLayer))
                    {
                        // Edge`s normal - slope`s normal
                        var edgeDir = edgeNormal - hit.normal;
                    }
                }*/
                if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                {
                    edgeNormal = hit.normal;
                    var pos = hit.point;

                    //Debug.DrawLine(pos, pos + edgeNormal, Color.black, 5.0f);

                    if (Physics.Raycast(transform.position + sideDir, Vector3.down, out hit, characterController.height, groundLayer))
                    {
                        slopeNormal = hit.normal;
                        var edgeDir = edgeNormal.normalized - slopeNormal.normalized;
                        edgeDir.y = 0;
                        //Debug.DrawLine(pos, pos + edgeDir, Color.green, 5.0f);
                    }
                    else if (Physics.Raycast(transform.position - sideDir, Vector3.down, out hit, characterController.height, groundLayer))
                    {
                        slopeNormal = hit.normal;
                        var edgeDir = edgeNormal.normalized - slopeNormal.normalized;
                        edgeDir.y = 0;
                        //Debug.DrawLine(pos, pos + edgeDir, Color.green, 5.0f);
                    }
                }
            }
            else
            {
                //GetComponent<Debugger>().pixelWalking = false;
            }
        }

        //***
        // Disable downforce on first edge frame
        if (characterController.isGrounded)
        {
            //color = Color.red;
            //downForce.y = -9.81f;
            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(transform.position, Vector3.down, characterController.height, groundLayer))
            {
                var fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z);
                var predictedPos = transform.position + fwdDir * Time.deltaTime;
                if (!Physics.Raycast(predictedPos, Vector3.down, characterController.height, groundLayer))
                {
                    //color = Color.blue;
                    //downForce.y = 0.0f;
                    //playerVelocity.y = 0.0f;
                }
            }
        }

        //***
#if false
        // Apply downforce if player is on the ground or pixel walking parallel with the ground
        if (characterController.isGrounded)
        {
            Vector3 fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z);
            var predictedPosition = transform.position + fwdDir * Time.deltaTime;
            //predictedPosition = transform.position;

            RaycastHit hit;
            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(/*transform.position*/ predictedPosition, Vector3.down, characterController.height, groundLayer))
            {
                color = Color.red;
                downForce.y = -9.81f;
            }
            else if (Physics.SphereCast(/*transform.position*/ predictedPosition, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
            {
                // Get direction of the edge
                //var closestPoint = hit.collider.bounds.ClosestPoint(transform.position); // Closest point on bounding box disregards shape of the ground //is not rotated //also doesnt work with complex shapes
                //var closestPoint = hit.collider.ClosestPoint(/*transform.position*/ predictedPosition); // wrong on slopes coz y; needs to be horizontally closest pos

                //*** EDGE VECTOR TEST
                (Vector3, Vector3) points = (Vector3.zero, Vector3.zero);
                (float, float) dists = (Mathf.Infinity, Mathf.Infinity);
                //var verts = hit.transform.GetComponent<MeshFilter>().mesh.vertices;
                var verts = hit.transform.GetComponent<MeshCollider>().sharedMesh.vertices; // would have to check if mesh or primitive -> manual calcs if latter
                foreach (var v in verts)
                {
                    var vw = hit.transform.TransformPoint(v);
                    //vw.y = 0;
                    //Debug.DrawLine(vw, vw + Vector3.up, Color.green, 5.0f);

                    var vdist = Vector3.Distance(/*transform.position*/ predictedPosition, vw);
                    if (/*vdist > 1.0e-5 &&*/ dists.Item1 > vdist) // Skip if points overlap > cant get direction vector
                    {
                        points.Item1 = vw;
                        dists.Item1 = vdist;

                        // just save dir vec
                    }
                }
                /*foreach (var v in verts) //clean this up
                {
                    var vw = hit.transform.TransformPoint(v);
                    vw.y = 0;
                    var vdist = Vector3.Distance(transform.position, vw);
                    if (dists.Item2 > vdist && vw != points.Item1)
                    {
                        points.Item2 = vw;
                        dists.Item2 = vdist;
                    }
                }*/
                /*var vec = points.Item2 - points.Item1;
                Debug.DrawLine(points.Item1, points.Item1 + vec, Color.cyan, 5.0f);
                Debug.DrawLine(points.Item1, points.Item1 + Vector3.up, Color.blue, 5.0f);
                Debug.DrawLine(points.Item2, points.Item2 + Vector3.up, Color.red, 5.0f);*/

                var vec = points.Item1 - hit.point;
                Debug.DrawLine(hit.point, hit.point + vec, Color.cyan, 5.0f);
                Debug.DrawLine(points.Item1, points.Item1 + Vector3.up, Color.cyan, 5.0f);
                //***

                //var edgeDir = transform.position - closestPoint;
                var edgeDir = points.Item1 - hit.point;
                edgeDir.y = 0;
                var overlap = edgeDir.magnitude < 1.0e-5; // corner
                GetComponent<Debugger>().downForce = edgeDir.magnitude;
                edgeDir.Normalize();
                //GetComponent<Debugger>().downForce = edgeDir.magnitude;

                var start = hit.point;
                start.y = 0;
                var end = points.Item1;
                end.y = 0;
                var dropDir = predictedPosition - (start + Vector3.Project(predictedPosition - start, end - start));
                dropDir.y = 0;
                dropDir.Normalize();
                Debug.DrawLine(hit.point, hit.point + dropDir, Color.cyan, 5.0f);

                // Moving towards the drop
                var moveDir = playerVelocity;
                moveDir.y = 0;
                moveDir.Normalize();
                if (Vector3.Dot(/*edgeDir*/ dropDir, moveDir) > 0.2f) // drop dir
                {
                    color = Color.blue;
                    downForce.y = 0.0f;
                }
                else
                {
                    color = Color.red;
                    downForce.y = -9.81f;
                }
                /*if (Mathf.Abs(Vector3.Dot(edgeDir, moveDir)) > 0.8f) //ground dir
                {
                    color = Color.red;
                    downForce.y = -9.81f;
                }
                else
                {
                    color = Color.blue;
                    downForce.y = 0.0f;
                }*/

                if (overlap)
                {
                    // on corner, check next predicted position
                    predictedPosition = transform.position + fwdDir * Time.deltaTime * 2;
                    if (Physics.SphereCast(predictedPosition, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                    { 
                    }

                        // works but also means that downforce is always applied at corners
                        // would be better to calculate edge vector with another vertex when points overlap
                }

                //Debug.DrawLine(transform.position, transform.position + edgeDir, Color.green, 5.0f);
                //Debug.DrawLine(closestPoint, closestPoint + moveDir, Color.magenta, 5.0f);
            }

            // TODO;
            // bugs with other surfaces close enough?
            // might have to use predicted position
            // + first air frame playerVelocity.y = characterController.velocity.y;
            //      ie. going down slope and then dropping off -> smoother
            // todo; get edge vector -> closestpoint
            // what if points overlap
        }
#endif

        //***
        // SAME AS ABOVE
        var color = Color.blue;
        downForce.y = 0.0f;
        if (characterController.isGrounded)
        {
            Vector3 fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z) * Time.deltaTime;
            var predictedPosition = transform.position + fwdDir;

            RaycastHit hit;
            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(predictedPosition, Vector3.down, characterController.height, groundLayer))
            {
                //color = Color.red;
                //downForce.y = -9.81f;
            }
            else
            {
                // Pixelwalking

                Vector3 npos = Vector3.zero; // debug
                Vector3 ndir = Vector3.zero;

                bool ApplyDownforce(Vector3 pos, bool first)
                {
                    //Debug.DrawLine(pos, pos + Vector3.down * characterController.height, Color.cyan, 5.0f);

                    if (Physics.SphereCast(pos, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                    {
                        //Debug.DrawLine(hit.point, hit.point + Vector3.up * 2, Color.green, 5.0f);

                        Vector3 points = Vector3.zero;
                        float dists = Mathf.Infinity;
                        //var verts = hit.transform.GetComponent<MeshFilter>().mesh.vertices;
                        var verts = hit.transform.GetComponent<MeshCollider>().sharedMesh.vertices; // would have to check if mesh or primitive -> manual calcs if latter
                        foreach (var v in verts)
                        {
                            var vw = hit.transform.TransformPoint(v);
                            var vdist = Vector3.Distance(pos, vw);
                            if (dists > vdist)
                            {
                                points = vw;
                                dists = vdist;

                                // just save dir vec
                            }
                        }

                        var lineStart = hit.point;
                        //Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.red, 5.0f); //***

                        //***
                        var start = hit.point;
                        start.y = 0;
                        var end = points;
                        end.y = 0;
                        var xxPos = pos;
                        //***

                        var edgeDir = points - hit.point;
                        edgeDir.y = 0;
                        if (edgeDir.magnitude < 1.0e-5) // points overlap > corner
                        {
                            //color = Color.red;
                            //downForce.y = -9.81f;
                            /*if (!first)
                            {
                                Debug.DrawLine(hit.point, hit.point + Vector3.up * 2, Color.black, 5.0f);
                            }

                            predictedPosition.y = hit.point.y;*/

                            //return false;

                            //***   
                            //Debug.Log("first hit corner");

                            // Try to get edge direction; Next predicted position at hit height > closest collider point
                            var checkPos = pos + fwdDir;
                            /*if (Physics.Raycast(predictedPosition, Vector3.down, characterController.height, groundLayer))
                            {
                                // Next predicted position is on the ground
                                color = Color.red;
                                downForce.y = -9.81f;
                                return true;

                                // can just check distance from pos to closestpoint instead?
                            }*/

                            //***
                            /* "Basically the convex check mark will make a collider around your mesh without any 
                             * 'indented' areas. Say your mesh is a jail door made out of bars. 
                             * With a mesh collider you could still shoot between the bars. 
                             * But if you check convex, unity will make a more optimized collider 
                             * around all if the bars, but your not gonna be able to anything between them. " */

                            checkPos.y = hit.point.y;
                            var closestPoint = hit.collider.ClosestPoint(checkPos); // !!!only works on convex

                            if (Vector3.Distance(checkPos, closestPoint) < 1.0e-5)
                            {
                                // Next predicted position is on the ground
                                color = Color.red;
                                downForce.y = -9.81f;
                                return true;
                            }

                            edgeDir = points - closestPoint;
                            edgeDir.y = 0;
                            //Debug.DrawLine(closestPoint, closestPoint + Vector3.up * 2, Color.blue, 5.0f);
                            lineStart = closestPoint;
                            if (edgeDir.magnitude < 1.0e-5) // points overlap > corner
                            {
                                Debug.LogError("second hit corner");
                                return false;
                            }
                            // >> Succeeded finding out edge directon vector. Continue with calcs
                            start = closestPoint;
                            start.y = 0;
                            //end = points;
                            //end.y = 0;
                            xxPos = pos + fwdDir;

                            // This doesnt work if 2nd check is on another colliders area?
                            // edgedir is sometimes wrong at "corners"
                            // colliders closest point might not be at the edge

                            //***

                        }
                        //Debug.DrawLine(lineStart, lineStart + edgeDir, Color.red, 5.0f); //***
                        edgeDir.Normalize();

                        /*var start = hit.point;
                        start.y = 0;
                        var end = points;
                        end.y = 0;*/
                            var dropDir = xxPos - (start + Vector3.Project(xxPos - start, end - start)); // Pos - closest point on vector
                        dropDir.y = 0;
                        dropDir.Normalize();

                        Debug.DrawLine(lineStart, lineStart + dropDir, Color.cyan, 5.0f);
                        npos = hit.point; // debug
                        ndir = dropDir;

                        // Moving towards the drop
                        var moveDir = playerVelocity;
                        moveDir.y = 0;
                        moveDir.Normalize();
                        if (Vector3.Dot(dropDir, moveDir) > 0.2f)
                        {
                            color = Color.blue;
                            downForce.y = 0.0f;
                        }
                        else
                        {
                            color = Color.red;
                            downForce.y = -9.81f;
                        }
                    }
                    return true;
                }

                /*if (!ApplyDownforce(predictedPosition, true))
                {
                    // Corner > try again at next position
                    predictedPosition += fwdDir;
                    if (!ApplyDownforce(predictedPosition, false))
                    {
                        Debug.Log("corner");

                        //***
                        // Sometimes both checks are too close to vertex
                        // and no downforce is applied when it should be
                        // Raycast hits upper platform first?

                        // BUG;
                        // its possible for both spherecast to hit same spot
                        // but the bump isnt noticeable unless 2nd pos is far enough from flat -> slope point

                        // Ignore collisions with already hit col?
                        // Wont work if its part of the same object
                        // plus raycast would hit top corner anyway?
                        // > maybe filter out vertex
                        // How to get next vertex? desired vertex isnt always closest
                    }
                }*/

                // No need for 2nd raycast? do collider point check if first hits corner
                //ApplyDownforce(predictedPosition, true);

                //if (ndir.magnitude > 0.0f) // debug
                    //Debug.DrawLine(npos, npos + ndir, Color.cyan, 5.0f);
            }
        }

        //***
        // TEST; use currentpos + predictedpos to get edge dir
        // instead of either + vertexpos
        // overlap instead of spherecast?

        /*if (characterController.isGrounded)
        {
            Vector3 fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z) * Time.deltaTime;
            var predictedPosition = transform.position + fwdDir;
            // = 0; -9 if dir calc ok

            RaycastHit hit;
            var groundLayer = LayerMask.GetMask("Environment");
            if (!Physics.Raycast(predictedPosition, Vector3.down, characterController.height, groundLayer))
            {
                var startPos = Vector3.zero;
                var endPos = Vector3.zero;

                if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                {
                    startPos = hit.point;

                    if (Physics.SphereCast(predictedPosition, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                    {
                        endPos = hit.point;

                        var vec = endPos - startPos;
                        vec.y = 0;
                        var dist = vec.magnitude;
                        vec.Normalize();
                        if (dist >= fwdDir.magnitude * 0.95f)
                            Debug.DrawLine(startPos, startPos + vec, Color.cyan, 5.0f);
                    }
                }
            }
        }*/

        //***
        // iter 9000

        if (characterController.isGrounded)
        {
            Vector3 fwdDir = new Vector3(playerVelocity.x, 0, playerVelocity.z) * Time.deltaTime;
            var predictedPosition = transform.position + fwdDir;

            RaycastHit hit;
            var groundLayer = LayerMask.GetMask("Environment");
            if (Physics.Raycast(transform.position, Vector3.down, characterController.height, groundLayer))
            {
                if (Physics.Raycast(predictedPosition, Vector3.down, characterController.height, groundLayer))
                {
                    // Ground > ground
                    color = Color.red;
                    downForce.y = -9.81f;
                }
                else
                {
                    // Ground > edge or air

                    // ground > slope pixel walk > hop
                    // would have to check if movedir == dropdir
                    // thus making this iter pointless
                }
            }
            else
            {
                if (Physics.Raycast(predictedPosition, Vector3.down, characterController.height, groundLayer))
                {
                    // Edge > ground
                    color = Color.red;
                    downForce.y = -9.81f;
                }
                else
                {
                    // Edge > edge or air

                    /*var closestGround = Vector3.zero;
                    var GroundDistance = Mathf.Infinity;
                    foreach (var col in Physics.OverlapSphere(predictedPosition + Vector3.down * characterController.height / 2, characterController.radius, groundLayer) )
                    {
                        // Get closest point on ground
                        var point = col.ClosestPoint(predictedPosition);
                        var dist = Vector3.Distance(point, predictedPosition);
                        if (GroundDistance > dist)
                        {
                            closestGround = point;
                            GroundDistance = dist;
                        }
                    }
                    if (GroundDistance <= characterController.radius)
                    {
                        // Edge > edge
                        if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                        {
                            var edgeDir = closestGround - hit.point;
                            edgeDir.y = 0;
                        }
                    }
                    else
                    {
                        // No hits; Edge > air
                    }*/

                    // TEST;
                    // No need to check for drop direction?
                    // Apply downforce as long as predicted position is on the ground (edge)
                    // Edge > edge
                    //if (Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hit, characterController.height, groundLayer))
                    if (Physics.OverlapSphere(predictedPosition + Vector3.down * characterController.height / 2, characterController.radius, groundLayer).Length > 0)
                    {
                        color = Color.red;
                        downForce.y = -9.81f;

                        // Doesnt work?
                        // Sticks to edges (after first frame)

                        // just get dropdir with pos?
                        // (if possible to get right pos on ground) (w/o elevation)
                    }
                    else
                    {
                        // Edge > air
                    }
                }
            }
        }

        //***

        // Jumps can be queued
        if (characterController.isGrounded && movementInputs.wishJump && dodgeTimer >= 0.25f)
        {
            //playerVelocity.y = jumpSpeed;
            const float jumpHeight = 1.8f;
            playerVelocity.y = Mathf.Sqrt(jumpHeight * 2.0f * gravity);
            movementInputs.wishJump = false;

            downForce.y = 0.0f;
        }

        // Save previous ground state
        wasOnGround = characterController.isGrounded;

        // Move the controller
        characterController.Move((playerVelocity + downForce) * Time.deltaTime);

        // DEBUGGING
        //GetComponent<Debugger>().downForce = downForce.y;

        // Debug velocity vector
        // Vel vector and movement should match
        //Debug.DrawLine(transform.position, transform.position + playerVelocity * Time.deltaTime, Color.green, 5.0f);

        Debug.DrawLine(prevPos, transform.position, color, 5.0f);
        Debug.DrawLine(prevPos, prevPos + (Vector3.down * characterController.height / 1.85f), color, 5.0f);
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
        //***
        // slopedir = {normal.x, -normal.y, normal.z}
        //***

        // Vel dir slope angle;
        var temp0 = playerVelocity;
        temp0.Normalize();
        temp0 = Quaternion.Euler(0, -90, 0) * temp0; // left
        var slopevec = Vector3.Cross(hit.normal, temp0);
        var slopeAngle = 90 - Vector3.Angle(Vector3.up, slopevec);
        //GetComponent<Debugger>().slopeAngle = slopeAngle;

        return;

        // Works but maybe bit excessive
        // onground + raycast check -> pixelwalk?

        //Debug.DrawLine(transform.position, transform.position + hit.normal, Color.green, 2.0f); // Draw ground normal

        // Obtain the normals from the Mesh
        Mesh mesh = hit.transform.GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;
        foreach (var normal0 in normals)
        {
            //Debug.DrawLine(transform.position, transform.position + normal, Color.green, 2.0f); // Draw ALL normals

            if (hit.normal == normal0)
            {
                GetComponent<Debugger>().pixelWalking = false;
                return;
            }
        }
        GetComponent<Debugger>().pixelWalking = true;
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
        if (currentSlope >= 25f && characterController.isGrounded)
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