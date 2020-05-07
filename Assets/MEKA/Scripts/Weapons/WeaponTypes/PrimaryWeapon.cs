using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

// TODO;
/*
 * move weaponstate to base?                    *********
 *      primary is heat based, secondary has ammo
 *        so maybe better to keep systems seperate
 *        *functionality should be the same. normal, noammo, disabled
 *        
 *    
 *    overheating vs "reload" vs passive cooling
 *    Overheat -> forced passive cooling to 0?
 *      Reload->fast heat dissipation
 *      *is there need for penalty? autoreload might suffice
 * 
 */

public class PrimaryWeapon : WeaponBase
{
    protected enum WeaponState
    {
        Normal, // Ready to fire
        Overheated, // Overheated
        Disabled // Disabled during dodge, etc. // Allow passive cooling?
    };
    protected WeaponState weaponState = WeaponState.Normal;

    private const float maxHeat = 100.0f; // Maximum heat
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
        weaponInput.fireWeapon = Input.GetButton("Fire1") && weaponTimer >= timeBetweenShots /*&& weaponState == WeaponState.Normal*/;

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
        //base.Fire();
        WeaponHeat();
    }

    //**************************************************
    private void WeaponHeat()
    {
        coolingTimer = 0.0f;

        // Heating
        heatLevel += heatPerShot;
        if (heatLevel >= maxHeat)
        {
            heatLevel = maxHeat;
            StartCoroutine(ResetHeat());
        }
    }

    //**************************************************
    private void WeaponCooling()
    {
        if (weaponState == WeaponState.Normal)
        {
            float coolingDelay = timeBetweenShots + 1.0f; // Delay before passive cooling starts
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
        if (weaponInput.reload && weaponState == WeaponState.Normal && heatLevel > 0.0)
        {
            StartCoroutine(ResetHeat());
        }
    }

    IEnumerator ResetHeat()
    {
        weaponState = WeaponState.Overheated;
        float reloadDuration = 2.5f;
        yield return new WaitForSeconds(reloadDuration);
        heatLevel = 0.0f;
        weaponState = WeaponState.Normal;
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height / 2, 400, 100), "Heat: " + heatLevel + "%", style);
    }
}
