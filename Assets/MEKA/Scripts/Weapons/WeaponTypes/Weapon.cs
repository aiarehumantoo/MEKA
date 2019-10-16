using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
/*
 * bool for buttondown here, move fireweapon to weapon itself
 * 
 * 
 */

//**************************************************
public struct WeaponInput
{
    public bool fireButtonDown;
    public bool fireWeapon;
}

//**************************************************
public class Weapon : MonoBehaviour
{
    protected float damagePerShot; // The damage inflicted by each shot or impact damage for projectiles
    protected float timeBetweenShots; // The time between each shot
    
    protected RaycastHit shootHit; // A raycast hit to get information about what was hit.
    protected int shootableMask; // A layer mask so the raycast only hits things on the shootable layer.

    protected Camera playerCamera; // Camera location for shooting

    protected float timer; // A timer to determine when to fire.

    public WeaponInput weaponInput;

    //**************************************************
    protected virtual void Start()
    {
        // Get camera
        playerCamera = Camera.main;

        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");   
    }

    //**************************************************
    protected virtual void Update()
    {
        // Use timer = -1.0f to disable counting. ie. during burst fire
        if (timer >= 0.0f)
        {
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;
        }
    }

    //**************************************************
    protected virtual void Fire() { }
}