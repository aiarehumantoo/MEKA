using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
/*
 * bool for buttondown here, move fireweapon to weapon itself
 * 
 * What about mixed type? ie. hitscan with projectile alt fire mode
 * Change stats for alt fire mode?
 * 
 * 
 */

//**************************************************
public struct WeaponInput
{
    public bool fireButtonDown;
    public bool fireWeapon;
    public bool reload;
}

//**************************************************
public class Weapon : MonoBehaviour
{
    protected bool isHitscan;

    protected float damagePerShot; // The damage inflicted by each shot or impact damage for projectiles
    protected float timeBetweenShots; // The time between each shot
    
    protected RaycastHit shootHit; // A raycast hit to get information about what was hit.
    protected int shootableMask; // A layer mask so the raycast only hits things on the shootable layer.

    protected Camera playerCamera; // Camera location for shooting

    protected float weaponTimer; // A timer to determine when to fire.

    // Hitscan weapon
    protected Ray shootRay = new Ray(); // A ray from the gun end forwards.
    protected float maximumRange; // Maximum range
    protected Vector3 beamSFXStartPos;
    protected Vector3 beamSFXEndPos;
    protected float beamLength; // Length of the beam sfx
    [SerializeField] protected Transform weapon; // For SFX starting position

    //Projectile weapon
    [SerializeField]
    protected GameObject projectilePrefab; // Prefab of the projectile
    protected float splashDamage; // Maximum amount of splash damage projectile can deal
    protected float spawnDistance = 0.0f; // How far from the player projectile should spawn. // Spawn distance is pointless with projectiles igonring the shooter. Might be needed for better visuals?
    protected float projectileSpeed = 25.0f;
    protected float splashRadius;
    protected float projectileLifeTime = 2.0f; // For deleting projectiles that hit nothing

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
        if (weaponTimer >= 0.0f)
        {
            // Add the time since Update was last called to the timer.
            weaponTimer += Time.deltaTime;
        }
    }

    //**************************************************
    protected virtual void Fire()
    {
        if (isHitscan)
        {
            FireHitscan();
        }
        else
        {
            FireProjectile();
        }
    }

    //**************************************************
    private void FireHitscan()
    {
        beamSFXStartPos = weapon.transform.position;

        // Shoot ray forward from camera
        shootRay.origin = playerCamera.transform.position;
        shootRay.direction = playerCamera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, maximumRange, shootableMask))
        {
            // Set the second position of the line renderer to the point the raycast hit.
            beamSFXEndPos = shootHit.point;
            beamLength = shootHit.distance; // Length

            // Hits CapsuleCollider of player
            if (shootHit.collider.tag == "Player" && shootHit.collider is CapsuleCollider)
            {
                // Deal damage
            }
        }
        else // Raycast hit nothing
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            beamSFXEndPos = shootRay.origin + shootRay.direction * maximumRange;
            beamLength = maximumRange;
        }

        Debug.Log("Fired a hitscan weapon");
    }

    //**************************************************
    private void FireProjectile()
    {
        // Fire projectile. just instantiate > projectile has stats and sfx?

        // Spawn point
        Vector3 projectileSpawn = playerCamera.transform.position + playerCamera.transform.forward.normalized * spawnDistance;

        // Create the projectile from the Prefab
        GameObject projectile = null;
        projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, playerCamera.transform.rotation);

        // Add velocity to the projectile
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;

        Debug.Log("Fired a projectile weapon");
    }

    //virtual sfx?
}