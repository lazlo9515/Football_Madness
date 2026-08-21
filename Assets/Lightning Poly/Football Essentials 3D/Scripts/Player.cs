namespace LightningPoly.FootballEssentials3D
{
    using UnityEngine;
    using System.Collections;
    using Photon.Pun;

    public class Player : MonoBehaviour
    {
        private PhotonView view;
        public float moveSpeed = 5f;
        public float acceleration = 10f;
        public float deceleration = 5f;
        public float jumpForce = 7f;
        public float rotationSpeed = 10f;
        public float kickForce = 10f;
        public float kickUpwardForce = 2f;
        public GameObject respawnUI; // Drag a UI Panel here with a Text component
        public TMPro.TextMeshProUGUI countdownText;
        public Transform respawnPoint;
        private bool isDead = false;

        private Rigidbody rb;
        private bool isGrounded;
        private Transform cameraTransform;
        private Vector3 moveDirection;

        public GameObject[] decorations, eyes, mouths, hairs, all;

        private void OnGUI()
        {
            if (GUILayout.Button("Change Character Appearance"))
            {
                ChangeCloth();
            }
        }
        [ContextMenu(nameof(ChangeCloth))]
        public void ChangeCloth()
        {
            foreach (var item in all)
            {
                item.SetActive(false);
            }
            decorations[Random.Range(0, decorations.Length)].SetActive(true);
            eyes[Random.Range(0, eyes.Length)].SetActive(true);
            mouths[Random.Range(0, mouths.Length)].SetActive(true);
            hairs[Random.Range(0, hairs.Length)].SetActive(true);
        }


        void Start()
        {
            view = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody>();
            cameraTransform = Camera.main.transform;
        }

        // Inside Player.cs Update()
        // Inside Player.cs -> Update()
        void Update()
        {
            // Find the GameManager and check the state
            if (FindAnyObjectByType<PhotonGameManager>().currentGameState != PhotonGameManager.GameState.Soccer)
            {
                return; // Lock movement during gambling
            }

            if (!view.IsMine)
                return;

            ProcessInput();
            Jump();
            RotateCharacter();

            // ==========================================
            // CHANGED: Now uses KeyCode.J to shoot!
            // ==========================================
            if (Input.GetKeyDown(KeyCode.J))
            {
                // Look for the OneShotGun script in the player's children
                OneShotGun gun = GetComponentInChildren<OneShotGun>();

                if (gun != null)
                {
                    gun.Fire(this); // Fire the gun!
                }
            }
        }

        void FixedUpdate()
        {
            if (!view.IsMine)
                return;

            Move();
        }

        void ProcessInput()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * moveZ + right * moveX).normalized;
        }

        // Inside Player.cs -> Move()
        void Move()
        {
            Vector3 targetVelocity = moveDirection * moveSpeed;

            // Tiny tweak: If moveSpeed is higher than default (5), 
            // maybe increase acceleration so it feels "snappy"
            float currentAccel = (moveSpeed > 5.1f) ? acceleration * 1.5f : acceleration;

            if (!isGrounded)
            {
                targetVelocity.y = rb.linearVelocity.y;
            }

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, (isGrounded ? currentAccel : deceleration) * Time.fixedDeltaTime);
        }

        void RotateCharacter()
        {
            if (moveDirection.magnitude > 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Ground ground))
            {
                isGrounded = true;
            }
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Ground ground))
            {
                isGrounded = false;
            }
        }
        public void Die()
        {
            if (isDead) return;
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            isDead = true;
            moveSpeed = 0; // Freeze the player

            // 1. Show the UI
            if (respawnUI != null) respawnUI.SetActive(true);

            // 2. Countdown Loop
            for (int i = 3; i > 0; i--)
            {
                if (countdownText != null) countdownText.text = "Respawning in: " + i;
                yield return new WaitForSeconds(1f);
            }

            // 3. Teleport and Reset
            transform.position = respawnPoint.position;
            if (respawnUI != null) respawnUI.SetActive(false);

            moveSpeed = 5f; // Reset to your original speed
            isDead = false;
            Debug.Log("Player Respawned!");
        }
    }
}

