using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
/*
 * Better way to select if weapon is primary or secondary
 * Currently just firePrimaryWeapon || fireSecondaryWeapon in weapons code (below)
 * 
 * 
 * 
 * 
 */

//**************************************************
[RequireComponent(typeof(LineRenderer))]
public class LightningGun : Hitscan
{
    // SFX
    private Vector3 beamStartPos; // Is used to sync beam start/end position in multiplayer
    private Vector3 beamEndPos;
    private float effectDisplayTime = 0.25f; // For how long beam is displayed
    private LineRenderer beamLine;

    //**************************************************
    protected override void Start()
    {
        base.Start();

        damagePerShot = 7.0f;
        timeBetweenShots = 0.055f;
        maximumRange = 50.0f;

        // Get line renderer component
        beamLine = GetComponent<LineRenderer>();

        timer = timeBetweenShots; // Start without cooldown
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();

        Fire();
    }

    //**************************************************
    protected override void Fire()
    {
        if (weaponInput.firePrimaryWeapon)
        {
            // Reset the timer.
            timer = 0f;

            base.Fire(); // Access Hitscan.Fire()
        }
    }
}
