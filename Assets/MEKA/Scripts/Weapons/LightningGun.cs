using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
/*
 * Addcomponent vs requirecomponent + getcomponent.
 * Case of both weapons using beamSFX?
 * 
 * Use Physics.SphereCast instead? For beam width
 */

//**************************************************
public class LightningGun : PrimaryWeapon
{
    // SFX
    private ContinuousBeamSFX sfx;

    //**************************************************
    protected override void Start()
    {
        base.Start();

        damagePerShot = 7.0f;
        timeBetweenShots = 0.055f;
        maximumRange = 50.0f;
        weaponTimer = timeBetweenShots; // Start without cooldown

        heatPerShot = 1.0f;

        // SFX setup
        sfx = gameObject.AddComponent<ContinuousBeamSFX>();
        sfx.Setup("Materials/Blue", weapon.localPosition);
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

            float beamLength; // Length of the beam SFX
            FireHitscan(out beamLength); // Fire hitscan
            base.Fire(); // Increase heat level

            sfx.UpdateBeam(beamLength);
        }
        else if (!weaponInput.fireButtonDown || overHeated)
        {
            // Disable SFX when not firing
            sfx.DisableSFX();
        }
    }
}
