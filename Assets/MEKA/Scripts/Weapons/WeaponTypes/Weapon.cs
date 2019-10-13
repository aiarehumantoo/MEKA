using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
public struct WeaponInput
{
    public bool primaryFireButtonDown;
    public bool secondaryFireButtonDown;

    public bool firePrimaryWeapon;
    public bool fireSecondaryWeapon;
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
    private void GetWeaponInputs()
    {
        weaponInput.primaryFireButtonDown = Input.GetButton("Fire1");
        weaponInput.secondaryFireButtonDown = Input.GetButton("Fire2");

        weaponInput.firePrimaryWeapon = Input.GetButton("Fire1") && timer >= timeBetweenShots;
        weaponInput.fireSecondaryWeapon = Input.GetButton("Fire2") && timer >= timeBetweenShots;
    }

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
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        GetWeaponInputs();
    }

    //**************************************************
    protected virtual void Fire() { }
}