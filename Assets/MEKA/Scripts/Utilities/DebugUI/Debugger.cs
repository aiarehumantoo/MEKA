﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utilities.DeveloperConsole.Commands;

namespace Utilities.DebugUI
{
    public class Debugger : MonoBehaviour
    {
        // Disable Field Unused warning
#pragma warning disable 0414

        //***
        // Ensure an instance is present
        private static Debugger _instance;
        private static Debugger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Debugger>();

                    if (_instance == null && Application.isPlaying)
                    {
                        _instance = new GameObject("Debugger").AddComponent<Debugger>();
                    }
                }
                return _instance;
            }
        }

        //***
        private bool hide = true;
        private GUIStyle style;
        private CharacterController controller;
        private float turnrateAngleThreshold;

        public static void Configure(CharacterController controller, float turnrateAngleThreshold)
        {

            if (Instance.style != null)
            {
                return; // No need to configure twice
            }

            // Set text style
            Instance.style = new GUIStyle();
            Instance.style.normal.textColor = Color.green; // Debug text color
            Instance.style.fontStyle = FontStyle.BoldAndItalic;

            Instance.controller = controller;
            Instance.turnrateAngleThreshold = turnrateAngleThreshold;
        }

        public static void ToggleDebugger()
        {
            Instance.hide = !Instance.hide;
            DebugGUI.ToggleDebuiUI();
        }

        //***
        // Debug variables
        private string touching = "Null";
        public static string Touching
        {
            //private get { return Instance.touching; }
            set { Instance.touching = value; }
        }

        private float debugTurn = 0;
        public static float DebugTurnRate
        {
            set { Instance.debugTurn = value; }
        }

        private float debugLegsAngle = 0;
        public static float DebugLegsAngle
        {
            set { Instance.debugLegsAngle = value; }
        }

        private float slopeAngle = 0;
        public static float SlopeAngle
        {
            set { Instance.slopeAngle = value; }
        }

        private float downForce = 0;
        public static float DownForce
        {
            set { Instance.downForce = value; }
        }

        private bool pixelWalking = false;
        public static bool PixelWalking
        {
            set { Instance.pixelWalking = value; }
        }

        //***
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

            // Hide at start
            DebugGUI.ToggleDebuiUI();
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
            var vel = controller.velocity;
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
            if (hide || style == null)
            {
                return;
            }

            // Create background
            GUI.Box(new Rect(5, 110, 275, 300), "");
            GUI.backgroundColor = new Color(0, 0, 0, 0);

            GUI.Label(new Rect(10, 120, 400, 100), "Touching: " + touching, style);
            GUI.Label(new Rect(10, 140, 400, 100), "OnGround: " + controller.isGrounded, style);

            var ups = controller.velocity; // Movement vector
            GUI.Label(new Rect(10, 160, 400, 100), "Player velocity: " + ups.magnitude + "\t ~" + Mathf.Round(ups.magnitude * 10) / 10 + "ups", style);

            var upsH = ups;
            upsH.y = 0;
            GUI.Label(new Rect(10, 180, 400, 100), "Horizontal velocity: " + upsH.magnitude + "\t ~" + Mathf.Round(upsH.magnitude * 10) / 10 + "ups", style);

            GUI.Label(new Rect(10, 200, 400, 100), "Vertical velocity: " + "~" + Mathf.Round(ups.y * 100) / 100 + "ups", style);

            GUI.Label(new Rect(10, 300, 400, 100), "Slope angle (vel dir): " + Mathf.Round(slopeAngle * 100) / 100 + " degrees", style);
            GUI.Label(new Rect(10, 320, 400, 100), "Downforce: " + Mathf.Round(downForce * 100) / 100 + " ups", style);
            GUI.Label(new Rect(10, 340, 400, 100), "PixelWalking: " + pixelWalking, style);

            //***
            GUI.Label(new Rect(10, 230, 400, 100), "Turn speed: " + debugTurn, style);

            GUI.Label(new Rect(10, 250, 400, 100), "Torso/legs angle: " + debugLegsAngle, style);

            if (Mathf.Abs(debugLegsAngle) >= turnrateAngleThreshold)
                GUI.Label(new Rect(10, 270, 400, 100), "Turn speed restricted", style);
        }
    }
}