using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    public Vector3 previousPhysicsStepPosition { get; private set; }
    Vector3 currentPhysicsStepPosition; // private;
    
    public Vector3 previousUpdateStepPosition { get; private set; }
    public Vector3 currentUpdateStepPosition { get; private set; }

    int teleportTracking;
    
    void FixedUpdate()
    {
        previousPhysicsStepPosition = currentPhysicsStepPosition;
        currentPhysicsStepPosition = transform.position;
    }

    void Update()
    {
        previousUpdateStepPosition = currentUpdateStepPosition;
        currentUpdateStepPosition = transform.position;
    }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        // Debug.Log($"TELEPORTING {name}: {pos} -> {rot}");
        // Debug.DrawRay(transform.position, pos - transform.position, Color.red, 4);
        transform.position = pos;
        currentPhysicsStepPosition = pos;
        previousPhysicsStepPosition = pos;
        currentUpdateStepPosition = pos;
        previousUpdateStepPosition = pos;
        transform.rotation = rot;

        StartCoroutine(TrackTeleport());
    }

    IEnumerator TrackTeleport()
    {
        ++teleportTracking;
        yield return new WaitForSeconds(2);
        --teleportTracking;
    }

    // Called when a traveller first touches a portal
    public virtual void EnterPortalThreshold()
    {

    }

    // TODO figure out "Except when teleporting" - Does that just mean it's not called unless there's a barrier cross?
    // Called once a traveller is no longer touching a portal (except when teleporting)
    public virtual void ExitPortalThreshold()
    {

    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 200, 200, 20), $"TELEPORTS: {teleportTracking}");
    }
}
