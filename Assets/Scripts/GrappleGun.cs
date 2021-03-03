using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{
    [SerializeField] string grappleableTag;
    [SerializeField] Transform shootPoint;
    [SerializeField] Transform camera;
    [SerializeField] Transform player;

    /// <summary>
    /// If true, a grapple can be started by panning over the grappleable collider while holding the grapple key
    /// If false, it must be pressed down while looking at the collider
    /// </summary>
    [Tooltip("If true, a grapple can be started by panning over the grappleable collider while holding the grapple key\n" 
           + "If false, it must be pressed down while looking at the collider")]
    [SerializeField] bool startGrappleWhileHold = true;
    
    [SerializeField] float range = 100;
    
    [SerializeField] float maxTension = 0.8f;
    [SerializeField] float minTension = 0.25f;
    [SerializeField] float spring = 4.5f;
    [SerializeField] float damper = 7f;
    [SerializeField] float massScale = 4.5f;
    
    LineRenderer lineRenderer;
    Vector3 grapplePoint;
    SpringJoint joint;

    public bool IsGrappling()
    {
        return joint;
    }
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (CanStartGrappling() && IsTryingToStartGrappling())
        {
            StartGrapple();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            StopGrapple();
        }
    }

    bool CanStartGrappling()
    {
        // TODO grounding checks?
        return !IsGrappling();
    }

    bool IsTryingToStartGrappling()
    {
        // TODO properly abstracted input
        return Input.GetKeyDown(KeyCode.LeftShift) ||
               startGrappleWhileHold && Input.GetKey(KeyCode.LeftShift);
    }

    void LateUpdate()
    {
        DrawRope();
    }
    
    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, range))
        {
            if (!hit.collider.gameObject.CompareTag(grappleableTag))
            {
                return;
            }
            
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromGrapplePoint = Vector3.Distance(player.position, grapplePoint);

            // The distance grapple will try to keep from grapple point;
            joint.maxDistance = distanceFromGrapplePoint * maxTension;
            joint.minDistance = distanceFromGrapplePoint * minTension;

            joint.spring = spring;
            joint.damper = damper;
            joint.massScale = massScale;

            lineRenderer.positionCount = 2;
        }
    }

    void DrawRope()
    {
        if (!IsGrappling()) return;
        
        lineRenderer.SetPosition(0, shootPoint.position);
        lineRenderer.SetPosition(1, grapplePoint);
    }
    
    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    void StopGrapple()
    {
        lineRenderer.positionCount = 0;
        Destroy(joint);
    }
}
