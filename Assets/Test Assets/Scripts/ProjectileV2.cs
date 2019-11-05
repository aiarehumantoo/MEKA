using System.Collections.Generic;
using UnityEngine;

/* NOTES;
 * 
 * Rigidbody + collider + OnTriggerEnter() is unreliable
 * Usable with fast projectiles only if physics update rate is high enough
 * 
 * Physics.OverlapSphere() is more reliable
 * Is otherwise consistent but projectiles spawning too close to a object might pass through
 * Doesnt work at very high speeds
 * 
 * Spherecast/Raycast between old/new position along projectile path
 * Possible cases of getting hit by projectile that barely missed?
 * Is this even noticeable in this use case?
 * Hit found > Correct projectile location > Projectile always stops in the right spot
 * But projectile might go past target before correction (visual flaw)
 * Use predictive calculation instead? That way projectile gets stopped before passing the target
 * 
 */


// Testing hit detection methods for accuracy and efficiency
public class ProjectileV2 : MonoBehaviour
{
    protected RaycastHit shootHit; // A raycast hit to get information about what was hit.
    protected int shootableMask; // A layer mask so the raycast only hits things on the shootable layer.

    bool move = true;
    float radius = 0.1f;
    float speed = 300f; // test has projectiles with speed of 300.0
    Vector3 m_Velocity;
    Vector3 projectileVelocity; //ditto

    Vector3 previousPosition;
    Vector3 rootPosition;

    private void Start()
    {
        m_Velocity = -transform.up * speed;
        projectileVelocity = -transform.up * speed;

        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");

        rootPosition = transform.position;
        previousPosition = transform.position;
    }

#if false
    void Update()
    {
        if (move)
        {
            transform.position += m_Velocity * Time.deltaTime;
        }

        /*
        // Overlap method
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, shootableMask);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                Debug.Log("HIT GROUND");
                move = false;
            }
        }*/

        // Raycast method
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // Sphere cast
            Vector3 displacementSinceLastFrame = transform.position - previousPosition;
            RaycastHit[] hits = Physics.SphereCastAll(previousPosition, radius, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, shootableMask);
            foreach (var hit in hits)
            {
                if (IsHitValid(hit) && hit.distance < closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;
                }
            }

            if (foundHit)
            {
                // Handle case of casting while already inside a collider
                if (closestHit.distance <= 0f)
                {
                    //closestHit.point = root.position;
                    closestHit.point = rootPosition;
                    closestHit.normal = -transform.forward;
                }

                //OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                Debug.Log("HIT GROUND");
                move = false;
                transform.position = closestHit.point;
            }
            else
            {
                previousPosition = transform.position;
            }
        }
    }
#endif

    void Update()
    {
        if (!move)
        {
            return;
        }

        // Hit detection, raycast between current and predicted location
        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;
        bool foundHit = false;

        // Sphere cast
        Vector3 nextPosition = transform.position + projectileVelocity * Time.deltaTime; // Projectile location after update
        var predictedMovement = nextPosition - transform.position;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, predictedMovement.normalized, predictedMovement.magnitude, shootableMask);
        foreach (var hit in hits)
        {
            if (IsHitValid(hit) && hit.distance < closestHit.distance)
            {
                foundHit = true;
                closestHit = hit;
            }
        }
        if (foundHit)
        {
            // Handle case of casting while already inside a collider
            if (closestHit.distance <= 0f)
            {
                //closestHit.point = root.position;
                closestHit.point = rootPosition;
                closestHit.normal = -transform.forward;
            }

            //OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            Debug.Log("HIT GROUND");
            move = false;
            transform.position = closestHit.point;
            //transform.position = hitLocation;
        }
        else if (move)
        {
            //transform.position += projectileVelocity * Time.deltaTime;
            transform.position = nextPosition;
        }
    }

    bool IsHitValid(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            return true;
        }

        return true;
    }
}