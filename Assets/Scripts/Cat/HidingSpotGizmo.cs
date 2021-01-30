using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingSpotGizmo : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);

        Gizmos.DrawWireCube(transform.position, new Vector3(.5f, .01f, .5f));
    }
}
