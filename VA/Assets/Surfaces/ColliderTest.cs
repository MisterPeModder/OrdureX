using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collision started: {transform.position} -> {other.transform.position}");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"Collision ended: {transform.position} -> {other.transform.position}");
    }
}
