using System.Collections;
using UnityEngine;
using LightningPoly.FootballEssentials3D;

public class GoalDetector : MonoBehaviour
{
    [Header("Scoring")]
    [Tooltip("Which team scores a point when the ball goes in this goal?")]
    public MatchManager.Team scoringTeam = MatchManager.Team.Red;

    [Header("Reset Logic")]
    public Transform centerFieldPosition;

    // 1. ADDED: Audio variable for the goal sound
    [Header("Audio")]
    public AudioClip goalSound;

    private bool isProcessingGoal = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !isProcessingGoal)
        {
            isProcessingGoal = true;

            // 2. ADDED: Play the goal sound immediately
            if (GlobalAudioManager.instance != null && goalSound != null)
            {
                GlobalAudioManager.instance.PlaySFX(goalSound);
            }

            Debug.Log($"Goal Scored by {scoringTeam}! Waiting 2 seconds...");

            if (MatchManager.Instance != null)
            {
                MatchManager.Instance.TeamScored(scoringTeam);
            }
            else
            {
                Debug.LogWarning("MatchManager not found!");
            }

            Ball ballScript = other.GetComponent<Ball>();
            if (ballScript != null)
            {
                ballScript.canBeGrabbed = false;
            }

            StartCoroutine(ResetBall(other.gameObject, ballScript));
        }
    }

    private IEnumerator ResetBall(GameObject ball, Ball ballScript)
    {
        yield return new WaitForSeconds(2f);

        ball.transform.position = centerFieldPosition.position;

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        if (ballScript != null)
        {
            ballScript.canBeGrabbed = true;
        }

        // --- TIMER RESUME HAS BEEN REMOVED! Clock keeps ticking. ---

        isProcessingGoal = false;
    }
}