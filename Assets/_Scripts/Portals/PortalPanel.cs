using UnityEngine;
using UnityEngine.Assertions;

public class PortalPanel : MonoBehaviour, IPortalable
{
    bool active;
    bool primary;
    [SerializeField] Portal portalPrefab;
    [SerializeField] GameObject panel;

    Portal portalInstance;
    
    public Portal PlacePortal(PortalGun portalGun, bool primary, out Portal replacedPortal)
    {
        // Replace any existing portal
        if (active)
        {
            replacedPortal = portalInstance;
            Debug.Log("Replacing portal");
            RemovePortal();
        }
        else
        {
            replacedPortal = null;
        }
        
        this.primary = primary;
        active = true;
        
        portalInstance = Instantiate(portalPrefab, transform);
        portalInstance.Activate(this.primary, portalGun);
        panel.SetActive(false);

        return portalInstance;
    }

    public void RemovePortal()
    {
        active = false;
        portalInstance.Deactivate();
        Destroy(portalInstance);
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
        Assert.IsTrue((portalInstance != null) == active, "Portal instance existence should match active state");
        return portalInstance;
    }
}
