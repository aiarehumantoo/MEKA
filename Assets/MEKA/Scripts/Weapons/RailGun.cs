﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
[RequireComponent(typeof(LineRenderer))]
public class RailGun : PrimaryWeapon
{
    // SFX
    private SingleBeamSFX sfx = new SingleBeamSFX();
    private LineRenderer beamLine; // Use linerenderer

    //**************************************************
    protected override void Start()
    {
        base.Start();

        isHitscan = true;
        damagePerShot = 30.0f;
        timeBetweenShots = 1.5f;
        maximumRange = 50.0f;
        weaponTimer = timeBetweenShots; // Start without cooldown

        heatPerShot = 10.0f;

        // SFX setup
        beamLine = GetComponent<LineRenderer>();
        float sfxDisplayTime = 0.35f;
        sfx.Setup(beamLine, sfxDisplayTime);
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();

        Fire();

        sfx.UpdateSFXTimer(Time.deltaTime); // Update sfx timer
    }

    //**************************************************
    protected override void Fire()
    {
        if (weaponInput.fireWeapon && !overHeated)
        {
            // Reset the timer.
            weaponTimer = 0.0f;

            base.Fire(); // Fire weapon

            sfx.UpdateBeam(beamSFXStartPos, beamSFXEndPos);
        }
        else
        {
            // Disable SFX
            sfx.DisableSFX();
        }
    }
}