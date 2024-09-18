using UnityEngine;
using UnityEngine.Assertions;

public class PortalPanel : MonoBehaviour, IPortalable
{
    
    /*
     * It seems like there are two general approaches to persisting world state that needs to be sent to
     * newly connecting clients.
     *  - One is to have a manager that keeps track of all the state and sends it to new clients.
     *  - Two is to have the state be part of the objects themselves, and make judicious use of network objects
     *      - one can be a little smarter with this approach by turning networking on and off for default objects
     *
     * The first approach seems easier, but does require manual syncing of state to new clients.
     * I think I'd like players and they're associated scripts to probably all be auto-synced,
     * but world state I might track myself and send to new clients.
     */
    
    
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
        // InstanceFinder.ServerManager.Spawn(portalInstance.gameObject);
        portalInstance.Activate(this.primary, portalGun);
        panel.SetActive(false);

        return portalInstance;
    }

    public void RemovePortal()
    {
        active = false;
        portalInstance.Deactivate();
        Destroy(portalInstance);
        // InstanceFinder.ServerManager.Despawn(portalInstance.gameObject);
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
