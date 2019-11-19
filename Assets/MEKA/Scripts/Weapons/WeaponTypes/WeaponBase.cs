//#define SHOWDEBUGLOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
/*
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
    protected float maximumRange; // Maximum range                                // Todo; Add as optional for projectiles. ie. MIRV pellets explode at max range

    [SerializeField] protected Transform weapon; // Weapon location
    public WeaponInput weaponInput; // Player inputs
    protected Camera playerCamera; // Camera location for shooting
    protected float weaponTimer; // A timer to determine when to fire


    //**************************************************
    protected virtual void Start()
    {
        // Get camera
        playerCamera = Camera.main;  
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
        projectile.GetComponent<ProjectileBase>().Setup(damagePerShot, splashDamage); // Setup projectile stats
        StartCoroutine(DeleteObject(projectile, 5.0f));

        //Debug.Log("Fired a projectile weapon");
    }

    //**************************************************
    private IEnumerator DeleteObject(GameObject obj, float lifetime)
    {
        // For deleting expired projectiles
        yield return new WaitForSeconds(lifetime);
        Destroy(obj);

//#if SHOWDEBUGLOG // TODO; Commented coz this is called regardless if projectile still exists. -> Use max range instead?
        Debug.Log("PROJECTILE EXPIRED");
//#endif
    }
}