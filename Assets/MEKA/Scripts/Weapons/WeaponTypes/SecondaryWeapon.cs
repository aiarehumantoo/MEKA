using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryWeapon : Projectile // Not weapon but type?
{
    protected bool outOfAmmo = false;
    private float maxShots = 5.0f; // Use int // or universal implementation for heat/ammo (primary/secondary?)
    private float shotsLeft = 0.0f;
    private float regenDelay = 7.5f;
    private float regenTimer = 0.0f;

    [HideInInspector]
    public GUIStyle style; // For debug

    //**************************************************
    private void GetWeaponInputs()
    {
        weaponInput.fireButtonDown = Input.GetButton("Fire2");
        weaponInput.fireWeapon = Input.GetButton("Fire2") && timer >= timeBetweenShots;
    }

    //**************************************************
    protected virtual void Start()
    {
        base.Start();

        shotsLeft = maxShots;

        // Debug text
        style.normal.textColor = Color.green;
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    //**************************************************
    protected virtual void Update()
    {
        base.Update();

        GetWeaponInputs();
        WeaponAmmo();
    }

    //**************************************************
    private void WeaponAmmo()
    {
        outOfAmmo = shotsLeft == 0.0f ? true : false;
        regenTimer = shotsLeft < maxShots ? regenTimer + Time.deltaTime : 0.0f;

        if (weaponInput.fireWeapon && !outOfAmmo) //check is already done in weapons code
        {
            shotsLeft -= 1.0f;
        }

        // Continuously regen ammo
        if (regenTimer >= regenDelay && shotsLeft < maxShots)
        {
            shotsLeft += 1.0f;
            regenTimer = 0.0f;
        }
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(Screen.width / 2 + 200, Screen.height / 2, 400, 100), "Ammo: " + shotsLeft + "/" + maxShots, style);
    }
}
