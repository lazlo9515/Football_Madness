using UnityEngine;
using System.Collections;
using LightningPoly.FootballEssentials3D;

public class SpeedBoostProp : MonoBehaviour
{
    [Header("Settings")]
    public float multiplier = 2.0f;
    public float duration = 3.0f;

    [Header("Visuals")]
    public GameObject artModel;

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();

        if (player != null)
        {
            // 1. CHECK THE LOCK
            PlayerPowerupState playerState = other.GetComponent<PlayerPowerupState>();

            // If they are already powered up or holding a gun, stop here!
            if (playerState != null && playerState.hasActivePowerup)
            {
                return;
            }

            // Pass the playerState into the Coroutine
            StartCoroutine(ApplyBoost(player, playerState));
        }
    }

    IEnumerator ApplyBoost(Player player, PlayerPowerupState playerState)
    {
        // 2. LOCK IT: Tell the system the player is currently powered up
        if (playerState != null) playerState.hasActivePowerup = true;

        if (artModel != null) artModel.SetActive(false);
        if (GetComponent<Renderer>() != null) GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= multiplier;
        Debug.Log("Speed Boost Activated!");

        yield return new WaitForSeconds(duration);

        player.moveSpeed = originalSpeed;
        Debug.Log("Speed Boost Expired.");

        // 3. UNLOCK IT: The speed boost is done
        if (playerState != null) playerState.hasActivePowerup = false;

        Destroy(gameObject);
    }
}