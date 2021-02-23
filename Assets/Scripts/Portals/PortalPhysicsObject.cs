using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPhysicsObject : PortalTraveller
{

    
    Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        base.Teleport(fromPortal, toPortal, pos, rot);
        _rigidbody.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(_rigidbody.velocity));
        _rigidbody.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(_rigidbody.angularVelocity));
    }
}
