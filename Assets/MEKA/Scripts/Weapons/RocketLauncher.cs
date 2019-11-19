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
    [SerializeField] private GameObject projectilePrefab; // Prefab of the projectile
    private float splashDamage = 50.0f; // Maximum amount of splash damage projectile can deal

    //**************************************************
    protected override void Start()
    {
        base.Start();

        // Weapon stats
        damagePerShot = 90.0f;
        //splashDamage = 50.0f;
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
        if (weaponInput.fireWeapon && !outOfAmmo)
        {
            // Reset the timer.
            weaponTimer = 0f;

            //base.Fire(); // Access WeaponBase.Fire()

            FireProjectile(projectilePrefab, splashDamage); // Fire projectile
        }
    }
}
