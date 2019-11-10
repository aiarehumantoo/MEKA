using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveleftright : MonoBehaviour
{
    private float move = 0.05f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 5f)
        {
            timer = 0f;
            move = -move;
        }

        transform.position += transform.right * move;
    }
}
