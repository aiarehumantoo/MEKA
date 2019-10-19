using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO;
/*
 * Beam SFX displays burstfire wrong
 * 
 */

//**************************************************
public class burstfiretest : Beam
{
    int numberOfShots = 3;

    //**************************************************
    protected override void Start()
    {
        base.Start();

        damagePerShot = 25.0f;
        timeBetweenShots = 1.5f;
        maximumRange = 50.0f;

        useContinuousBeamSFX = false;
        effectDisplayTime = 0.1f;
        //beamMaterial = (Material)Resources.Load("BeamMaterial", typeof(Material));

        weaponTimer = timeBetweenShots; // Start without cooldown
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
        if (weaponInput.fireWeapon)
        {
            // Stop the timer
            weaponTimer = -1.0f;

            StartCoroutine(BurstFire());
        }
    }

    //**************************************************
    IEnumerator BurstFire()
    {
        for (int i = 0; i < numberOfShots; i++)
        {
            Debug.Log("shot " + i);
            if (i == 0)
            {
                base.Fire(); // Access Hitscan.Fire()
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
                base.Fire();
            }
        }

        // Reset the timer
        weaponTimer = 0.0f;
    }
}
