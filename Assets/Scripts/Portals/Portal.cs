using System;
using System.Collections.Generic;
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
    public Camera portalCam;
    public Camera playerCam;
    List<PortalTraveller> trackedTravellers;
    RenderTexture viewTexture;

    void OnEnable()
    {
        // TODO proper handling of cameras for portals through events
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false; // We want to manually control this camera
        trackedTravellers = new List<PortalTraveller>();
        screen.material.SetInt ("displayMask", 1);
    }

    // void FixedUpdate()
    // {
    //     foreach (var traveller in trackedTravellers)
    //     {
    //         Debug.DrawRay(traveller.transform.position, traveller.transform.up, Color.white, 3);
    //     }
    // }

    // Note that there is a choice here to teleport not in the physics loop. Primarily graphical (and uniformly applied to any portalTraveller)
    // so should be fine (I think). Main consequence is less deterministic physics since multiple travellers are teleported dependent on framerate
    // One idea to investigate later is temporarily separating the player camera from the player physics object
    void LateUpdate()
    {
        if (!linkedPortal) return;
        
        for (int i = 0; i < trackedTravellers.Count; ++i)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerTransform = traveller.transform;
            Vector3 offsetFromPortal = traveller.currentUpdateStepPosition - transform.position;
            Vector3 previousOffsetFromPortal = traveller.previousUpdateStepPosition - transform.position;
            
            var stepRay = new Ray(traveller.previousUpdateStepPosition, traveller.currentUpdateStepPosition - traveller.previousUpdateStepPosition);
            
            // Debug.DrawRay(traveller.transform.position, traveller.transform.up, Color.black, 3);
            // Debug.DrawLine(traveller.previousUpdateStepPosition, traveller.currentUpdateStepPosition, new Color(1f, .5f, 0f), 4);
            // Debug.DrawRay(transform.position, offsetFromPortal*.5f, Color.blue, 4);
            
            if ( !screenCollider.bounds.IntersectRay(stepRay, out float distance))
            {
                continue;
            }
            if (distance > Vector3.Distance(traveller.previousUpdateStepPosition, traveller.currentUpdateStepPosition))
            {
                continue;
            }
            
            int portalSidePrevious = Math.Sign(Vector3.Dot(previousOffsetFromPortal, transform.forward));
            int portalSide = Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));

            // Debug.Log(
            //     $"{name} - {traveller.name}: {offsetFromPortal}{Vector3.Dot(offsetFromPortal, transform.forward)}{transform.forward} {portalSide}{portalSidePrevious}\n" +
            //     $"{name} - {traveller.name}: {traveller.previousOffsetFromPortal}{Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward)}{transform.forward} <- previous");

            // Teleport the traveller if it has crossed from one side of the portal to the other
            if (portalSide != portalSidePrevious)
            {
                // Debug.DrawRay(transform.position, previousOffsetFromPortal, Color.yellow, 4);
                // Debug.DrawRay(transform.position, offsetFromPortal*1.5f, Color.green, 4);
                
                // TODO figure out math to align portal normals (so they're symmetric) in a less retarded way (don't rotate the linked portal temporarily, actually figure out the linalg...)
                // Also note that as long as portals are set up to work from both sides (a distinction only made in terms of graphical affordances)
                // it technically doesn't matter, it's just cleaner from the perspective of the editor
                // linkedPortal.transform.Rotate(linkedPortal.transform.up, 180);
                var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerTransform.localToWorldMatrix;
                traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
                // linkedPortal.transform.Rotate(linkedPortal.transform.up, -180);

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

        if (!linkedPortal) return;
        
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
        screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt("displayMask", 0);

        // Make portal cam position and rotation the same relative to this portal as player cam is to the linked portal
        portalCam.projectionMatrix = playerCam.projectionMatrix;
        // TODO figure out math to align portal normals (so they're symmetric) in a less retarded way (don't rotate the linked portal temporarily, actually figure out the linalg...)
        // linkedPortal.transform.Rotate(linkedPortal.transform.up, 180);
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
        // linkedPortal.transform.Rotate(linkedPortal.transform.up, -180);
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        // Render to the camera (to the view texture, which is the linked target texture)
        SetPortalCameraNearClipPlane();
        UniversalRenderPipeline.RenderSingleCamera(renderContext, portalCam);
        
        // screen.enabled = true;
        linkedPortal.screen.material.SetInt("displayMask", 1);
        screen.shadowCastingMode = ShadowCastingMode.On;
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

    // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender()
    {
        if (!playerCam || !linkedPortal) return;
        ProtectScreenFromClipping(playerCam.transform.position);
    }
    
    // Sets the thickness of the portal screen so as not to clip with the camera near plane when the player goes through
    // This is less brittle if set up for SINGLE-DIRECTION portals (it is currently still dual-direction) - if doing so
    // the portal logic will still allow going through from the back face, but it will graphically clip to do so.
    float ProtectScreenFromClipping(Vector3 viewPoint)
    {
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCam.aspect;
        float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenTransform = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
        screenTransform.localScale = new Vector3(screenTransform.localScale.x, screenTransform.localScale.y, screenThickness);
        screenTransform.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        
        return screenThickness;
    }
    
    // Use custom projection matrix to align the portal camera's near clip plane to the surface of the portal
    // www.terathon.com/lengyel/Lengyel-Oblique.pdf
    void SetPortalCameraNearClipPlane()
    {
        Transform clipPlane = transform;
        int dot = Math.Sign(Vector3.Dot(clipPlane.forward, clipPlane.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDistance = -Vector3.Dot(camSpacePos, camSpaceNormal);
        Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDistance);

        Debug.Log($"{transform.parent.name}-{name} {clipPlaneCameraSpace}");
        // Update the projection based on the new clip plane
        // Calculate the projection matrix with the player camera so that the player camera settings are used
        portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
    
}
