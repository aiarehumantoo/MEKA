using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CornerTest : MonoBehaviour
{
    public GameObject _position2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // NOTES;
    // Spherecast does not seem to register colliders inside the radius
    // ie. starting new spherecast from  hit height of previous one results in no hit targets
    // since spherecast overlaps with the ground from the start

    // Update is called once per frame
    void Update()
    {
        var color = Color.blue;
        var radius = 0.5f;
        var height = 1.8f;

        var position = transform.position;
        var position2 = _position2.transform.position;
        // make sure that .y doesnt get lower every loop

        bool ApplyDownforce(Vector3 pos, bool first, string name)
        {
            RaycastHit hit;
            var groundLayer = LayerMask.GetMask("Environment");

            Debug.DrawLine(pos, pos + Vector3.down * height, Color.cyan, 1.0f);
            if (Physics.SphereCast(pos, radius, Vector3.down, out hit, height, groundLayer))
            {
                //Debug.Log(name +" hit");
                //Debug.Log("hitheight; " + hit.point.y);

                //***
                // closestpoint from correct height
                var testPos = pos;
                testPos.y = hit.point.y; //13.5
                var closestPoint = hit.collider.ClosestPoint(testPos); // !!!only works on convex
                //***

                Debug.DrawLine(closestPoint, closestPoint + Vector3.up * 2, Color.green, 1.0f);
                //Debug.DrawLine(hit.point, hit.point + Vector3.up * 2, Color.green, 1.0f);

                Vector3 points = Vector3.zero;
                float dists = Mathf.Infinity;
                var verts = hit.transform.GetComponent<MeshCollider>().sharedMesh.vertices;
                foreach (var v in verts)
                {
                    var vw = hit.transform.TransformPoint(v);
                    var vdist = Vector3.Distance(pos, vw);
                    if (dists > vdist)
                    {
                        points = vw;
                        dists = vdist;
                    }
                }

                var edgeDir = points - hit.point;
                edgeDir.y = 0;
                if (edgeDir.magnitude < 1.0e-5) // corner > points overlap
                {
                    //if (!first)
                    {
                        Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.black, 5.0f);
                    }
                    //position2.y = hit.point.y + radius;
                    return false;
                }
            }
            return true;
        }

        ApplyDownforce(position, true, "raycast1");
        //position2.y = 13.5f;
        if (!ApplyDownforce(position2, false, "raycast2"))
        {
            //Debug.Log("corner");
        }
    }
}
