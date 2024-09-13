using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Racer : MonoBehaviour
{
    public int CheckpointIndex { get; set; }
    int lapNumber;
    
    public void FinishLap()
    {
        CheckpointIndex = 0;
        ++lapNumber;
        // TODO race finish checking??
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 250, 200, 20), $"Laps: {lapNumber}");
        GUI.Label(new Rect(20, 300, 200, 20), $"Checkpoint: {CheckpointIndex}");
    }
}
