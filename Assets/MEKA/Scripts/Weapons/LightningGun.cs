using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
/*
 * Addcomponent vs requirecomponent + getcomponent.
 * Case of both weapons using beamSFX?
 */

//**************************************************
public class LightningGun : PrimaryWeapon
{
    // SFX
    private ContinuousBeamSFX sfx = new ContinuousBeamSFX();

    //**************************************************
    protected override void Start()
    {
        base.Start();

        isHitscan = true;
        damagePerShot = 7.0f;
        timeBetweenShots = 0.055f;
        maximumRange = 50.0f;
        weaponTimer = timeBetweenShots; // Start without cooldown

        heatPerShot = 1.0f;

        // SFX setup
        LineRenderer beamLine = this.gameObject.AddComponent<LineRenderer>();
        sfx.Setup(beamLine, "Materials/Blue", weapon.localPosition);
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
        if (weaponInput.fireWeapon && !overHeated)
        {
            // Reset the timer.
            weaponTimer = 0.0f;

            base.Fire(); // Fire weapon

            sfx.UpdateBeam(beamLength);
        }
        else if (!weaponInput.fireButtonDown || overHeated)
        {
            // Disable SFX when not firing
            sfx.DisableSFX();
        }
    }
}
