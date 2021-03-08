using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCheckpoint : MonoBehaviour
{

    [SerializeField] public int Index;

    void OnTriggerEnter(Collider other)
    {
        var racer = other.GetComponent<Racer>();
        if (!racer) return;

        if (racer.CheckpointIndex == Index + 1 || racer.CheckpointIndex == Index - 1)
        {
            // can go forward or backwards in checkpoint indices
            racer.CheckpointIndex = Index;
            Debug.Log($"{racer} through checkpoint {Index}");
        }
    }
}
