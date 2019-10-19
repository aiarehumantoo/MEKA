using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
public class RocketLauncher : Projectile
{
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
            // Reset the timer.
            weaponTimer = 0f;

            base.Fire(); // Access Projectile.Fire()
        }
    }
}
