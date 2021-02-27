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

    public Portal PlacePortal(bool primary, out Portal replacedPortal)
    {
        replacedPortal = null;
        if (active)
        {
            replacedPortal = portal;
            RemovePortal();
        }
        
        active = true;
        this.primary = primary;
        UpdatePortalState();

        return portal;
    }

    public void RemovePortal()
    {
        active = false;
        // TODO all this portal enabling/disabling WHERE A PORTAL STILL EXISTS, JUST UNPAIRED WITH ANOTHER should happen inside the portal class
        if (portal.linkedPortal)
        {
            portal.linkedPortal.linkedPortal = null;
        }
        portal.linkedPortal = null;
        UpdatePortalState();
    }

    void UpdatePortalState()
    {
        if (active)
        {
            CameraPortalRendering.AddPortal(portal);
        }
        else
        {
            CameraPortalRendering.RemovePortal(portal);
        }
        portal.transform.localRotation = Quaternion.Euler(0, active && !primary ? 180 : 0, 0);
        portal.gameObject.SetActive(active);
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
