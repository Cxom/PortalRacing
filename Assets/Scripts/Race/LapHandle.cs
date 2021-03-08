using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapHandle : MonoBehaviour
{

    [SerializeField] int numCheckpoints;

    void OnTriggerEnter(Collider other)
    {
        var racer = other.GetComponent<Racer>();
        if (!racer) return;

        if (racer.CheckpointIndex == numCheckpoints)
        {
            // reached the final checkpoint
            racer.FinishLap();
        }
    }
}
