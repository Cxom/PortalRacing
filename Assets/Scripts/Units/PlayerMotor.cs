using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Units
{

    /// <summary>
    /// A dummy super simple movement class for testing/implementing multiplayer code
    /// </summary>
    public class PlayerMotor : NetworkBehaviour
    {
        // [SerializeField] MonoBehaviour playerInputSystem;

        public Color color;
        
        Rigidbody _rigidbody;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // playerInputSystem.enabled = base.hasAuthority;
        }

        void FixedUpdate()
        {
            if (base.hasAuthority)
            {
                Move();
            }
        }

        void Move()
        {
            float forward = Input.GetAxisRaw("Vertical");
            float rotation = Input.GetAxisRaw("Horizontal");

            Vector3 next = new Vector3(0f, 0f, forward * Time.deltaTime * 3000);
            next += Physics.gravity * Time.deltaTime;

            transform.Rotate(new Vector3(0f, rotation * Time.deltaTime * 90, 0f));
            _rigidbody.AddForce(transform.TransformDirection(next));
        }
    }
    
}