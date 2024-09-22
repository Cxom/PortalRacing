using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public override void OnSpawnServer(NetworkConnection connection)
    {
        // TODO try to refactor this into the onspawnserver of the portalgun
        base.OnSpawnServer(connection);
        SendServerPortalsToTarget(connection);
    }

    [Server]
    void SendServerPortalsToTarget(NetworkConnection conn)
    {
        PortalGun[] allGuns = FindObjectsOfType<PortalGun>();
        foreach (PortalGun portalGun in allGuns)
        {
            portalGun.InitializePortalsFromServer(conn, portalGun.primaryPortalable, portalGun.secondaryPortalable);
        }
    }
    
    /*
     * TODO THINK ABOUT HOW THE RESENDING OF STATE APPLIES TO ALL OUR OTHER SYSTEMS IN-GAME (GRAPPLING, PINGING, ANIMATED PANELS, the long black list on red)
     */
}
