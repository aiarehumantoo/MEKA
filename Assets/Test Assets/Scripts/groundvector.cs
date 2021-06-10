using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class groundvector : MonoBehaviour
{
    void Update()
    {
        RaycastHit shootHit; // A raycast hit to get information about what was hit
        var shootableMask = LayerMask.GetMask("Default"); // A layer mask so the raycast only hits things on the shootable layer
        Ray shootRay = new Ray(); // A ray from the gun end forwards

        // Raycast from above
        shootRay.origin = transform.position + transform.up * 2;
        shootRay.direction = -transform.up;

        // Perform the raycast against gameobjects on the shootable layer and if it hits something...
        if (Physics.Raycast(shootRay, out shootHit, 10, shootableMask))
        {
            var startpos = shootHit.point;
            var normal = shootHit.normal;
            //Debug.DrawLine(startpos, startpos + normal, Color.green);
        }

        // obtain the normals from the Mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;
        foreach (var normal in normals)
        {
            var localNormal = transform.TransformDirection(normal);
            Debug.DrawLine(transform.position, transform.position + localNormal, Color.green, 2.0f); // Draw ALL normals
        }
    }
}
