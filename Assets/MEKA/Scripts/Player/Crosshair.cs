using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public Texture2D crosshairImage;
    float crosshairSize = 0.15f;                //scaler

    void OnGUI()
    {
        float xMin = (Screen.width / 2) - ((crosshairImage.width * crosshairSize) / 2);
        float yMin = (Screen.height / 2) - ((crosshairImage.width * crosshairSize) / 2);
        GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width * crosshairSize, crosshairImage.height * crosshairSize), crosshairImage);
    }
}
