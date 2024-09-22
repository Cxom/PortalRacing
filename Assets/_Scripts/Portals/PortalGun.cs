using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PortalGun : NetworkBehaviour
{
    [Header("Portal Gun")]
    [SerializeField] float range = 100f;
    [SerializeField] float beamSpeed = 100f;
    [SerializeField] public Color primaryColor;
    [SerializeField] public Color secondaryColor;
    // public Color SecondaryColor { get; private set; }
    
    [SerializeField] Transform orientation;
    [SerializeField] Transform shootOrigin;
    [SerializeField] LayerMask collisionMask;

    [Header("Graphics - UI")]
    [SerializeField] GameObject primaryIndicator;
    [SerializeField] GameObject secondaryIndicator;
    
    [Header("Graphics - Scene")]
    [SerializeField] Material portalBeamMaterial;
    
    // TODO lots of portalable null checks checking the unity lifetime - useful to have an active state instead? or just microoptimization?

    internal IPortalable primaryPortalable { get; private set; }

    internal IPortalable secondaryPortalable { get; private set; }


    // When a player shoots a portal, we need to convey to other players the following things:
    //  - the beam actually shot
    //  - the newly placed portal
    //  - if a portal was replaced, that it was removed

    //  - potential: an indicator on the player pawn/portal gun graphic object that shot the portal  


    public override void OnStartServer()
    {
        base.OnStartServer();

        ServerManager.Objects.OnPreDestroyClientObjects += RemovePortalsOnDisconnect;
    }
    
    public override void OnStopServer()
    {
        base.OnStopServer();

        ServerManager.Objects.OnPreDestroyClientObjects -= RemovePortalsOnDisconnect;
    }
    
    void RemovePortalsOnDisconnect(NetworkConnection conn)
    {
        if (conn != Owner) return;
        
        if (primaryPortalable != null)
        {
            primaryPortalable.RemovePortal();
            primaryPortalable = null;
        }
        
        if (secondaryPortalable != null)
        {
            secondaryPortalable.RemovePortal();
            secondaryPortalable = null;
        }
    }

    public void CheckShootPortal()
    {
        if (!IsOwner)
        {
            Debug.Log("Expected check shoot portal to only be called by the local owner!");
            return;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            bool primary = true;
            ShootPortal(primary, orientation.position, orientation.forward);
            CmdShootPortal(primary, orientation.position, orientation.forward);
        }

        if (Input.GetButtonDown("Fire2"))
        {
            bool primary = false;
            ShootPortal(primary, orientation.position, orientation.forward);
            CmdShootPortal(primary, orientation.position, orientation.forward);
        }
    }

    void ShootPortal(bool primary, Vector3 shotOrigin, Vector3 shotDirection)
    {
        RaycastHit hit;
        if (!Physics.Raycast(shotOrigin, shotDirection, out hit, range, collisionMask)) return;
        // we have a hit
        
        // TODO the actual beam
        
        IPortalable portalable = hit.collider.GetComponentInParent<IPortalable>();
        if (portalable == null) return;

        CreatePortalAfterHit(primary, portalable);
    }

    void CreatePortalAfterHit(bool primary, IPortalable portalable)
    {
        Portal replacedPortal;
        Portal placedPortal = portalable.PlacePortal(this, primary, out replacedPortal);

        if (replacedPortal != null)
        {
            Debug.Log("Portal " + replacedPortal.gameObject.GetFullPathName() + " was replaced!");
            // Don't disable the other portal I think the IPortalable should handle that
            // TODO solve portal lifetime. Basically we have a portalable, which could have dynamic or static portal lifetimes
            //       Even more basically, we have multiple reasons why a portal could be removed, and we need to handle updating the gun state for them all
            // But we also need some conceptualization of if one portal replaces another
            // This is probably best done with links between from the portal to the gun,
            // but nevertheless it needs planned and solved once and properly
            if (primaryPortalable != null && replacedPortal == primaryPortalable.GetPortal())
            {
                Debug.Log("Removing primary portal because it was replaced!");
                primaryPortalable = null;
            } 
            else if (secondaryPortalable != null && replacedPortal == secondaryPortalable.GetPortal())
            {
                Debug.Log("Removing secondary portal because it was replaced!");
                secondaryPortalable = null;
            }
        }
        
        // Update portal references
        UpdatePortalReferences(primary, portalable);
            
        // Update visual indicators
        UpdateVisualIndicators();

        // Link portals
        AttemptToLinkPortals();
    }

    void UpdatePortalReferences(bool primary, IPortalable portalable)
    {
        if (primary)
        {
            Debug.Log($"primary portalable is the same as portalable: {primaryPortalable == portalable}");
            if (primaryPortalable != null && primaryPortalable != portalable)
            {
                Debug.Log("Removing primary portal because it was shot elsewhere!");
                primaryPortalable.RemovePortal();
            }
            else
            {
                Debug.Log("It seems like there was not a primary portalable to remove!");
            }

            primaryPortalable = portalable;
        }
        else
        {
            Debug.Log($"secondary portalable is the same as portalable: {secondaryPortalable == portalable}");
            if (secondaryPortalable != null && secondaryPortalable != portalable)
            {
                Debug.Log("Removing secondary portal because it was shot elsewhere!");
                secondaryPortalable.RemovePortal();
            }
            else
            {
                Debug.Log("It seems like there was not a secondary portalable to remove!");
            }

            secondaryPortalable = portalable;
        }
    }

    void UpdateVisualIndicators()
    {
        primaryIndicator.SetActive(primaryPortalable != null);
        secondaryIndicator.SetActive(secondaryPortalable != null);
    }

    void AttemptToLinkPortals()
    {
        if (primaryPortalable == null || secondaryPortalable == null) return;
        
        primaryPortalable.GetPortal().LinkedPortal = secondaryPortalable.GetPortal();
        secondaryPortalable.GetPortal().LinkedPortal = primaryPortalable.GetPortal();
    }

    [ServerRpc]
    void CmdShootPortal(bool primary, Vector3 shotOrigin, Vector3 shotDirection)
    {
        //Only fire again on server if not client host/owner.
        if (!base.IsOwner)
        {
            // TODO validity checks
            // I think we can probably skip distance checks - we always need to know about portal updates
            // But maybe if we want we can TODO optimize out beam firing being sent to clients from other players super far away
            
            ShootPortal(primary, shotOrigin, shotDirection);
        }
        ObserversShootPortal(primary, shotOrigin, shotDirection);
    }

    [ObserversRpc]
    void ObserversShootPortal(bool primary, Vector3 shotOrigin, Vector3 shotDirection)
    {
        if (IsOwner || IsServerStarted) return;
        // Any validity checks here?

        Debug.Log("Doing observers shoot portal for other client's shot!");
        
        ShootPortal(primary, shotOrigin, shotDirection);
    }

    // TODO do this with an event maybe?
    public void PortalWasDeactivated(bool primary)
    {
        if (primary)
        {
            primaryPortalable = null;
        }
        else
        {
            secondaryPortalable = null;
        }
    }

    public Portal GetPortal(bool primary)
    {
        if (primary)
            return primaryPortalable != null ? primaryPortalable.GetPortal() : null;
        else
            return secondaryPortalable != null ? secondaryPortalable.GetPortal() : null;
    }

    [TargetRpc]
    internal void InitializePortalsFromServer(NetworkConnection conn, IPortalable primaryPortalable, IPortalable secondaryPortalable)
    {
        Debug.Log("InitializePortalsFromServer!!!");

        if (IsOwner)
        {
            Debug.Log("Assuming both portals are null on owner: " + (primaryPortalable == null) + " " +
                      (secondaryPortalable == null));
        }
        
        if (primaryPortalable != null)
        {
            Debug.Log($"Recasting primary portal on [{primaryPortalable.gameObject.GetFullPathName()}]");
            CreatePortalAfterHit(true, primaryPortalable);
        }

        if (secondaryPortalable != null)
        {
            Debug.Log($"Recasting secondary portal on [{secondaryPortalable.gameObject.GetFullPathName()}]");
            CreatePortalAfterHit(false, secondaryPortalable);
        }
        // Portals will auto be linked if both are non-null by the second CreatePortalAfterHit call
    }    
}
