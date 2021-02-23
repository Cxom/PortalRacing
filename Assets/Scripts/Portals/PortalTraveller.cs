using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    public Vector3 previousPhysicsStepPosition { get; private set; }
    Vector3 currentPhysicsStepPosition; // private;

    void FixedUpdate()
    {
        previousPhysicsStepPosition = currentPhysicsStepPosition;
        currentPhysicsStepPosition = transform.position;
    }

    public virtual void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        // Debug.Log($"TELEPORTING {name}: {pos} -> {rot}");
        // Debug.DrawRay(transform.position, pos - transform.position, Color.red, 3);
        transform.position = pos;
        currentPhysicsStepPosition = pos;
        previousPhysicsStepPosition = pos;
        transform.rotation = rot;
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

}
