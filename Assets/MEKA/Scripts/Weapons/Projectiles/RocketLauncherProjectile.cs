using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncherProjectile : ProjectileBase
{
    protected override void Start()
    {
        base.Start();

        damagePerShot = 90.0f;
        splashDamage = 50.0f;
        splashRadius = 2.0f;
    }

    void OnTriggerEnter(Collider other)
    {
        // Hits terrain 
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            Debug.Log("dmg " +damagePerShot);
        }
    }
}
