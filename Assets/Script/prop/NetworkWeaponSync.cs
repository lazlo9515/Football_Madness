using UnityEngine;
using Photon.Pun;
using LightningPoly.FootballEssentials3D;

public class NetworkWeaponSync : MonoBehaviourPun
{
    [Header("Fallback Settings")]
    // FIX 2: We need a backup plan if no guns are on the map!
    public GameObject fallbackGunPrefab;

    [PunRPC]
    public void RPC_SyncGunPickup()
    {
        if (photonView.IsMine) return; // Local player already grabbed it!

        PlayerPowerupState state = GetComponent<PlayerPowerupState>();
        if (state != null) state.hasActivePowerup = true;

        // ==========================================
        // ATTEMPT 1: FIND A LOOSE GUN ON THE GRASS
        // ==========================================
        GunPickup[] allGuns = FindObjectsByType<GunPickup>(FindObjectsSortMode.None);

        foreach (GunPickup gun in allGuns)
        {
            // If the gun has no parent AND hasn't been picked up yet
            if (gun.transform.parent == null && !gun.isPickedUp)
            {
                gun.isPickedUp = true; // Lock it so we can't accidentally steal it!
                gun.GetComponent<Collider>().enabled = false;
                gun.transform.SetParent(this.transform);
                gun.transform.localPosition = gun.offset;
                gun.transform.localRotation = Quaternion.identity;

                Debug.Log("[NETWORK WEAPON] Stole a loose gun and snapped it to remote player!");
                return; // We found one, stop here!
            }
        }

        // ==========================================
        // ATTEMPT 2: NO GUNS FOUND? SPAWN A VISUAL FAKE!
        // ==========================================
        if (fallbackGunPrefab != null)
        {
            GameObject fakeGun = Instantiate(fallbackGunPrefab, this.transform);

            GunPickup fakeGunScript = fakeGun.GetComponent<GunPickup>();
            if (fakeGunScript != null)
            {
                fakeGunScript.isPickedUp = true; // Lock it!
                fakeGun.transform.localPosition = fakeGunScript.offset;
            }

            fakeGun.GetComponent<Collider>().enabled = false;
            fakeGun.transform.localRotation = Quaternion.identity;

            Debug.Log("[NETWORK WEAPON] No loose guns found on my screen. Spawned a fallback visual gun!");
        }
        else
        {
            Debug.LogError("[NETWORK WEAPON] Failed to sync gun! Please assign the Fallback Gun Prefab in the Player Inspector.");
        }
    }

    [PunRPC]
    public void RPC_SyncGunDestroy()
    {
        if (photonView.IsMine) return;

        OneShotGun gun = GetComponentInChildren<OneShotGun>();
        if (gun != null)
        {
            Destroy(gun.gameObject);
        }
    }
}