using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:
/*
 * Better way to select if weapon is primary or secondary
 * Currently just firePrimaryWeapon || fireSecondaryWeapon in weapons code (below)
 * 
 * Delay after firing before cooling starts?
 * Reload that dissipates all heat (like that weapon in lawbreakers)
 * Move heat/cooling to base > avoids dealing with able to fire or not
 * 
 */

//**************************************************
//[RequireComponent(typeof(LineRenderer))]
public class LightningGun : PrimaryWeapon
{
    // SFX
    private ContinuousBeamSFX sfx = new ContinuousBeamSFX();
    //[SerializeField] private Transform weapon; // For SFX starting position
    //private LineRenderer beamLine; // Use linerenderer

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
        //beamLine = GetComponent<LineRenderer>();
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
