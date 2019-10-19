using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBeamSFX
{
    private Vector3 beamStartPos;
    private Vector3 beamEndPos;

    private LineRenderer beamLine;

    //**************************************************
    public void Setup(LineRenderer lineRenderer, Vector3 weaponPos)
    {
        // Get line renderer component
        beamLine = lineRenderer;

        // Configure beam and set length to zero
        //beamStartPos = new Vector3(weaponPos.x, weaponPos.y, weaponPos.z); // Get weapon local position // Get distance to camera rather than local position?
        beamStartPos = new Vector3(weaponPos.x, 0.0f, 0.0f);
        beamEndPos = beamStartPos;

        // Set line renderer
        beamLine.useWorldSpace = false; // Local space for continuous sfx, world for single shot
        beamLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamLine.receiveShadows = false;
        //beamLine.materials[0] = beamMaterial;
        //beamLine.material = (Material)Resources.Load("BeamMaterial", typeof(Material));
        beamLine.startWidth = 0.3f;
        beamLine.endWidth = 0.1f;
    }

    //**************************************************
    public void UpdateBeam(float beamLength)
    {
        beamEndPos = new Vector3(0.0f, 0.0f, beamLength);

        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamEndPos);
    }

    //**************************************************
    public void DisableSFX()
    {
        // Disable beam SFX
        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamStartPos);
    }
}
