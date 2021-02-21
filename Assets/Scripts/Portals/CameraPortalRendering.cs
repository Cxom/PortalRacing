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
        foreach (var portal in portals)
        {
            RenderPipelineManager.beginCameraRendering += portal.BeforeRender;
            RenderPipelineManager.endCameraRendering += portal.AfterRender;
        }
    }

    void OnDestroy()
    {
        foreach (var portal in portals)
        {
            RenderPipelineManager.beginCameraRendering -= portal.BeforeRender;
            RenderPipelineManager.endCameraRendering -= portal.AfterRender;
        }
    }

    // void OnPreCull()
    // {
    //     // Debug.Log("OnPreCull");
    //     // portal rendering stuff
    //     foreach (var portal in portals)
    //     {
    //         portal.Render();
    //     }
    // }
    
}
