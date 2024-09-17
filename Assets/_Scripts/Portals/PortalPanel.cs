using UnityEngine;

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
            Debug.Log("Replacing portal");
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
        // Do to the design of original portal panels, there is always a portal object, just it's disabled sometimes
        // To emulate the api other portalables should have, we return null if the portal is not active
        return active ? portal : null;
    }
}
