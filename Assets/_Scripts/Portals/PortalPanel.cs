using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PortalPanel : MonoBehaviour, IPortalable
{
    bool active;
    bool primary;
    [SerializeField] Portal portal;
    [SerializeField] GameObject panel;

    public Portal PlacePortal(PortalGun portalGun, bool primary, out Portal replacedPortal)
    {
        // Replace any existing portal
        if (active)
        {
            replacedPortal = portal;
            RemovePortal();
        }
        else
        {
            replacedPortal = null;
        }
        
        this.primary = primary;
        ActivatePortal(portalGun);

        return portal;
    }

    public void RemovePortal()
    {
        // TODO all this portal enabling/disabling WHERE A PORTAL STILL EXISTS, JUST UNPAIRED WITH ANOTHER should happen inside the portal class
        if (portal.LinkedPortal != null)
        {
            portal.LinkedPortal.LinkedPortal = null;
            portal.LinkedPortal = null;
        }
        DeactivatePortal();
    }

    void ActivatePortal(PortalGun portalGun)
    {
        active = true;
        CameraPortalRendering.AddPortal(portal);
        // TODO This may need to be setting materials, not colors, so we can have different patterns for different players for increased accessibility
        portal.PortalBorder.materials[0].color = primary ? portalGun.primaryColor : portalGun.secondaryColor;
        portal.PortalBorder.materials[0].EnableKeyword("_EMISSION");
        portal.PortalBorder.materials[0].SetColor("_EmissionColor",
                primary ? portalGun.primaryColor : portalGun.secondaryColor);

        // We need one portal to be flipped around - just make it the secondary one
        portal.transform.localRotation = Quaternion.Euler(0, !primary ? 180 : 0, 0);
        
        portal.gameObject.SetActive(true);
        portal.PortalBorder.gameObject.SetActive(true);
        panel.SetActive(false);
    }
    
    void DeactivatePortal()
    {
        active = false;
        CameraPortalRendering.RemovePortal(portal);
        // Not sure if these really do anything, since it should be gone, might just be insurance
        portal.PortalBorder.materials[0].color = Color.white;
        portal.PortalBorder.materials[0].SetColor("_EmissionColor", Color.black);
        
        // Reset rotation in case portal was flipped around as a secondary portal
        portal.transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        portal.gameObject.SetActive(false);
        portal.PortalBorder.gameObject.SetActive(false);
        panel.SetActive(true);
    }

    public bool isActive()
    {
        return active;
    }

    public Portal GetPortal()
    {
        return portal;
    }
}
