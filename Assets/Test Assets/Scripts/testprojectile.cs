using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testprojectile : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Hits terrain 
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            transform.GetComponent<Rigidbody>().velocity = Vector3.zero; // stopping projectile instead of destroying it. for debugging. projectile is destroyed anyway when its lifetime expires
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(1.0f);

        // Destroy projectile
        Destroy(gameObject);
    }
}
