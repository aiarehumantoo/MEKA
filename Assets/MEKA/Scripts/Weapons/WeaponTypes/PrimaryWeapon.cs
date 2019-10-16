using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO;
/*
 * Reload
 * Recovery from overheat. just force reload? penalty?
 * 
 */

public class PrimaryWeapon : Hitscan //TEST
{
    protected bool overHeated = false;
    private float maxHeat = 100.0f;
    private float heatPerShot = 0.5f;
    private float heatLevel = 0.0f;
    private float coolingRate = 0.2f;

    [HideInInspector]
    public GUIStyle style; // For debug

    //**************************************************
    private void GetWeaponInputs()
    {
        weaponInput.fireButtonDown = Input.GetButton("Fire1");
        weaponInput.fireWeapon = Input.GetButton("Fire1") && timer >= timeBetweenShots;
    }

    //**************************************************
    protected virtual void Start()
    {
        base.Start();

        // Debug text
        style.normal.textColor = Color.green;
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    //**************************************************
    protected virtual void Update()
    {
        base.Update();

        GetWeaponInputs();
        WeaponHeat();
    }

    //**************************************************
    private void WeaponHeat()
    {
        if (weaponInput.fireButtonDown) //+ if actually able to fire
        {
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
            // Cooling
            heatLevel = heatLevel > 0.0f ? heatLevel - coolingRate : 0.0f;
        }
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2, 400, 100), "Heat: " + heatLevel + "%", style);
    }
}
