using UnityEngine;
using System.Collections;

public class SizePowerup : MonoBehaviour
{
    public float sizeMultiplier = 3f;
    public float massMultiplier = 10f;
    public float duration = 7f;
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            // 1. CHECK THE LOCK on the Player
            PlayerPowerupState playerState = other.GetComponent<PlayerPowerupState>();

            // If the script exists AND they already have a power-up, stop right here!
            if (playerState != null && playerState.hasActivePowerup)
            {
                return;
            }

            isTriggered = true;

            // Pass the playerState into the Coroutine so we can unlock it later
            StartCoroutine(ApplySizeEffect(other.gameObject, playerState));
        }
    }

    IEnumerator ApplySizeEffect(GameObject player, PlayerPowerupState playerState)
    {
        // 2. LOCK IT: Tell the player they now have an active power-up
        if (playerState != null) playerState.hasActivePowerup = true;

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        Vector3 originalScale = player.transform.localScale;
        Rigidbody rb = player.GetComponent<Rigidbody>();
        float originalMass = 1f;

        if (rb != null) originalMass = rb.mass;

        player.transform.localScale = originalScale * sizeMultiplier;
        if (rb != null) rb.mass = originalMass * massMultiplier;

        yield return new WaitForSeconds(duration);

        if (player != null)
        {
            player.transform.localScale = originalScale;
            if (rb != null) rb.mass = originalMass;
        }

        // 3. UNLOCK IT: The power-up is done, the player can pick up new ones now
        if (playerState != null) playerState.hasActivePowerup = false;

        Destroy(gameObject);
    }
}