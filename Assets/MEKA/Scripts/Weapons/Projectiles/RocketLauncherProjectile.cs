using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncherProjectile : ProjectileBase
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
    protected override void Update()
    {
        base.Update();
    }

    //**************************************************
    protected override void Explosion(RaycastHit hit)
    {
        base.Explosion(hit);
    }
}
