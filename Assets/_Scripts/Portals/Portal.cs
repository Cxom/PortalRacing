using System;
using System.Collections.Generic;
using Portals;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

// TODO namespace

public class Portal : MonoBehaviour
{
    // TODO fix access modifiers

    [Header("Main Settings")] 
    Portal linkedPortal;
    public Portal LinkedPortal {
        get => linkedPortal;

        set
        {
            linkedPortal = value;
            UpdatePortalActiveState();
        } 
    }
    
    public MeshRenderer screen;
    public int recursionLimit = 5;
    public Renderer PortalBorder;
    public Collider screenCollider;
    [FormerlySerializedAs("portalMaterial")]
    public Material linkedPortalMaterial;
    [FormerlySerializedAs("unlinkedMaterial")]
    public Material unlinkedPortalMaterial;
    
    [Header("Advanced Settings")] 
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    
    // Private variables
    PortalGun portalOwner;
    bool primary;
    
    public Camera portalCam;
    public Camera playerCam;
    List<PortalTraveller> trackedTravellers;
    RenderTexture viewTexture;

    // TODO debug confirm that we are properly handling adjustment of both trackedTravellers in the Portal class and trackedPortals in the PortalTraveller class WHEN
    // - A traveller is in a threshold and shoots a portal out of it
    // - A traveller is not in a threshold and shoots a portal close enough that they are immediately in it
    // We also need to just clear these values when a portal is disabled no matter what
    
    void OnEnable()
    {
        // TODO proper handling of cameras for portals through events
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false; // We want to manually control this camera
        trackedTravellers = new List<PortalTraveller>();
        UpdatePortalActiveState();
    }
    
    void UpdatePortalActiveState()
    {
        if (linkedPortal)
        {
            screen.material = linkedPortalMaterial;
            screen.material.SetInt ("displayMask", 1);
            screenCollider.isTrigger = true;
        }
        else
        {
            screen.material = unlinkedPortalMaterial;
            screenCollider.isTrigger = false;
        }
    }

    public void Activate(bool primary, PortalGun portalGun)
    {
        this.primary = primary;
        portalOwner = portalGun;
        
        CameraPortalRendering.AddPortal(this);
        // TODO This may need to be setting materials, not colors, so we can have different patterns for different players for increased accessibility
        PortalBorder.materials[0].color = primary ? portalGun.primaryColor : portalGun.secondaryColor;
        PortalBorder.materials[0].EnableKeyword("_EMISSION");
        PortalBorder.materials[0].SetColor("_EmissionColor",
            primary ? portalGun.primaryColor : portalGun.secondaryColor);

        // We need one portal to be flipped around - just make it the secondary one
        transform.localRotation = Quaternion.Euler(0, !primary ? 180 : 0, 0);
        
        gameObject.SetActive(true);
        PortalBorder.gameObject.SetActive(true);
    }
    
    public void Deactivate()
    {
        portalOwner.PortalWasDeactivated(primary);
        portalOwner = null;
        
        foreach (PortalTraveller trackedTraveller in trackedTravellers)
        {
            trackedTraveller.ExitPortalThreshold(this);
        }
        trackedTravellers.Clear();
            
        CameraPortalRendering.RemovePortal(this);
        // Not sure if these really do anything, since it should be gone, might just be insurance
        PortalBorder.materials[0].color = Color.white;
        PortalBorder.materials[0].SetColor("_EmissionColor", Color.black);
        
        if (LinkedPortal != null)
        {
            LinkedPortal.LinkedPortal = null;
            LinkedPortal = null;
        }
        
        // Reset rotation in case portal was flipped around as a secondary portal
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        gameObject.SetActive(false);
        PortalBorder.gameObject.SetActive(false);
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
    // TODO this (no longer?) seems to be handling camera logic - does it even need to be LateUpdate if it's not in the physics loop?
    void LateUpdate()
    {
        if (!linkedPortal) return;
        
        Debug.DrawLine(transform.position, linkedPortal.transform.position);
        
        for (int i = 0; i < trackedTravellers.Count; ++i)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerTransform = traveller.transform;
            Vector3 offsetFromPortal = traveller.currentUpdateStepPosition - transform.position;
            Vector3 previousOffsetFromPortal = traveller.previousUpdateStepPosition - transform.position;
            
            var stepRay = new Ray(traveller.previousUpdateStepPosition, traveller.currentUpdateStepPosition - traveller.previousUpdateStepPosition);
            
            Debug.DrawRay(traveller.transform.position, traveller.transform.up, Color.black, 3);
            Debug.DrawLine(traveller.previousUpdateStepPosition, traveller.currentUpdateStepPosition, new Color(1f, .5f, 0f), 4);
            Debug.DrawRay(transform.position, offsetFromPortal*.5f, Color.blue, 4);
            
            // TODO placement in this code for handling screen-intersection detection
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerTransform.localToWorldMatrix;
            
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
                traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
                // linkedPortal.transform.Rotate(linkedPortal.transform.up, -180);

                // Can't rely on OnTriggerEnter/Exit to be called next frame because it depends on the physics loop, not the update loop
                // This results in the portal teleporting twice or more by unpredictably screwing with the portal side calculations (I think)
                linkedPortal.OnTravellerEnterPortal(traveller);
                --i;
            }
        }
    }
    
    // Called just before the player camera is rendered
    public void Render(ScriptableRenderContext renderContext, Camera[] cams)
    {
        // TODO handle initialization via some sort of action delegate when players are spawned (is this already done??)
        Assert.IsNotNull(playerCam, "playerCam is null!");
        if (playerCam == null)
        {
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found!");
                return;
            }
            playerCam = Camera.main;
            Debug.LogError("Assigned player camera");
        }
        
        if (linkedPortal == null) return;

        // We're checking the linked portal's screen because we're RENDERING THE LINKED PORTAL'S screen.
        // We do not render our own portal screen. It might be more responsible to be responsible for rendering our own
        // screen, but in that case, the camera for doing that will be positioned at the linked portal. We have to
        // take some responsibility for EITHER the SCREEN or the CAMERA at the linked portal, no way around it.
        // TODO analyze if it is more responsible to render our own screen
        // TODO This check may need to be updated with more cameras that can see portals
        if (!VisibleFromCamera(linkedPortal.screen, playerCam))
        {
            return;
        }
        
        CreateViewTexture();
        
        // We set the shadowCastingMode to shadows only as a trick to make the screen invisible to the portal camera,
        // which needs to see through it to know what to render
        // The shadows rendering are fine because the portal still "exists within a panel" that we would expect to cast the same shadows
        // Still, it would be edifying to do a performance and result comparison of the following options someday
        //  - Disabling the screen object (probably a bad idea, but I can't figure out what consequences doing this actually triggers immediately and what doesn't get triggered because the state at the end of the frame is the same as at the start)
        //  - Setting the screen object shadowCastingMode to ShadowsOnly (are there potentially unwanted shadows? probably not, the portal panel still exists and would be casting shadows)
        //  - Setting the layer mask to a layer that is not rendered by the camera (TODO could the screens just permanently exist on a layer not rendered by the portal cameras?)
        //  - Setting the material to a transparent material
        //  - Using a shader that discards all fragments
        //  - Setting the alpha value of the material to 0
        screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        // linkedPortal.screen.material.SetInt("displayMask", 0);

        // Make portal cam position and rotation the same relative to this portal as player cam is to the linked portal
        portalCam.projectionMatrix = playerCam.projectionMatrix;
        // TODO figure out if this comment is still relevant - math to align portal normals (so they're symmetric) in a less retarded way (don't rotate the linked portal temporarily, actually figure out the linalg...)
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
        // TODO 2024 - the texture is not being successfully reused - rather this is causing an issue where previously used portals just render gray - figure out how to properly reuse render textures - DO NOT REINITIALIZE EVERY TIME
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
        }
        linkedPortal.screen.material.SetTexture("_MainTex", portalCam.targetTexture);
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
            traveller.EnterPortalThreshold(this);
            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold(this);
            trackedTravellers.Remove(traveller);
        }
    }

    // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender()
    {
        if (playerCam == null || linkedPortal == null) return;
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

        // Debug.Log($"{transform.parent.name}-{name} {clipPlaneCameraSpace}");
        // Update the projection based on the new clip plane
        // Calculate the projection matrix with the player camera so that the player camera settings are used
        portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    // I don't like that the reset of the position happens in PortalTraveller but we're updating it here
    // Seems like failure to encapsulate
    // TODO investigate refactoring
    void UpdateSliceParams(PortalTraveller traveller)
    {
        // Calculate the slice normal
        int side = SideOfPortal(traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;
        
        // Calculate the slice center
        Vector3 slicePosition = transform.position;
        Vector3 cloneSlicePosition = linkedPortal.transform.position;
        
        // Apply parameters
        for (int i = 0; i < traveller.originalMaterials.Length; ++i)
        {
            traveller.originalMaterials[i].SetVector("sliceCenter", slicePosition);
            traveller.originalMaterials[i].SetVector("sliceNormal", sliceNormal);

            traveller.cloneMaterials[i].SetVector("sliceCenter", cloneSlicePosition);
            traveller.cloneMaterials[i].SetVector("sliceNormal", cloneSliceNormal);
        }
    }

    int SideOfPortal(Vector3 position)
    {
        return Math.Sign(Vector3.Dot(position - transform.position, transform.forward));
    }
    
}
