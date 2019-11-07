using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TODO;
 * 
 * Save gameobject instead of colliders? For cases when enemy has multiple colliders
 * So that damage is applied just once, regardless of how many colliders enemy has
 *  saving all colliders works too
 *  
 *  max distance option; *explodes*
 *  Delayed explosion. ie. 1 second after contact with ground (but direct is instant?)
 * 
 */

public class ProjectileBase : MonoBehaviour
{
    // Stats
    protected float damagePerShot; // Damage dealt by direct hit. Enemies hit directly do not take splash damage
    protected float splashDamage; // Maximum amount of splash damage projectile can deal

    protected float projectileRadius; // Size of the projectile
    protected float splashRadius; // Explosion damage radius
    protected float projectileSpeed = 300.0f;

    // Simulation
    private bool move = true; // Projectile simulation
    protected int shootableMask; // A layer mask so the raycast only hits things on the shootable layer.

    private Vector3 spawnPosition;
    private Vector3 movementVector;

    private const float projectileLifeTime = 10.0f; // For deleting projectiles that hit nothing
    private float lifetime = 0.0f;

    List<Collider> ignoreColliders = new List<Collider>(); // Save damaged colliders so that same damage is not dealt twice

    //**************************************************
    public virtual void Setup(float damage, float splashdmg)
    {
        damagePerShot = damage;
        splashDamage = splashdmg;
    }

    //**************************************************
    protected virtual void Start()
    {
        movementVector = transform.forward * projectileSpeed;

        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");

        spawnPosition = transform.position;
    }

    //**************************************************
    protected virtual void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= projectileLifeTime)
        {
            Destroy(this.gameObject); // Delete projectile
        }

        if (!move)
        {
            return;
        }

        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;
        bool foundHit = false;
        Vector3 hitLocation = new Vector3(0, 0, 0);

        // Predict projectile movement
        Vector3 nextPosition = transform.position + movementVector * Time.deltaTime;
        var predictedMovement = nextPosition - transform.position;

        // Sphere cast from current to next position
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, projectileRadius, predictedMovement.normalized, predictedMovement.magnitude, shootableMask);
        foreach (var hit in hits)
        {
            if (IsHitValid(hit) && hit.distance < closestHit.distance)
            {
                foundHit = true;
                closestHit = hit;
                hitLocation = hit.point;
            }
        }
        if (foundHit)
        {
            // Projectile hit something, update position
            move = false;
            nextPosition = hitLocation;

            // Create explosion
            Explosion(nextPosition);
        }

        // Move projectile
        transform.position = nextPosition;

        // Orient towards velocity
        transform.forward = predictedMovement.normalized;
    }

    //**************************************************
    private bool IsHitValid(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Debug.Log("PROJECTILE HIT ENVIRONMENT");
            return true;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (!ignoreColliders.Contains(hit.collider))
            {
                Debug.Log("PROJECTILE HIT ENEMY. " +damagePerShot +" damage");

                // Save collider(s) projectile hit directly
                Collider[] hitCollider = hit.collider.gameObject.GetComponentsInChildren<Collider>();
                ignoreColliders.AddRange(hitCollider);

                return true;
            }
        }

        //if hits enemy
        // Deal direct damage, exclude target from taking splash damage

        return false;
    }

    //**************************************************
    private void Explosion(Vector3 location)
    {
        // Layers that can receive damage
        var damageLayer = LayerMask.GetMask("Player", "Enemy");

        // Check for enemies inside the splash radius
        Collider[] hitColliders = Physics.OverlapSphere(location, splashRadius, damageLayer);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("SPLASH DID SELFDAMAGE");
            }
            if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (!ignoreColliders.Contains(hitColliders[i]))
                {
                    Debug.Log("SPLASH HIT ENEMY. " + splashDamage +" damage");

                    // Save collider(s) inside splash radius    // No need to save colliders, this is done just once. needed for !multihits?
                    Collider[] hitCollider = hitColliders[i].gameObject.GetComponentsInChildren<Collider>();                             //does this get every collider or just childrens
                    ignoreColliders.AddRange(hitCollider);

                    // Save gameobject, in case of hitting multiple colliders of same enemy. Saving all targets colliders works too?
                    // Physics.OverlapSphere gets all hit colliders
                    // but after first time dealing dmg, enemys every collider is in the list and rest are skipped
                    // gameobject check might be more efficient even if functionally identical
                }
            }
        }
    }

    //**************************************************
    private void OnDrawGizmosSelected()
    {
        // DEBUG; Pause & select projectile to display radiuses

        // Projectile
        Color radiusColor = Color.cyan * 0.5f;
        Gizmos.color = radiusColor;
        Gizmos.DrawSphere(transform.position, projectileRadius);

        // Splash
        Color radiusColorSplash = Color.red * 0.5f;
        Gizmos.color = radiusColorSplash;
        Gizmos.DrawSphere(transform.position, splashRadius);
    }
}