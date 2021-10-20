using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePickupItem : MonoBehaviour
{
    // Respawn
    protected float respawnDuration = -1.0f;
    private float respawnTimer = 0.0f;
    private bool hidden = false;
    private MeshRenderer meshRenderer;

    // Animate
    private const float rotationSpeed = 0.4f;
    private const float moveDuration = 2.0f;
    private float moveSpeed = 0.1f;
    private float timer;

    protected virtual void Start()
    {
        meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
    }

    protected virtual void GiveItem(GameObject player)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if ( !hidden && other.tag == "Player")
        {
            GiveItem(other.gameObject);

            if (respawnDuration < 0.0f)
            {
                // Item does not respawn -> Destroy after use
                Destroy(this.gameObject);
            }
            else
            {
                // Hide & start respawn timer
                meshRenderer.enabled = false;
                respawnTimer = 0.0f;
                hidden = true;
            }
        }
    }

    private void Update()
    {
        if (hidden)
        {
            // Respawn item once timer has expired
            respawnTimer += Time.deltaTime;

            if (respawnTimer >= respawnDuration)
            {
                meshRenderer.enabled = true;
                hidden = false;
            }
        }
        else
        {
            Animate();
        }
    }

    private void Animate()
    {
        // Add the time since Update was last called to the timer.
        timer += Time.deltaTime;

        if (timer >= moveDuration && Time.timeScale != 0)
        {
            // Change direction
            timer = 0.0f;
            moveSpeed = -moveSpeed;

        }

        // move & rotate
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + rotationSpeed, transform.eulerAngles.z);
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}
