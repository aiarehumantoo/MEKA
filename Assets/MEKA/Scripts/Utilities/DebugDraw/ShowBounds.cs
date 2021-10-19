using UnityEngine;
using System.Collections;

// GeometryUtility.CalculateBounds - example

public class ShowBounds : MonoBehaviour
{
    void Awake()
    {
        transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        transform.Rotate(10.0f, 30.0f, -50.0f, Space.Self);

        Debug.Log(transform.localToWorldMatrix);
    }

    void OnDrawGizmosSelected()
    {
        LightProbeGroup lightProbeGroup = GetComponent<LightProbeGroup>();

        Vector3 center = transform.position;
        var bounds = GeometryUtility.CalculateBounds(lightProbeGroup.probePositions, transform.localToWorldMatrix);
        Gizmos.color = new Color(1, 1, 1, 0.25f);
        Gizmos.DrawCube(center, bounds.size);
        Gizmos.DrawWireCube(center, bounds.size);
    }
}