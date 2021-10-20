using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourItem : BasePickupItem
{
    protected float armourAmount;

    /*protected override void Start()
    {
        base.Start();
    }

    protected override void GiveItem(GameObject player)
    {
        base.GiveItem(player);
    }*/

    protected void GiveArmour(GameObject player)
    {
        //player.GetComponent<health>().GiveArmour(armourAmount);
    }
}
