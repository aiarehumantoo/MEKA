using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* NOTES;
 Materials, like textures, are not cleaned up automatically. 
 When you destroy objects whose scripts have explicitly created materials like this, you MUST manually destroy the material asset yourself. 
 Forgetting to do so causes a memory leak.
 private void OnDestroy()
 {
     Destroy(this.renderer.material);
 }

 */

public class ContinuousBeamSFX : MonoBehaviour
{
    private Vector3 beamStartPos;
    private Vector3 beamEndPos;

    private LineRenderer beamLine;

    //**************************************************
    public void Setup(string materialPath, Vector3 weaponPos)
    {
        // Add LineRenderer
        beamLine = this.gameObject.AddComponent<LineRenderer>();

        // Configure beam and set length to zero
        beamStartPos = new Vector3(weaponPos.x, weaponPos.y, weaponPos.z); // Get weapon local position // Location is wrong since weapon parent location differs from camera`s (and cant parent weapons to camera directly because of viewmodels) // Using empty gameobject as placeholder until this is fixed            
        beamEndPos = beamStartPos;

        // Set LineRenderer
        beamLine.useWorldSpace = false; // Local space for continuous sfx, world for single shot
        beamLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        beamLine.receiveShadows = false;
        beamLine.startWidth = 0.3f;
        beamLine.endWidth = 0.1f;
        beamLine.material = Resources.Load<Material>(materialPath);
    }

    //**************************************************
    public void UpdateBeam(float beamLength)
    {
        beamEndPos = new Vector3(0.0f, 0.0f, beamLength);

        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamEndPos);
    }

    //**************************************************
    public void DisableSFX()
    {
        // Disable beam SFX
        beamLine.SetPosition(0, beamStartPos);
        beamLine.SetPosition(1, beamStartPos);
    }
}
