using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//**************************************************
public class RailGun : PrimaryWeapon
{
    // SFX
    private SingleBeamSFX sfx = new SingleBeamSFX();

    //**************************************************
    protected override void Start()
    {
        base.Start();

        isHitscan = true;
        damagePerShot = 30.0f;
        timeBetweenShots = 1.5f;
        maximumRange = 50.0f;
        weaponTimer = timeBetweenShots; // Start without cooldown

        heatPerShot = 10.0f;

        // SFX setup
        float sfxDisplayTime = 0.35f;
        LineRenderer beamLine = this.gameObject.AddComponent<LineRenderer>();
        sfx.Setup(beamLine, "Materials/Green", sfxDisplayTime);
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();

        Fire();

        sfx.UpdateSFXTimer(Time.deltaTime); // Update sfx timer
    }

    //**************************************************
    protected override void Fire()
    {
        if (weaponInput.fireWeapon && !overHeated)
        {
            // Reset the timer.
            weaponTimer = 0.0f;

            base.Fire(); // Fire weapon

            sfx.UpdateBeam(beamSFXStartPos, beamSFXEndPos);
        }
        else
        {
            // Disable SFX
            sfx.DisableSFX();
        }
    }
}
