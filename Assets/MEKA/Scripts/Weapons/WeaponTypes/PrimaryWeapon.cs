using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

// TODO;
/*
 * 
 */

public class PrimaryWeapon : WeaponBase
{
    private bool reloading = false;
    protected bool overHeated = false;
    private const float maxHeat = 100.0f; // <-- TODO: add constants
    protected float heatPerShot; // Heat generated per shot
    private float heatLevel = 0.0f;
    private float coolingRate = 0.1f; // Passive cooling per Update
    private float coolingTimer = 0.0f;

    public Image heatBar; // HUD for displaying heat level

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

        heatBar.fillAmount = heatLevel / maxHeat;
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
        if (overHeated && !reloading)
        {
            StartCoroutine(ResetHeat());
            return;
        }
        

        //if (weaponInput.fireButtonDown) //+ if actually able to fire
        //{
            coolingTimer = 0.0f;

            // Heating
            heatLevel += heatPerShot;
            if (heatLevel > maxHeat)
            {
                heatLevel = maxHeat;
                overHeated = true;
            }
        //}
    }

    //**************************************************
    private void WeaponCooling()
    {
        if (overHeated && !reloading)
        {
            StartCoroutine(ResetHeat());
        }
        else if (!weaponInput.fireButtonDown && !overHeated)
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
        reloading = true;
        float reloadDuration = 2.5f;
        yield return new WaitForSeconds(reloadDuration);
        heatLevel = 0.0f;
        overHeated = false;
        reloading = false;
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2, 400, 100), "Heat: " + heatLevel + "%", style);
    }
}
