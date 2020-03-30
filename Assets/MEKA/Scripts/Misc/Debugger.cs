﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    private CharacterController _controller;

    public GUIStyle style;
    string touching = "Null";

    void Start()
    {
        _controller = GetComponent<CharacterController>();

        style.normal.textColor = Color.green; // Debug text color
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 120, 400, 100), "Touching: " + touching, style);
        GUI.Label(new Rect(10, 140, 400, 100), "OnGround: " + _controller.isGrounded, style);

        var ups = _controller.velocity;
        //ups.y = 0;
        GUI.Label(new Rect(10, 160, 400, 100), "Player velocity: " + Mathf.Round(ups.magnitude * 100) / 100 + "ups", style);
        
        var upsV = _controller.velocity;
        //ups2.x = 0;
        //ups2.z = 0;
        //GUI.Label(new Rect(0, 120, 400, 100), "Vertical Speed: " + Mathf.Round(ups2.magnitude * 100) / 100 + "ups", style);
        GUI.Label(new Rect(10, 180, 400, 100), "Vertical velocity: " + upsV.y + "ups", style);

        var upsH = _controller.velocity;
        upsH.y = 0;
        GUI.Label(new Rect(10, 200, 400, 100), "Horizontal velocity: " + /*Mathf.Round(upsH.magnitude * 100) / 100*/ Mathf.Round(upsH.magnitude) + "ups", style);
    }

    // Get tag of the object charactercontroller is touching
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        touching = hit.collider.tag;
    }
}