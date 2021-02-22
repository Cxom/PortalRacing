using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Portal : MonoBehaviour
{
    // TODO fix access modifiers
    
    [Header("Main Settings")] 
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 5;
    
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
        // trackedTravellers
        screen.material.SetInt ("displayMask", 1);
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

    // Called just before the player camera is rendered
    public void Render(ScriptableRenderContext renderContext, Camera[] cams)
    {
        // TODO handle initialization via some sort of action delegate
        if (!playerCam)
        {
            if (!Camera.main)
            {
                return;
            }
            playerCam = Camera.main;
        }
        
        CreateViewTexture();
        
        // hide screen object blocking our view
        // screen.enabled = false;
        // Why is it casting shadows? Is this just a clever way to make the object invisible??
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt("displayMask", 0);

        // Make portal cam position and rotation the same relative to this portal as player cam is to the linked portal
        portalCam.projectionMatrix = playerCam.projectionMatrix;
        var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

        // Render to the camera (to the view texture, which is the linked target texture)
        UniversalRenderPipeline.RenderSingleCamera(renderContext, portalCam);
        
        linkedPortal.screen.material.SetInt("displayMask", 1);
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

}
