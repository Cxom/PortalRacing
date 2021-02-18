using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingTool : MonoBehaviour
{

    public Camera playerCamera;
    [SerializeField] Transform firePoint;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] float maxRange;

    void Start()
    {
        playerCamera = Camera.main; // TODO fix
    }

    void Update()
    {
        // TODO Abstract this input
        if (Input.GetKey(KeyCode.F))
        {
            Ping();
        }
        else
        {
            ResetPing();
        }
    }

    void Ping()
    {
        lineRenderer.SetPosition(0, firePoint.position);
        
        // TODO maybe should be center of camera. Write exploratory code for mousepos with fp camera
        RaycastHit hit;
        var mousePos = Input.mousePosition;
        var mouseRay = playerCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, maxRange))
        {
            lineRenderer.enabled = true;
            if (hit.collider)
            {
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                // TODO some sort of dud indicator?? But also if not hit pingable
                lineRenderer.SetPosition(1, mouseRay.GetPoint(maxRange));
            }
        } 
    }

    void ResetPing()
    {
        lineRenderer.enabled = false;
        // lineRenderer.SetPosition(1, firePoint.position);
    }
}
