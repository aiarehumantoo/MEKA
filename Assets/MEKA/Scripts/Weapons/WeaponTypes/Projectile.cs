using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
public class Projectile : Weapon
{
    [SerializeField]
    protected GameObject projectilePrefab; // Prefab of the projectile

    protected float splashDamage; // Maximum amount of splash damage projectile can deal
    protected float spawnDistance = 0.0f; // How far from the player projectile should spawn. // Spawn distance is pointless with projectiles igonring the shooter. Might be needed for better visuals?
    protected float projectileSpeed = 25.0f;
    protected float splashRadius;
    protected float projectileLifeTime = 2.0f; // For deleting projectiles that hit nothing

    //**************************************************
    protected override void Fire()
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
}
