using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraPortalRendering : MonoBehaviour
{
    // public List<Portal> portals = new List<Portal>();

    static List<Portal> portals;
    
    void Start()
    {
        // find portals that are already in the world
        portals = FindObjectsOfType<Portal>().ToList();
        RenderPipelineManager.beginFrameRendering += DoPortalRendering;
    }

    public static void AddPortal(Portal portal)
    {
        portals.Add(portal);
    }

    public static void RemovePortal(Portal portal)
    {
        portals.Remove(portal);
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
