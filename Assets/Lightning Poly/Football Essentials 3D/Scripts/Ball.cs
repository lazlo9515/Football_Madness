namespace LightningPoly.FootballEssentials3D
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class Ball : MonoBehaviour
    {
        // ADD THIS LINE: State flag to track if the ball is currently active
        public bool canBeGrabbed = true;
        private Rigidbody rb;

        [Header("Ball Physics Tuning")]
        [Tooltip("Air resistance. Stops the ball from flying forever.")]
        public float airDrag = 0.4f; 
        
        [Tooltip("Rolling resistance. This is what forces the ball to eventually stop rolling.")]
        public float rollingDrag = 1.8f; 

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // A standard size 5 football weighs about 0.45 kg
            rb.mass = 0.45f; 

            // Apply the drags
            rb.linearDamping = airDrag;
            rb.angularDamping = rollingDrag;

            // Prevents the ball from glitching through the goal net or walls at high speeds
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // You can call this from your player script when they press the kick button
        public void KickBall(Vector3 direction, float force)
        {
            // ForceMode.Impulse is best for instant impacts like a foot hitting a ball
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }
    }
}