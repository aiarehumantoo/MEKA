using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO;
/*
 * Reload
 * Recovery from overheat. just force reload? penalty?
 * 
 * Bug; After reloading (over overheating) heat generation has delay before starting
 * 
 */

public class PrimaryWeapon : WeaponBase
{
    protected bool overHeated = false;
    private const float maxHeat = 100.0f; // <-- TODO: add constants
    protected float heatPerShot; // Heat generated per shot
    private float heatLevel = 0.0f;
    private float coolingRate = 0.1f; // Passive cooling per Update
    private float coolingTimer = 0.0f;

    [HideInInspector]
    public GUIStyle style; // For debug

    //**************************************************
    private void GetWeaponInputs()
    {
        weaponInput.fireButtonDown = Input.GetButton("Fire1");
        weaponInput.fireWeapon = Input.GetButton("Fire1") && weaponTimer >= timeBetweenShots;

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
        WeaponCooling();
        Reload();
    }

    //**************************************************
    protected override void Fire()
    {
        base.Fire();
        WeaponHeat();
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
    }

    //**************************************************
    private void WeaponCooling()
    {
        if (overHeated)
        {
            StartCoroutine(ResetHeat());
        }
        else if (!weaponInput.fireButtonDown)
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
