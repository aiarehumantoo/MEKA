using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleBeamSFX
{
    private float sfxDisplayTime; // For how long beam is displayed
    private LineRenderer beamLine;

    private float timer; // Timer for sfx

    //**************************************************
    public void Setup(LineRenderer lineRenderer, string materialPath, float displayTime)
    {
        sfxDisplayTime = displayTime;

        // Get line renderer component
        beamLine = lineRenderer;

        // Set line renderer
        beamLine.useWorldSpace = true; // Local space for continuous sfx, world for single shot
        beamLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamLine.receiveShadows = false;
        beamLine.startWidth = 0.3f;
        beamLine.endWidth = 0.1f;
        beamLine.material = Resources.Load<Material>(materialPath);
    }

    //**************************************************
    public void UpdateSFXTimer(float time)
    {
        timer += time;
    }

    //**************************************************
    public void UpdateBeam(Vector3 startPos, Vector3 endPos)
    {
        // Reset timer & set beamSFX positions
        timer = 0.0f;
        beamLine.SetPosition(0, startPos);
        beamLine.SetPosition(1, endPos);
    }

    //**************************************************
    public void DisableSFX()
    {
        // Turn beamSFX off when duration expires
        if (timer >= sfxDisplayTime)
        {
            beamLine.SetPosition(0, Vector3.zero);
            beamLine.SetPosition(1, Vector3.zero);
        }
    }
}
