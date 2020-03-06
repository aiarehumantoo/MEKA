using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO;
/*
 * Beam SFX displays burstfire wrong
 * 
 */

//**************************************************
public class burst2 : SecondaryWeapon
{
    int numberOfShots = 3;

    //**************************************************
    protected override void Start()
    {
        base.Start();

        //damagePerShot = 90.0f;
        timeBetweenShots = 1.5f;

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
        if (weaponInput.fireWeapon && /*!outOfAmmo*/ weaponState == WeaponState.Normal)
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
            if (i == 0)
            {
                base.Fire(); // Access Weapon.Fire() 
                // Skips Primary/SecondaryWeapon.Fire() since they do not have implementation
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
                base.Fire(); // Fire projectile weapon
            }
        }

        // Reset the timer
        weaponTimer = 0.0f;
    }  
}
