using System;
using Mirror;
using UnityEngine;

namespace Client
{

    public class ClientInstance : NetworkBehaviour
    {

        public static ClientInstance Instance;

        // public static Action<GameObject> OnOwnerCharacterSpawned;

        [Tooltip("Prefab for the player.")] [SerializeField]
        NetworkIdentity playerPrefab;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Instance = this;
            CmdRequestSpawn();
        }

        /// <summary>
        /// Request spawning a player character.
        /// </summary>
        [Command]
        void CmdRequestSpawn()
        {
            NetworkSpawnPlayer();
        }

        /// <summary>
        /// Spawns a character for the player.
        /// </summary>
        [Server]
        void NetworkSpawnPlayer()
        {
            GameObject go = Instantiate(playerPrefab.gameObject, transform.position, transform.rotation);
            // Give authority to client
            NetworkServer.Spawn(go, base.connectionToClient);
        }

        public static ClientInstance ReturnClientInstance(NetworkConnection conn = null)
        {
            /* When trying to access as the server connection,
             * conn will always contain a value, but if it's a client then it will be null
             */
            if (NetworkServer.active && conn != null)
            {
                NetworkIdentity localPlayer;
                if (PortalRacingNetworkManager.LocalPlayers.TryGetValue(conn, out localPlayer))
                {
                    return localPlayer.GetComponent<ClientInstance>();
                }
                else
                {
                    return null;
                }
            }
            // If the server is not active or the connection is null, then it is a client. 
            else
            {
                return Instance;
            }
        }
    }

}
