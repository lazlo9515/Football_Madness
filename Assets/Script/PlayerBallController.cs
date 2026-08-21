namespace LightningPoly.FootballEssentials3D
{
    using UnityEngine;
    using Photon.Pun;
    using System.Collections; // ADDED: Required for the Coroutine Timer!

    public class PlayerBallController : MonoBehaviourPun
    {
        [Header("Steal Mechanics")]
        public KeyCode stealButton = KeyCode.L;
        public bool canSteal = true;
        private bool isHoldingSteal = false;

        [Header("References")]
        public Transform holdPosition;

        [Header("Settings")]
        public float kickForce = 5f;
        public float grabCooldown = 0.5f;

        // 1. ADDED: The variable to hold your sound file
        [Tooltip("Drag your kick sound effect here")]
        public AudioClip kickSound;
        private Ball heldBall;
        private float currentGrabCooldown = 0f;
        private float currentStealCooldown = 0f;

        void Update()
        {
            if (photonView.IsMine)
            {
                // Check if we are currently holding down the Steal button
                isHoldingSteal = Input.GetKey(stealButton);
            }
            if (!photonView.IsMine) return;

            // Tick down both cooldowns
            if (currentGrabCooldown > 0f) currentGrabCooldown -= Time.deltaTime;
            if (currentStealCooldown > 0f) currentStealCooldown -= Time.deltaTime;

            // Keep the ball attached to our feet if we are holding it
            if (heldBall != null)
            {
                heldBall.transform.position = holdPosition.position;
                heldBall.transform.rotation = holdPosition.rotation;
            }

            // Handle Kicking
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (heldBall != null)
                {
                    KickHeldBall();
                    
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!photonView.IsMine) return;

            // =========================================================
            // SCENARIO 1: HITTING A LOOSE BALL ON THE GROUND
            // =========================================================
            Ball looseBall = collision.gameObject.GetComponent<Ball>();
            if (looseBall != null)
            {
                // Pick it up normally if we don't already have a ball, and our grab cooldown is finished!
                if (heldBall == null && currentGrabCooldown <= 0f)
                {
                    GrabBallInterception(looseBall);
                    return; // We grabbed it, so stop running the rest of this code!
                }
            }

            // =========================================================
            // SCENARIO 2: HITTING AN ENEMY PLAYER (STEAL MECHANIC)
            // =========================================================
            PlayerBallController enemyPlayer = collision.gameObject.GetComponent<PlayerBallController>();
            if (enemyPlayer != null && enemyPlayer != this) // Don't try to steal from ourselves!
            {
                // ONLY steal if we hold 'L', we are allowed to steal, and THEY have the ball
                if (isHoldingSteal && canSteal && enemyPlayer.heldBall != null)
                {
                    Debug.Log("[STEAL] I successfully stole the ball!");

                    // 1. Tell the enemy they got robbed and apply the penalty!
                    enemyPlayer.photonView.RPC("RPC_SufferStealPenalty", enemyPlayer.photonView.Owner);

                    // 2. Take their ball!
                    GrabBallInterception(enemyPlayer.heldBall);
                }
            }
        }

        // Created a small helper function to keep the code clean
        void GrabBallInterception(Ball target)
        {
            PhotonView ballPv = target.GetComponent<PhotonView>();
            if (ballPv != null)
            {
                // Force the RPC to trigger
                photonView.RPC("RPC_StealBall", RpcTarget.All, ballPv.ViewID);
                Debug.Log("[SOCCER] Caught the ball!");
            }
        }

        [PunRPC]
        void RPC_StealBall(int ballViewID)
        {
            PhotonView ballPv = PhotonView.Find(ballViewID);
            if (ballPv == null) return;

            Ball targetBall = ballPv.GetComponent<Ball>();
            Rigidbody ballRb = targetBall.GetComponent<Rigidbody>();

            // ==========================================================
            // 1. FORCE ABSOLUTELY EVERYONE TO DROP THE BALL
            // ==========================================================
            PlayerBallController[] allPlayers = FindObjectsByType<PlayerBallController>(FindObjectsSortMode.None);
            foreach (PlayerBallController player in allPlayers)
            {
                if (player.heldBall == targetBall)
                {
                    player.heldBall = null; // Drop it!

                    if (player.photonView.IsMine && player != this)
                    {
                        player.currentStealCooldown = 1.0f;
                        Debug.Log("[NETWORK] Forced to drop the ball by a Magnet or Steal!");
                    }
                }
            }

            // 2. STOP THE BALL PHYSICS DEAD IN ITS TRACKS
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.isKinematic = true;
            targetBall.GetComponent<Collider>().enabled = false;

            // 3. GIVE THE BALL TO THE THIEF
            this.heldBall = targetBall;

            // 4. FORCE THE NETWORK TAKEOVER
            if (this.photonView.IsMine)
            {
                if (!ballPv.IsMine)
                {
                    ballPv.TransferOwnership(PhotonNetwork.LocalPlayer);
                    Debug.Log("[NETWORK] I am the new owner of the ball!");
                }
            }
        }

        private void KickHeldBall()
        {
            Ball ballToKick = heldBall;
            currentGrabCooldown = grabCooldown;

            PhotonView ballPv = ballToKick.GetComponent<PhotonView>();
            photonView.RPC("RPC_ReleaseBall", RpcTarget.All, ballPv.ViewID);

            Vector3 kickDirection = transform.forward + (Vector3.up * 0.2f);
            ballToKick.KickBall(kickDirection.normalized, kickForce);

            // Trigger the sound and verify in Console
            if (GlobalAudioManager.instance != null && kickSound != null)
            {
                GlobalAudioManager.instance.PlaySFX(kickSound);
                Debug.Log("SUCCESS: Kick sound was triggered!"); 
            }
            else
            {
                Debug.LogWarning("FAILED: Missing GlobalAudioManager or Kick Sound clip!");
            }
        }
        
        [PunRPC]
        void RPC_ReleaseBall(int ballViewID)
        {
            PhotonView ballPv = PhotonView.Find(ballViewID);
            if (ballPv == null) return;
            Ball targetBall = ballPv.GetComponent<Ball>();

            if (heldBall == targetBall)
            {
                heldBall = null;
            }

            targetBall.GetComponent<Rigidbody>().isKinematic = false;
            targetBall.GetComponent<Collider>().enabled = true;
        }

        public void ForceReleaseBall()
        {
            if (heldBall != null)
            {
                PhotonView ballPv = heldBall.GetComponent<PhotonView>();
                if (ballPv != null)
                {
                    photonView.RPC("RPC_ReleaseBall", RpcTarget.All, ballPv.ViewID);
                }
            }
        }

        // ==========================================
        // FIX 3: MOVED THESE INSIDE THE CLASS!
        // ==========================================
        [PunRPC]
        public void RPC_SufferStealPenalty()
        {
            // Only the victim's local computer needs to run this timer
            if (photonView.IsMine)
            {
                StartCoroutine(StealCooldownRoutine());
            }
        }

        private IEnumerator StealCooldownRoutine()
        {
            // 1. Turn off the ability to steal
            canSteal = false;
            Debug.Log("[STEAL PENALTY] You were robbed! You cannot steal for 1 second.");

            // 2. Wait exactly 1 second
            yield return new WaitForSeconds(1.0f);

            // 3. Turn the ability to steal back on!
            canSteal = true;
            Debug.Log("[STEAL PENALTY] Cooldown finished. You can steal again!");
        }
    } // End of Class
} // End of Namespace