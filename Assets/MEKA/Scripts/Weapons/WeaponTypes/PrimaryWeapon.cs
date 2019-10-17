using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO;
/*
 * Reload
 * Recovery from overheat. just force reload? penalty?
 * 
 */

public class PrimaryWeapon : Weapon
{
    protected bool overHeated = false;
    private float maxHeat = 100.0f;
    private float heatPerShot = 0.1f; //isnt per shot atm, just per tick while m1 is down
    private float heatLevel = 0.0f;     // cooling rate same as heating up.
    private float coolingRate = 0.1f;
    private float coolingTimer = 0.0f;

    [HideInInspector]
    public GUIStyle style; // For debug

    //**************************************************
    private void GetWeaponInputs()
    {
        weaponInput.fireButtonDown = Input.GetButton("Fire1");
        weaponInput.fireWeapon = Input.GetButton("Fire1") && timer >= timeBetweenShots;

        weaponInput.reload = Input.GetButton("Reload");
    }

    //**************************************************
    protected override void Start()
    {
        base.Start();

        // Debug text
        style.normal.textColor = Color.green;
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();

        GetWeaponInputs();
        WeaponHeat();
        Reload();
    }

    //**************************************************
    private void WeaponHeat()
    {
        if (overHeated)
        {
            StartCoroutine(ResetHeat());
            return;
        }

        if (weaponInput.fireButtonDown) //+ if actually able to fire
        {
            coolingTimer = 0.0f;

            // Heating
            heatLevel += heatPerShot;
            if (heatLevel > maxHeat)
            {
                heatLevel = maxHeat;
                overHeated = true;
            }
        }
        else //if (!overHeated)
        {
            float coolingDelay = 1.0f; // Delay before passive cooling starts
            coolingTimer += Time.deltaTime;

            // Passive cooling
            heatLevel -= heatLevel > 0.0f && coolingTimer >= coolingDelay ? coolingRate : 0.0f;
            if (heatLevel < 0.0f)
            {
                heatLevel = 0.0f;
            }
        }
    }

    //**************************************************
    private void Reload()
    {
        if (weaponInput.reload)
        {
            overHeated = true;
            StartCoroutine(ResetHeat());
        }
    }

    IEnumerator ResetHeat()
    {
        float reloadDuration = 2.5f;
        yield return new WaitForSeconds(reloadDuration);
        heatLevel = 0.0f;
        overHeated = false;
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2, 400, 100), "Heat: " + heatLevel + "%", style);
    }
}
