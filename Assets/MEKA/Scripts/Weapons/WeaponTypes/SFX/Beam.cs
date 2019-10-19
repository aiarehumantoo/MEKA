using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
/*
 * Configure beam settings in start rather than in editor
 * Beam startpos is at guns barrel
 * 
 * SFX should use weapon firing to update instead of just mouse1
 * ie. weapon overheating
 * 
 */

[RequireComponent(typeof(LineRenderer))]
public class Beam : Weapon //Hitscan
{
    // SFX
    protected bool useContinuousBeamSFX;
    private Vector3 beamStartPos; // Is used to sync beam start/end position in multiplayer
    private Vector3 beamEndPos;
    protected float effectDisplayTime; // For how long beam is displayed
    protected LineRenderer beamLine;

    public Transform weapon; // For SFX starting position

    //[SerializeField]
    //protected Material beamMaterial;

    //**************************************************
    protected override void Start()
    {
        base.Start();

        // Get line renderer component
        beamLine = GetComponent<LineRenderer>();

        // Set line renderer
        beamLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamLine.receiveShadows = false;
        //beamLine.materials[0] = beamMaterial;
        //beamLine.material = (Material)Resources.Load("BeamMaterial", typeof(Material));
        beamLine.startWidth = 0.3f;
        beamLine.endWidth = 0.1f;

        // Disable beam SFX
        beamStartPos = transform.position;
        beamEndPos = transform.position;
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();

        if (useContinuousBeamSFX)
            ContinuousBeamSFX();
        else
            SingleBeamSFX();
    }

    //**************************************************
    private void ContinuousBeamSFX()
    {
        beamStartPos = transform.position;
        beamEndPos = transform.position;

        if (weaponInput.fireButtonDown)
        {
            beamStartPos = weapon.transform.position;

            // Shoot ray forward from camera
            shootRay.origin = playerCamera.transform.position;
            shootRay.direction = playerCamera.transform.forward;

            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if (Physics.Raycast(shootRay, out shootHit, maximumRange, shootableMask))
            {
                // Set the second position of the line renderer to the point the raycast hit
                beamEndPos = shootHit.point;
            }
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                beamEndPos = shootRay.origin + shootRay.direction * maximumRange;
            }
        }

        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamEndPos);
    }

    //**************************************************
    private void SingleBeamSFX()
    {
        if (weaponInput.fireWeapon)
        {
            beamStartPos = weapon.transform.position;

            // Shoot ray forward from camera
            shootRay.origin = playerCamera.transform.position;
            shootRay.direction = playerCamera.transform.forward;

            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if (Physics.Raycast(shootRay, out shootHit, maximumRange, shootableMask))
            {
                // Set the second position of the line renderer to the point the raycast hit
                beamEndPos = shootHit.point;
            }
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                beamEndPos = shootRay.origin + shootRay.direction * maximumRange;
            }
        }
        else
        {
            // Turn beam SFX off when duration expires
            if (weaponTimer >= effectDisplayTime)
            {
                beamStartPos = transform.position;
                beamEndPos = transform.position;
            }
        }

        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamEndPos);
    }

    //**************************************************
    protected override void Fire()
    {
        base.Fire(); // Access Hitscan.Fire()
    }
}
