using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyBolterProjectile : ProjectileBase
{
    protected override void Start()
    {
        base.Start();

        // Projectile stats
        projectileRadius = 0.1f;
        projectileSpeed = 125.0f;
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();
    }

    //**************************************************
    protected override void Explosion(RaycastHit hit)
    {
        // This projectile does not explode
        // Kinematic bolt that deals impact damage, lingers on a while and is then deleted

        // Hit collider
        if (hit.collider)
        {
            // Add delay if hit environment
            const float explosionDelay = 1.5f;
            StartCoroutine(DelayedExplosion(hit, explosionDelay));
            transform.SetParent(hit.transform);
            return;
        }

        // Destroy immediately
        Destroy(this.gameObject);
    }

    //**************************************************
    private IEnumerator DelayedExplosion(RaycastHit hit, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}