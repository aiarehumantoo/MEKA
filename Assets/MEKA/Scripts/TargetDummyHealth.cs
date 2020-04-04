using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDummyHealth : HealthBase
{
    protected override void Start()
    {
        //base.Start();

        currentHealth = Mathf.Infinity;
    }
}
