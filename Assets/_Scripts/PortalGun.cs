using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGun : MonoBehaviour
{
    

    [Header("Portal Gun")]
    [SerializeField] float range = 100f;
    [SerializeField] float beamSpeed = 100f;
    [SerializeField] public Color primaryColor;
    [SerializeField] public Color secondaryColor;
    public Color SecondaryColor { get; private set; }
    
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

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            ShootPortal(true);
        }

        if (Input.GetButtonDown("Fire2"))
        {
            ShootPortal(false);
        }
    }

    void ShootPortal(bool primary)
    {
        RaycastHit hit;
        if (!Physics.Raycast(orientation.position, orientation.forward, out hit, range, collisionMask)) return;
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
}
