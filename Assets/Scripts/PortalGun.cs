using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalGun : MonoBehaviour
{

    [SerializeField] float range = 100f;
    [SerializeField] float beamSpeed = 100f;
    
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
        if (Physics.Raycast(orientation.position, orientation.forward, out hit, range, collisionMask))
        {
            IPortalable portalable = hit.collider.GetComponentInParent<IPortalable>();
            if (portalable == null) return;

            Portal replacedPortal;
            Portal placedPortal = portalable.PlacePortal(primary, out replacedPortal);

            if (replacedPortal)
            {
                // Don't disable the other portal I think the IPortalable should handle that
                // TODO solve portal lifetime. Basically we have a portalable, which could have dynamic or static portal lifetimes
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
            
            if (primary)
            {
                if (primaryPortal != null)
                {
                    primaryPortal.RemovePortal();
                    primaryPortal = null;
                }
                primaryPortal = portalable;
            }
            else
            {
                if (secondaryPortal != null)
                {
                    secondaryPortal.RemovePortal();
                    secondaryPortal = null;
                }
                secondaryPortal = portalable;
            }
            
            primaryIndicator.SetActive(primaryPortal != null);
            secondaryIndicator.SetActive(secondaryPortal != null);

            if (primaryPortal != null && secondaryPortal != null)
            {
                primaryPortal.GetPortal().linkedPortal = secondaryPortal.GetPortal();
                secondaryPortal.GetPortal().linkedPortal = primaryPortal.GetPortal();
            }
        }
    }

}
