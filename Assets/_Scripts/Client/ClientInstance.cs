using System;
using Units;
using UnityEngine;

namespace Client
{

    public class ClientInstance : MonoBehaviour
    {
        public static ClientInstance Instance;

        // public static Action<GameObject> OnOwnerCharacterSpawned;

        [Tooltip("Prefab for the player.")] [SerializeField]
        PlayerBasicRigidbodyMotor playerPrefab;
        
        GameObject playerInstance;

        void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(playerInstance);
            }
            Debug.Log("Instantiating player.");
            playerInstance = Instantiate(playerPrefab.gameObject, transform.position, transform.rotation);
        }

    }

}
