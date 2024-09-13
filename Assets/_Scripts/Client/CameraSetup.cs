using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{

    public class CameraSetup : MonoBehaviour
    {
        [Tooltip("Object for the camera within the child of this transform.")] [SerializeField]
        Transform cameraObject;

        public void OnEnable()
        {
            // Every client object has a camera -> set it active only in the local player
            // We do this instead of reparenting one world camera so we can use other player's views in the future
            Debug.Log("Enabling camera object.");
            cameraObject.gameObject.SetActive(true);
        }
    }

}