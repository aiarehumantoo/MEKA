﻿//#define SHOWDEBUGLOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
/*
 * Assigning hitsounds
 *      load from resources? get rid of assigning in editor for each weapon (unless different types of hitsounds are needed)
 * 
 * 
 * What about mixed type? ie. hitscan with projectile alt fire mode (charged shot?)
 * Change stats for alt fire mode?
 *                  *normal shot is projectile, charged shot is hitscan (could just raise speed a lot?)
 * 
 * 
 * 
 * 
 * Raycast for projectile weapons too incase of player is too close to a wall
 * (not needed if using camera instead of barrel location)
 * For projectiles;
 * Raycast camera to spawnpoint > hits > splash
 *                              > no hit > spawn projectile
 *                                                           
 * Hit detection: Raycast forward from camera
 * But also from gun to hit location?
 *      For correct bullet/beam sfx
 *      Wouldnt work for projectiles
 *      
 *      
 *  // fire raycast regardless of weapon type and use it as spawn check for projectiles?
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
public class WeaponBase : MonoBehaviour
{
    // Weapon stats
    protected float damagePerShot; // The damage inflicted by each shot
    protected float timeBetweenShots; // The time between each shot
    protected float maximumRange; // Maximum range // Note: Projectiles use their own max range since single weapon may use 2 different projectiles

    [SerializeField] protected Transform weapon; // Weapon location
    public WeaponInput weaponInput; // Player inputs
    protected Camera playerCamera; // Camera location for shooting
    protected float weaponTimer; // A timer to determine when to fire

    private AudioSource audioSource;
    public AudioClip[] hitSounds; // 0 = hitsound, 1 = killsound

    //**************************************************
    protected virtual void Start()
    {
        // Get camera
        playerCamera = Camera.main;

        // Get audio source
        audioSource = transform.root.GetComponent<AudioSource>();
        //TODO: create if not found
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
    }

    //**************************************************
    protected void FireHitscan(out float beamLength)
    {
#if SHOWDEBUGLOG
        Debug.Log("Fired a hitscan weapon");
#endif

        RaycastHit shootHit; // A raycast hit to get information about what was hit
        var shootableMask = LayerMask.GetMask("Environment", "Enemy"); // A layer mask so the raycast only hits things on the shootable layer
        Ray shootRay = new Ray(); // A ray from the gun end forwards

        // Shoot ray forward from camera
        shootRay.origin = playerCamera.transform.position;
        shootRay.direction = playerCamera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, maximumRange, shootableMask))
        {
            // Set the second position of the line renderer to the point the raycast hit.
            beamLength = shootHit.distance; // Length

            // Hits CapsuleCollider of enemy
            if (shootHit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && shootHit.collider is CapsuleCollider)
            {
#if SHOWDEBUGLOG
                Debug.Log("HITSCAN HIT ENEMY. " + damagePerShot + " damage");
#endif
                // Deal damage
                DealDamage(shootHit.transform.root.gameObject, damagePerShot);
            }
        }
        else // Raycast hit nothing
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            beamLength = maximumRange;
        }
    }

    //**************************************************
    protected void FireHitscan(out Vector3 beamSFXStartPos, out Vector3 beamSFXEndPos)
    {
#if SHOWDEBUGLOG
        Debug.Log("Fired a hitscan weapon");
#endif

        RaycastHit shootHit; // A raycast hit to get information about what was hit
        var shootableMask = LayerMask.GetMask("Environment", "Enemy"); // A layer mask so the raycast only hits things on the shootable layer
        Ray shootRay = new Ray(); // A ray from the gun end forwards

        beamSFXStartPos = weapon.transform.position;

        // Shoot ray forward from camera
        shootRay.origin = playerCamera.transform.position;
        shootRay.direction = playerCamera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, maximumRange, shootableMask))
        {
            // Set the second position of the line renderer to the point the raycast hit.
            beamSFXEndPos = shootHit.point;

            // Hits CapsuleCollider of enemy
            if (shootHit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && shootHit.collider is CapsuleCollider)
            {
#if SHOWDEBUGLOG
                Debug.Log("HITSCAN HIT ENEMY. " + damagePerShot + " damage");
#endif
                // Deal damage
                DealDamage(shootHit.transform.root.gameObject, damagePerShot);
            }
        }
        else // Raycast hit nothing
        {
            // ... set the second position of the line renderer to the fullest extent of the gun's range.
            beamSFXEndPos = shootRay.origin + shootRay.direction * maximumRange;
        }
    }

    //**************************************************
    protected void FireProjectile(GameObject projectilePrefab, float splashDamage)
    {
        // Spawn point
        Vector3 projectileSpawn = playerCamera.transform.position;
        //Vector3 projectileSpawn = weapon.transform.position; // TODO; Use weapon location after pathing is done

        //Create the projectile from the Prefab
        GameObject projectile = null;
        projectile = (GameObject)Instantiate(projectilePrefab, projectileSpawn, playerCamera.transform.rotation);
        projectile.GetComponent<ProjectileBase>().Setup(damagePerShot, splashDamage, this); // Setup projectile stats
    }

    //**************************************************
    private void DealDamage(GameObject target, float damage)
    {
        HealthBase targetHealth = target.GetComponent<HealthBase>();
        if (targetHealth)
        {
            if (!targetHealth.IsDead())
            {
                // Damage target & play hitsounds
                PlayHitSounds(targetHealth.ReceiveDamage(damage));
            }
        }

    }

    //**************************************************
    public void PlayHitSounds(bool finalHit)
    {
        if (finalHit)
        {
            audioSource.clip = hitSounds[1];
            audioSource.PlayOneShot(audioSource.clip);
        }
        else
        {
            audioSource.clip = hitSounds[0];
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}