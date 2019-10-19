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

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }
}
