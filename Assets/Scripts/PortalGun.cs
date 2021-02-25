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

    bool primaryActive = false;
    bool secondaryActive = false;
    
    [Header("Graphics - UI")]
    [SerializeField] GameObject primaryIndicator;
    [SerializeField] GameObject secondaryIndicator;
    
    [Header("Graphics - Scene")]
    [SerializeField] GameObject hitNormalGraphic;
    [SerializeField] float hitNormalGraphicDisplaySeconds = 2;
    [SerializeField] Material portalBeamMaterial;


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
            Debug.Log(hit.transform.name);
            if (primary)
            {
                primaryActive = !primaryActive;
                primaryIndicator.SetActive(primaryActive);
            }
            else
            {
                secondaryActive = !secondaryActive;
                secondaryIndicator.SetActive(secondaryActive);
            }

            StartCoroutine(DisplayHitGraphic(hit));
        }
    }

    IEnumerator DisplayHitGraphic(RaycastHit hit)
    {
        var hitGraphic = Instantiate(hitNormalGraphic, hit.point + (hit.normal * 0.05f), Quaternion.LookRotation(-hit.normal));
        yield return new WaitForSeconds(hitNormalGraphicDisplaySeconds);
        Destroy(hitGraphic);
    }
    
}
