//#define SHOWDEBUGLOG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TODO;
 * 
 *  
 *              
 */


public class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected GameObject explosionSFX;

    // Stats
    protected float damagePerShot; // Damage dealt by direct hit. Enemies hit directly do not take splash damage
    protected float splashDamage; // Maximum amount of splash damage projectile can deal

    protected float projectileRadius; // Size of the projectile
    protected float splashRadius; // Explosion damage radius
    protected float projectileSpeed = 300.0f;

    // Simulation
    private bool move = true; // Projectile simulation
    private Vector3 movementVector;
    protected float maximumRange = 100.0f; // Maximum range
    private float totalTraveledDistance = 0.0f;

    // List of hit (root) gameobjects
    List<GameObject> hitTargets = new List<GameObject>(); // Filters out multihits. (Several colliders or direct + splash)

    //**************************************************
    public void Setup(float damage, float splashdmg)
    {
        damagePerShot = damage;
        splashDamage = splashdmg;
    }

    //**************************************************
    protected virtual void Start()
    {
        movementVector = transform.forward * projectileSpeed;
    }

    //**************************************************
    protected virtual void Update()
    {
        if (!move)
        {
            return;
        }

        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;
        bool foundHit = false;
        bool reachedMaxDistance = false;

        // Predict projectile movement
        Vector3 nextPosition = transform.position + movementVector * Time.deltaTime;
        Vector3 predictedMovement = nextPosition - transform.position;

        // Sphere cast from current to next position
        int shootableMask = LayerMask.GetMask("Enemy", "Environment"); // Layers projectile can collide with
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, projectileRadius, predictedMovement.normalized, predictedMovement.magnitude, shootableMask);
        foreach (var hit in hits)
        {
            if (hit.distance < closestHit.distance)
            {
#if SHOWDEBUGLOG
                Debug.Log("PROJECTILE HIT SOMETHING");
#endif

                foundHit = true;
                closestHit = hit;
            }
        }

        // Check for maximum range
        float traveledDistance = predictedMovement.magnitude;
        if (totalTraveledDistance + traveledDistance >= maximumRange) // Reached max projectile range
        {
            Vector3 maxdistPos = transform.position + predictedMovement.normalized * (maximumRange - totalTraveledDistance);

            // Maximum range was reached before projectile hit anything
            if (closestHit.distance > Vector3.Distance(transform.position, maxdistPos))
            {
#if SHOWDEBUGLOG
                Debug.Log("PROJECTILE REACHED MAXIMUM RANGE");
#endif

                foundHit = false; // Set to false in case hit was found outside of maximum range
                reachedMaxDistance = true;
                closestHit.point = maxdistPos;

                // Add remaining distance
                totalTraveledDistance += Vector3.Distance(transform.position, maxdistPos);
            }
        }

        // Hit something or reached maximum range
        if (foundHit || reachedMaxDistance)
        {
            // Projectile hit something, update position
            move = false;
            nextPosition = closestHit.point;

            if (foundHit)
            {
                // Deal direct hit damage
                DealDamage(closestHit.transform.root.gameObject, damagePerShot);
            }

            // Create explosion
            Explosion(closestHit);

#if SHOWDEBUGLOG
            Debug.Log(totalTraveledDistance);
#endif
        }

        // Move projectile
        transform.position = nextPosition;

        // Orient towards velocity
        transform.forward = predictedMovement.normalized;

        // Update distance traveled
        totalTraveledDistance += traveledDistance;
    }

    //**************************************************
    protected virtual void Explosion(RaycastHit hit)
    {
        // Hit location
        Vector3 location = hit.point; // Remember; hit.point = point of collision; hit.transform.position = location of hit target

        // Destroy projectile & create explosion SFX
        Destroy(this.gameObject);
        Instantiate(explosionSFX, location, transform.rotation);

        // Layers that can receive damage
        int damageLayer = LayerMask.GetMask("Player", "Enemy");

        // Check for targets inside the splash radius
        Collider[] hitColliders = Physics.OverlapSphere(location, splashRadius, damageLayer);
        foreach (var collider in hitColliders)
        {
            DealDamage(collider.transform.root.gameObject, splashDamage);
        }
    }

    //**************************************************
    private void DealDamage(GameObject target, float damage)
    {
        // Hit nothing or environment
        if (!target || target.layer == LayerMask.NameToLayer("Environment"))
        {
            return;
        }

        // Make sure target was not already hit
        if (!hitTargets.Contains(target))
        {
            // Save target (root gameobject)
            hitTargets.Add(target);

            // Damage target
            //hit.collider.gameObject.GetComponent<HealthBase>().ReceiveDamage();
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

        // Splash radius
        Color radiusColorSplash = Color.red * 0.5f;
        Gizmos.color = radiusColorSplash;
        Gizmos.DrawSphere(transform.position, splashRadius);
    }
}