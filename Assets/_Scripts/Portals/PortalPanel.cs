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
        active = true;
        portal.Activate(this.primary, portalGun);
        panel.SetActive(false);

        return portal;
    }

    public void RemovePortal()
    {
        active = false;
        portal.Deactivate();
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
