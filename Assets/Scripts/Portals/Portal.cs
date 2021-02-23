using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// TODO namespace

public class Portal : MonoBehaviour
{
    // TODO fix access modifiers
    
    [Header("Main Settings")] 
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 5;
    public Collider screenCollider;
    
    [Header("Advanced Settings")] 
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    
    // Private variables
    Camera portalCam;
    public Camera playerCam;
    List<PortalTraveller> trackedTravellers;
    RenderTexture viewTexture;

    void Awake()
    {
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false; // We want to manually control this camera
        trackedTravellers = new List<PortalTraveller>();
        screen.material.SetInt ("displayMask", 1);
    }

    void FixedUpdate()
    {
        for (int i = 0; i < trackedTravellers.Count; ++i)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerTransform = traveller.transform;
            Vector3 offsetFromPortal = traveller.transform.position - transform.position;
            Vector3 previousOffsetFromPortal = traveller.previousPhysicsStepPosition - transform.position;

            var stepRay = new Ray(traveller.previousPhysicsStepPosition, traveller.transform.position - traveller.previousPhysicsStepPosition);
            
            // Debug.DrawRay(stepRay.origin, stepRay.direction, new Color(1f, .5f, 0f), 3);
            // Debug.DrawRay(transform.position, offsetFromPortal*.5f, Color.blue, 3);
            
            if ( !screenCollider.bounds.IntersectRay(stepRay))
            {
                return;
            }
            
            int portalSidePrevious = Math.Sign(Vector3.Dot(previousOffsetFromPortal, transform.forward));
            int portalSide = Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));

            // Debug.Log(
            //     $"{name} - {traveller.name}: {offsetFromPortal}{Vector3.Dot(offsetFromPortal, transform.forward)}{transform.forward} {portalSide}{portalSidePrevious}\n" +
            //     $"{name} - {traveller.name}: {traveller.previousOffsetFromPortal}{Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward)}{transform.forward} <- previous");

            // Teleport the traveller if it has crossed from one side of the portal to the other
            if (portalSide != portalSidePrevious)
            {
                // Debug.DrawRay(transform.position, previousOffsetFromPortal, Color.yellow, 3);
                // Debug.DrawRay(transform.position, offsetFromPortal*1.5f, Color.green, 3);

                var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix *
                        travellerTransform.localToWorldMatrix;
                traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);

                // Can't rely on OnTriggerEnter/Exit to be called next frame because it depends on the physics loop, not the update loop
                // This results in the portal teleporting twice or more by unpredictably screwing with the portal side calculations (I think)
                linkedPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                --i;
            }
        }
    }
    
    // Called just before the player camera is rendered
    public void Render(ScriptableRenderContext renderContext, Camera[] cams)
    {
        // TODO handle initialization via some sort of action delegate when players are spawned
        if (!playerCam)
        {
            if (!Camera.main)
            {
                return;
            }
            playerCam = Camera.main;
        }
        
        // TODO This check may need to be updated with more cameras that can see portals
        if (!VisibleFromCamera(linkedPortal.screen, playerCam))
        {
            return;
        }
        
        CreateViewTexture();
        
        // hide screen object blocking our view
        // Why is it casting shadows? Is this just a clever way to make the object invisible??
        // TODO figure out the portal shadow situation and desired behaviour and correct it
        // screen.enabled = false;
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt("displayMask", 0);

        // Make portal cam position and rotation the same relative to this portal as player cam is to the linked portal
        portalCam.projectionMatrix = playerCam.projectionMatrix;
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        // Render to the camera (to the view texture, which is the linked target texture)
        UniversalRenderPipeline.RenderSingleCamera(renderContext, portalCam);
        
        // screen.enabled = true;
        linkedPortal.screen.material.SetInt("displayMask", 1);
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    static bool VisibleFromCamera(Renderer renderer, Camera camera) {
        Plane[] viewFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(viewFrustumPlanes, renderer.bounds);
    }
    
    void CreateViewTexture()
    {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (viewTexture != null)
            {
                viewTexture.Release();
            }
            viewTexture = new RenderTexture(Screen.width, Screen.height,  0);
            // Render the view from the portal camera to the view texture
            portalCam.targetTexture = viewTexture;
            // Display the view texture on the screen of the linked portal
            // TODO Change for dynamic portals (link may not exist)
            linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller)
        {
            OnTravellerEnterPortal(traveller);
        }
    }
    
    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            // Debug.DrawRay(transform.position, traveller.transform.position - transform.position, Color.magenta, 3);
            
            traveller.EnterPortalThreshold();
            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }
    
}