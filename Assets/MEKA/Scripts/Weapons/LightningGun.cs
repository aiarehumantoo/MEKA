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
//[RequireComponent(typeof(Beam))] // TODO; make sfx into standalone code and include as component? -> split into types of sfx ie. ContinuousBeamSFX
public class LightningGun : PrimaryWeapon //Beam
{
    //**************************************************
    protected override void Start()
    {
        base.Start();

        isHitscan = true;
        damagePerShot = 7.0f;
        timeBetweenShots = 0.055f;
        maximumRange = 50.0f;

        //useContinuousBeamSFX = true;
        //beamMaterial = (Material)Resources.Load("BeamMaterial", typeof(Material));

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
        if (weaponInput.fireWeapon && !overHeated)
        {
            // Reset the timer.
            timer = 0f;

            base.Fire(); // Access Weapon.Fire()
            // Skips Primary/SecondaryWeapon.Fire() since they do not have implementation
        }
    }

}
