using System;
using System.Collections;
using System.Collections.Generic;
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

    IPortalable primaryPortal;
    IPortalable secondaryPortal;

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
        
        primaryPortal?.RemovePortal();
        primaryPortal = null;
        secondaryPortal?.RemovePortal();
        secondaryPortal = null;
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
        
        IPortalable portalable = hit.collider.GetComponentInParent<IPortalable>();
        if (portalable == null) return;

        Portal replacedPortal;
        Portal placedPortal = portalable.PlacePortal(this, primary, out replacedPortal);

        if (replacedPortal)
        {
            // Don't disable the other portal I think the IPortalable should handle that
            // TODO solve portal lifetime. Basically we have a portalable, which could have dynamic or static portal lifetimes
            //       Even more basically, we have multiple reasons why a portal could be removed, and we need to handle updating the gun state for them all
            // But we also need some conceptualization of if one portal replaces another
            // This is probably best done with links between from the portal to the gun,
            // but nevertheless it needs planned and solved once and properly
            if (primaryPortal != null && replacedPortal == primaryPortal.GetPortal())
            {
                primaryPortal = null;
            } 
            else if (secondaryPortal != null && replacedPortal == secondaryPortal.GetPortal())
            {
                secondaryPortal = null;
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
            primaryPortal?.RemovePortal();
            primaryPortal = portalable;
        }
        else
        {
            secondaryPortal?.RemovePortal();
            secondaryPortal = portalable;
        }
    }

    void UpdateVisualIndicators()
    {
        primaryIndicator.SetActive(primaryPortal != null);
        secondaryIndicator.SetActive(secondaryPortal != null);
    }

    void AttemptToLinkPortals()
    {
        if (primaryPortal == null || secondaryPortal == null) return;
        
        primaryPortal.GetPortal().LinkedPortal = secondaryPortal.GetPortal();
        secondaryPortal.GetPortal().LinkedPortal = primaryPortal.GetPortal();
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
        
        ShootPortal(primary, shotOrigin, shotDirection);
    }

    // TODO do this with an event maybe?
    public void PortalWasDeactivated(bool primary)
    {
        if (primary)
        {
            primaryPortal = null;
        }
        else
        {
            secondaryPortal = null;
        }
    }

    public Portal GetPortal(bool primary)
    {
        return primary ? primaryPortal?.GetPortal() : secondaryPortal?.GetPortal();
    }
}
