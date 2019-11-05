using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Inheriting weapon vs base code
 * weapon -> inherits lot of unnecessary things
 * projectilebase -> 
 * 
 * 
 */

public class ProjectileBase : MonoBehaviour
{
    protected float damagePerShot;
    protected float splashDamage; // Maximum amount of splash damage projectile can deal
    protected float splashRadius;
    private float projectileLifeTime = 2.0f; // For deleting projectiles that hit nothing

    private bool move = true;
    private float projectileSpeed = 300.0f;
    private Vector3 projectileVelocity;
    protected int shootableMask; // A layer mask so the raycast only hits things on the shootable layer.
    float radius = 0.1f;

    private Vector3 spawnPosition;

    Vector3 hitLocation;


    protected virtual void Start()
    {
        projectileVelocity = transform.forward * projectileSpeed;

        // Create a layer mask for the Shootable layer.
        shootableMask = LayerMask.GetMask("Enemy", "Environment");

        spawnPosition = transform.position;
    }

    protected virtual void Update()
    {
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

                hitLocation = hit.point;
            }
        }
        if (foundHit)
        {
            // Handle case of casting while already inside a collider
            if (closestHit.distance <= 0f)
            {
                //closestHit.point = root.position;
                closestHit.point = spawnPosition;
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

    private bool IsHitValid(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
        //if (hit.collider.tag == "TargetWall")
        {
            return true;
        }

        return true;
    }
}





/*
        // Move
        transform.position += m_Velocity * Time.deltaTime;
        if (inheritWeaponVelocity)
        {
            transform.position += m_ProjectileBase.inheritedMuzzleVelocity * Time.deltaTime;
        }

        // !!! fix for projectile path / visual sfx !!!

        // Drift towards trajectory override (this is so that projectiles can be centered 
        // with the camera center even though the actual weapon is offset)
        if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude < m_TrajectoryCorrectionVector.sqrMagnitude)
        {
            Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
            float distanceThisFrame = (root.position - m_LastRootPosition).magnitude;
            Vector3 correctionThisFrame = (distanceThisFrame / trajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
            correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
            m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

            // Detect end of correction
            if(m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                m_HasTrajectoryOverride = false;
            }

            transform.position += correctionThisFrame;
        }

        ///

        // Orient towards velocity
        transform.forward = m_Velocity.normalized;

        // Gravity
        if (gravityDownAcceleration > 0)
        {
            // add gravity to the projectile velocity for ballistic effect
            m_Velocity += Vector3.down * gravityDownAcceleration * Time.deltaTime;
        }

        // Hit detection
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // Sphere cast
            Vector3 displacementSinceLastFrame = tip.position - m_LastRootPosition;
            RaycastHit[] hits = Physics.SphereCastAll(m_LastRootPosition, radius, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, hittableLayers, k_TriggerInteraction);
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
                if(closestHit.distance <= 0f)
                {
                    closestHit.point = root.position;
                    closestHit.normal = -transform.forward;
                }

                OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            }
        }

        m_LastRootPosition = root.position;
        */
