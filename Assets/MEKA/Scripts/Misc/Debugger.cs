using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    // Disable Field Unused warning
#pragma warning disable 0414

    private CharacterController _controller;

    public GUIStyle style;
    string touching = "Null";

    // Debug variables
    private float debugTurn = 0;
    private float debugLegsAngle = 0;

    //***
    public float slopeAngle = 0;
    public float downForce = 0;

    public void UpdateDebugTurnRate(float val)
    {
        debugTurn = val;
    }

    public void UpdateDebugLegsAngle(float val)
    {
        debugLegsAngle = val;
    }

    void Start()
    {
        _controller = GetComponent<CharacterController>();

        style.normal.textColor = Color.green; // Debug text color
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    void Awake()
    {
        // Set up graph properties
        int group = 0;
        DebugGUI.SetGraphProperties("smoothFrameRate", "SmoothFPS", 0, 200, group, new Color(0, 1, 1), false);
        DebugGUI.SetGraphProperties("frameRate", "FPS", 0, 200, group++, new Color(1, 0.5f, 1), false);

        DebugGUI.SetGraphProperties("PlayerVelocity", "Horizontal Velocity", 0, 50, group, Color.green, false);
        DebugGUI.SetGraphProperties("PlayerVelocityY", "Vertical Velocity", -50, 50, group++, Color.yellow, false);

        DebugGUI.SetGraphProperties("PlayerTurnRate", "TurnRate", 0, 500, group, new Color(1.0f, 0.64f, 0.0f), false); // 200 is capped turn rate
        DebugGUI.SetGraphProperties("PlayerLegsAngle", "Torso/legs angle", 0, 45, group++, Color.green, false);

        DebugGUI.SetGraphProperties("Slope angle", "Slope angle", 0, 45, group++, Color.green, false);
    }

    private void Update()
    {
        // Manual persistent logging
        DebugGUI.LogPersistent("smoothFrameRate", "SmoothFPS: " + (1 / Time.deltaTime).ToString("F3"));
        DebugGUI.LogPersistent("frameRate", "FPS: " + (1 / Time.smoothDeltaTime).ToString("F3"));

        // FPS Graph
        if (Time.smoothDeltaTime != 0)
            DebugGUI.Graph("smoothFrameRate", 1 / Time.smoothDeltaTime);
        if (Time.deltaTime != 0)
            DebugGUI.Graph("frameRate", 1 / Time.deltaTime);

        // Player stats
        var vel = _controller.velocity;
        var velY = vel.y;
        vel.y = 0;
        DebugGUI.Graph("PlayerVelocity", vel.magnitude);
        DebugGUI.Graph("PlayerVelocityY", velY);

        DebugGUI.Graph("PlayerTurnRate", debugTurn);
        DebugGUI.Graph("PlayerLegsAngle", debugLegsAngle);

        DebugGUI.Graph("Slope angle", Mathf.Abs(slopeAngle));
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 120, 400, 100), "Touching: " + touching, style);
        GUI.Label(new Rect(10, 140, 400, 100), "OnGround: " + _controller.isGrounded, style);

        var ups = _controller.velocity; // Movement vector
        GUI.Label(new Rect(10, 160, 400, 100), "Player velocity: " + ups.magnitude +"\t ~" +Mathf.Round(ups.magnitude * 10) / 10 + "ups", style);

        var upsH = ups;
        upsH.y = 0;
        GUI.Label(new Rect(10, 180, 400, 100), "Horizontal velocity: " + upsH.magnitude + "\t ~" + Mathf.Round(upsH.magnitude * 10) / 10 + "ups", style);

        GUI.Label(new Rect(10, 200, 400, 100), "Vertical velocity: " +"~" +Mathf.Round(ups.y * 100) / 100 + "ups", style);

        GUI.Label(new Rect(10, 300, 400, 100), "Slope angle (vel dir): " + Mathf.Round(slopeAngle * 100) / 100 + " degrees", style);
        GUI.Label(new Rect(10, 320, 400, 100), "Downforce: " + Mathf.Round(downForce * 100) / 100 + " ups", style);
    }

    // Get tag of the object charactercontroller is touching
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        touching = hit.collider.tag;
    }

    void OnDestroy()
    {
        // Clean up our logs and graphs when this object is destroyed
        DebugGUI.RemoveGraph("smoothFrameRate");
        DebugGUI.RemoveGraph("frameRate");

        DebugGUI.RemovePersistent("smoothFrameRate");
        DebugGUI.RemovePersistent("frameRate");
    }
}