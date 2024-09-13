using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPortalable
{

    Portal PlacePortal(PortalGun portalGun, bool primary, out Portal replacedPortal);
    void RemovePortal();

    bool isActive();
    Portal GetPortal();

}
