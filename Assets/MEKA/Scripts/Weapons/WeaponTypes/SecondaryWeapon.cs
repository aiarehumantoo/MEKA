using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryWeapon : WeaponBase
{
    protected enum WeaponState
    {
        Normal,
        OutOfAmmo,
        Disabled
    };
    protected WeaponState weaponState = WeaponState.Normal;

    private int maxShots = 5; // universal implementation for heat/ammo (primary/secondary?)
    private int shotsLeft = 0;
    private float regenDelay = 7.5f;
    private float regenTimer = 0.0f;

    [HideInInspector]
    public GUIStyle style; // For debug

    //**************************************************
    private void GetWeaponInputs()
    {
        weaponInput.fireButtonDown = Input.GetButton("Fire2");
        weaponInput.fireWeapon = Input.GetButton("Fire2") && weaponTimer >= timeBetweenShots;
    }

    //**************************************************
    protected override void Start()
    {
        base.Start();

        shotsLeft = maxShots;

        // Debug text
        style.normal.textColor = Color.green;
        style.fontStyle = FontStyle.BoldAndItalic;
    }

    //**************************************************
    protected override void Update()
    {
        base.Update();

        GetWeaponInputs();
        AmmoRegen();
    }

    //**************************************************
    protected override void Fire()
    {
        //base.Fire();

        shotsLeft--;
        if (shotsLeft == 0)
        {
            weaponState = WeaponState.OutOfAmmo;
        }
    }

    //**************************************************
    private void AmmoRegen()
    {
        regenTimer = shotsLeft < maxShots ? regenTimer + Time.deltaTime : 0.0f;
        
        // Continuously regen ammo
        if (regenTimer >= regenDelay)
        {
            shotsLeft++;
            regenTimer = 0.0f;
        }
        if (shotsLeft > 0)
        {
            weaponState = WeaponState.Normal;
        }
    }

    //**************************************************
    private void OnGUI()
    {
        // FOR TESTING

        GUI.Label(new Rect(Screen.width / 2 + 200, Screen.height / 2, 400, 100), "Ammo: " + shotsLeft + "/" + maxShots, style);
    }
}
