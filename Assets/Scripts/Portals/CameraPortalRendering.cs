using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraPortalRendering : MonoBehaviour
{
    // public List<Portal> portals = new List<Portal>();

    Portal[] portals;
    
    void Start()
    {
        // something about portals idek
        portals = FindObjectsOfType<Portal>();
        RenderPipelineManager.beginFrameRendering += DoPortalRendering;
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginFrameRendering -= DoPortalRendering;
    }

    void DoPortalRendering(ScriptableRenderContext renderContext, Camera[] cams)
    {
        foreach (var portal in portals)
        {
            portal.Render(renderContext, cams);
        }
        foreach (var portal in portals)
        {
            portal.PostPortalRender();
        }
    }
    
}
