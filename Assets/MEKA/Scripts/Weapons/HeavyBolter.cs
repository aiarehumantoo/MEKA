using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
public class HeavyBolter : PrimaryWeapon
{
    [SerializeField] private GameObject projectilePrefab; // Prefab of the projectile

    //**************************************************
    protected override void Start()
    {
        base.Start();

        damagePerShot = 15.0f;
        timeBetweenShots = 0.15f;

        weaponTimer = timeBetweenShots; // Start without cooldown

        heatPerShot = 1.25f;
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
        if (weaponInput.fireWeapon && weaponState == WeaponState.Normal)
        {
            // Reset the timer.
            weaponTimer = 0.0f;

            FireProjectile(projectilePrefab, 0.0f); // Fire projectile
            base.Fire(); // Increase heat level
        }
    }
}