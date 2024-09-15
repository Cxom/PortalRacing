using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;

namespace Units
{
    // TODO - use one main camera - not one per player

    /// <summary>
    /// A dummy super simple movement class for testing/implementing multiplayer code
    /// </summary>
    public class PlayerBasicRigidbodyMotor : NetworkBehaviour
    {
        // [SerializeField] MonoBehaviour playerInputSystem;
        [SerializeField] float moveForce = 3000f;
        
        [SerializeField] float _yawRate = 300f;
        [SerializeField] float _pitchRate = 300f;
        [SerializeField] Camera camera;
        
        float pitch;
        
        Rigidbody _rigidbody;
        float forward;
        float lateral;

        bool paused = false;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
            {
                // playerInputSystem.enabled = base.hasAuthority;
                camera.enabled = true;
                camera.GetComponent<AudioListener>().enabled = true;
            }
            else
            {
                gameObject.GetComponent<PlayerBasicRigidbodyMotor>().enabled = false;
            }
        }

        void Start()
        {
            LockCursor();
        }

        static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.P))
            {
                if (!paused)
                {
                    Debug.Log("Unlocking cursor");
                    UnlockCursor();
                    paused = true;
                }   
                else
                {
                    Debug.Log("Relocking cursor");
                    LockCursor();
                    paused = false;
                }
            }
            
            if (paused) return;
            
            forward = Input.GetAxisRaw("Vertical");
            lateral = Input.GetAxisRaw("Horizontal");
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
            if (paused) return;
            
            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * _yawRate;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * _pitchRate;
        
            // Find the current rotation
            Vector3 rotation = transform.eulerAngles;
            float yaw = rotation.y + mouseX;
        
            // Rotate and clamp angles
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -90f, 90f);
        
            // Perform rotations
            camera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.localRotation = Quaternion.Euler(0, yaw, 0);
        }
        
        void OnGUI()
        {
            GUI.Label(new Rect(20, 25, 200, 20), $"Velocity: {_rigidbody.velocity.magnitude}");
        }
    }
    
}