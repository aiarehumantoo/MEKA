using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyProjectile : ProjectileBase
{
    protected override void Start()
    {
        base.Start();

        // Projectile stats
        projectileRadius = 0.15f;
        splashRadius = 2.0f;
        projectileSpeed = 100.0f;
    }

    //**************************************************
    protected override void Explosion(RaycastHit hit)
    {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Parent to enemy
            transform.parent = hit.transform;

            // Add delay if hit environment
            const float explosionDelay = 10.5f;
            StartCoroutine(DelayedExplosion(hit, explosionDelay));
        }
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // Add delay if hit environment
            const float explosionDelay = 1.5f;
            StartCoroutine(DelayedExplosion(hit, explosionDelay));
        }
        else
        {
            // Explode immediately
            base.Explosion(hit);
        }
    }

    //**************************************************
    private IEnumerator DelayedExplosion(RaycastHit hit, float time)
    {
        yield return new WaitForSeconds(time);
        base.Explosion(hit);
    }
}