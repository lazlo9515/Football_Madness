using LightningPoly.FootballEssentials3D;
using UnityEngine;
using Photon.Pun;

public class GunPickup : MonoBehaviour
{
    public Vector3 offset = new Vector3(0.05f, 0.1f, 0.1f);

    // FIX 1: Made this PUBLIC so we don't accidentally steal it from our friend's hand!
    public bool isPickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        PhotonView playerPv = other.GetComponent<PhotonView>();

        if (playerPv != null && playerPv.IsMine)
        {
            PlayerPowerupState playerState = other.GetComponent<PlayerPowerupState>();
            if (playerState != null && playerState.hasActivePowerup) return;

            isPickedUp = true;
            if (playerState != null) playerState.hasActivePowerup = true;

            // Snap it to your hand locally
            GetComponent<Collider>().enabled = false;
            transform.SetParent(other.transform);
            transform.localPosition = offset;
            transform.localRotation = Quaternion.identity;

            // Shout to the server!
            playerPv.RPC("RPC_SyncGunPickup", RpcTarget.Others);
        }
    }
}