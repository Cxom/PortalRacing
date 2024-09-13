using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Units
{
    

    /// <summary>
    /// A dummy super simple movement class for testing/implementing multiplayer code
    /// </summary>
    public class PlayerMotor : MonoBehaviour
    {
        // [SerializeField] MonoBehaviour playerInputSystem;
        [SerializeField] float moveForce = 3000f;
        
        [SerializeField] float _yawRate = 300f;
        [SerializeField] float _pitchRate = 300f;
        [SerializeField] Transform cameraOrientation;
        
        float pitch;
        
        Rigidbody _rigidbody;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        // public override void OnStartClient()
        // {
        //     base.OnStartClient();
        //     if (base.hasAuthority)
        //     {
        //         // playerInputSystem.enabled = base.hasAuthority;
        //         CaptureCursor();
        //     }
        // }

        void OnEnable()
        {
            CaptureCursor();
        }

        static void CaptureCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void FixedUpdate()
        {
            // if (base.hasAuthority)
            // {
                Move();
            // }
        }

        void Move()
        {
            float forward = Input.GetAxisRaw("Vertical");
            float lateral = Input.GetAxisRaw("Horizontal");
            
            // Assert.AreEqual(Time.fixedDeltaTime, Time.deltaTime, "This should be a fixed update method.");

            Vector3 next = new Vector3(lateral, 0f, forward) * (Time.deltaTime * moveForce);
            next += Physics.gravity * Time.deltaTime;

            _rigidbody.AddForce(transform.TransformDirection(next));
        }

        void LateUpdate()
        {
            // if (base.hasAuthority)
            // {
                MoveCamera();
            // }
        }

        void MoveCamera()
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * _yawRate;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * _pitchRate;
        
            // Find the current rotation
            Vector3 rotation = transform.eulerAngles;
            float yaw = rotation.y + mouseX;
        
            // Rotate and clamp angles
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -90f, 90f);
        
            // Perform rotations
            cameraOrientation.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.localRotation = Quaternion.Euler(0, yaw, 0);
        }
        
        void OnGUI()
        {
            GUI.Label(new Rect(20, 25, 200, 20), $"Velocity: {_rigidbody.velocity.magnitude}");
        }
    }
    
}