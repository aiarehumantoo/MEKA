using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
public class Hitscan : Weapon
{
    protected Ray shootRay = new Ray(); // A ray from the gun end forwards.
    protected float maximumRange; // Maximum range

    //**************************************************
    protected override void Start()
    {
        base.Start();
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();
    }

    //**************************************************
    protected override void Fire()
    {
        //base.Fire(); // Access Weapon.Fire() // Empty

        // Shoot ray forward from camera
        shootRay.origin = playerCamera.transform.position;
        shootRay.direction = playerCamera.transform.forward;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, maximumRange, shootableMask))
        {
            // Hits CapsuleCollider of player
            if (shootHit.collider.tag == "Player" && shootHit.collider is CapsuleCollider)
            {
                // Deal damage
            }
        }

        Debug.Log("Fired a hitscan weapon");
    }
}
