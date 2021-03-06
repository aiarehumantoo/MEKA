﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO;
 * Set stats & do setup. Currently just slightly edited RL projectile
 * 
 * 
 */

public class EocProjectile : ProjectileBase
{
    protected override void Start()
    {
        base.Start();

        // Projectile stats
        projectileRadius = 0.1f;
        splashRadius = 1.5f;
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
        // Hit collider
        if (hit.collider)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                // Add delay if hit environment
                const float explosionDelay = 0.75f;
                StartCoroutine(DelayedExplosion(hit, explosionDelay));
                return;
            }
        }

        // Explode immediately
        base.Explosion(hit);
    }

    //**************************************************
    private IEnumerator DelayedExplosion(RaycastHit hit, float time)
    {
        yield return new WaitForSeconds(time);
        base.Explosion(hit);
    }
}