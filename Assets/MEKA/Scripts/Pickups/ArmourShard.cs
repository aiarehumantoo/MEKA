using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourShard : ArmourItem
{
    protected override void Start()
    {
        base.Start();

        // Configure
        respawnDuration = 15.0f;

        armourAmount = 5.0f;
    }

    protected override void GiveItem(GameObject player)
    {
        //base.GiveItem(player);

        GiveArmour(player);
    }
}
