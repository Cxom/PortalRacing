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
        replacedPortal = null;
        if (active)
        {
            replacedPortal = portal;
            RemovePortal();
        }
        
        active = true;
        this.primary = primary;
        UpdatePortalState(portalGun);

        return portal;
    }

    public void RemovePortal()
    {
        active = false;
        // TODO all this portal enabling/disabling WHERE A PORTAL STILL EXISTS, JUST UNPAIRED WITH ANOTHER should happen inside the portal class
        if (portal.LinkedPortal)
        {
            portal.LinkedPortal.LinkedPortal = null;
        }
        portal.LinkedPortal = null;
        UpdatePortalState(null);
    }

    void UpdatePortalState(PortalGun portalGun)
    {
        if (active)
        {
            CameraPortalRendering.AddPortal(portal);
            // TODO This may need to be setting materials, not colors, so we can have different patterns for different players for increased accessibility
            portal.PortalBorder.materials[0].color = primary ? portalGun.primaryColor : portalGun.secondaryColor;
            portal.PortalBorder.materials[0].EnableKeyword("_EMISSION");
            portal.PortalBorder.materials[0].SetColor("_EmissionColor", primary ? portalGun.primaryColor : portalGun.secondaryColor);
        }
        else
        {
            CameraPortalRendering.RemovePortal(portal);
            portal.PortalBorder.materials[0].color = Color.white;
            portal.PortalBorder.materials[0].SetColor("_EmissionColor", Color.black);
        }
        portal.transform.localRotation = Quaternion.Euler(0, active && !primary ? 180 : 0, 0);
        portal.gameObject.SetActive(active);
        portal.PortalBorder.gameObject.SetActive(active);
        panel.SetActive(!active);
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
