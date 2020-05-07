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
public class RocketLauncher : SecondaryWeapon, IConsole
{
    [SerializeField] private GameObject projectilePrefab; // Prefab of the projectile
    private float splashDamage = 50.0f; // Maximum amount of splash damage projectile can deal

    //**************************************************
    protected override void Start()
    {
        base.Start();

        // Weapon stats
        damagePerShot = 90.0f;
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
        if (weaponInput.fireWeapon && weaponState == WeaponState.Normal)
        {
            // Reset the timer.
            weaponTimer = 0f;

            FireProjectile(projectilePrefab, splashDamage); // Fire projectile
            base.Fire();
        }
    }

    //**************************************************
    public void ConsoleCommand()
    {
        //Console test
        Debug.Log("received console command");
    }
}
