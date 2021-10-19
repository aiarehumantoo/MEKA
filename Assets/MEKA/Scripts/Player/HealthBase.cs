using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBase : MonoBehaviour
{
    protected float maximumHealth = 100.0f;
    protected float currentHealth = 100.0f;

    //**************************************************
    protected virtual void Start()
    {

    }

    //**************************************************
    public bool ReceiveDamage(float dmg)
    {
        currentHealth -= dmg;
        
        if (currentHealth <= 0.0f)
        {
            currentHealth = 0.0f;
            OnDeath();

            // Return true if damage is lethal. For hitsounds
            return true;
        }

        return false;
    }

    //**************************************************
    public bool IsDead()
    {
        return currentHealth <= 0.0f;
    }

    //**************************************************
    private void OnDeath()
    {
        // TODO; death state
        // disable movement, taking damage, unable to fire
        // death camera?
        // respawn timer / game over screen
    }

    //**************************************************
    private void Respawn()
    {
        currentHealth = maximumHealth;
    }
}
