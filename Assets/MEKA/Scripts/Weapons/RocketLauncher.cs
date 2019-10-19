using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE;
/*
 * [SerializeField] private is inherited by child classes
 * 
 * 
 * 
 */

//**************************************************
public class RocketLauncher : SecondaryWeapon
{

    //**************************************************
    protected override void Start()
    {
        base.Start();

        isHitscan = false;
        //damagePerShot = 90.0f;
        timeBetweenShots = 1.5f;
        //projectilePrefab = ; // Set in editor?
        projectileSpeed = 25.0f;
        //splashDamage = 50.0f;
        //splashRadius = 2.0f;

        //spawnDistance =;

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
        if (weaponInput.fireWeapon && !outOfAmmo)
        {
            // Reset the timer.
            weaponTimer = 0f;

            base.Fire(); // Access Projectile.Fire()
        }
    }
}
