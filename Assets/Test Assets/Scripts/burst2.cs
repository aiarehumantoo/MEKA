﻿using System.Collections;
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

        damagePerShot = 90.0f;
        timeBetweenShots = 1.5f;
        //projectilePrefab = ;
        projectileSpeed = 25.0f;
        splashDamage = 50.0f;
        splashRadius = 2.0f;

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
        if (weaponInput.fireWeapon && !outOfAmmo)
        {
            // Stop the timer
            timer = -1.0f;

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
                base.Fire();
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
                base.Fire();
            }
        }

        // Reset the timer
        timer = 0.0f;
    }  
}
