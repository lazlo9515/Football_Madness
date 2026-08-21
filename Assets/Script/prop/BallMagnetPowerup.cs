using UnityEngine;
using Photon.Pun;
using LightningPoly.FootballEssentials3D;

public class BallMagnetPowerup : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Ensure only the local player who hit the trigger runs this logic
        PhotonView playerPv = other.GetComponent<PhotonView>();

        if (playerPv != null && playerPv.IsMine)
        {
            PlayerPowerupState playerState = other.GetComponent<PlayerPowerupState>();

            // Check for active powerup lock
            if (playerState != null && playerState.hasActivePowerup)
            {
                return;
            }

            // Execute the steal
            ActivateMagnet(playerPv);

            // 2. Request cleanup
            RequestDestruction();
        }
    }

    void ActivateMagnet(PhotonView playerPv)
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            PhotonView ballPv = ball.GetComponent<PhotonView>();
            if (ballPv != null)
            {
                // CHANGE: Use AllViaServer for better synchronization
                playerPv.RPC("RPC_StealBall", RpcTarget.AllViaServer, ballPv.ViewID);
            }
        }
    }

    void RequestDestruction()
    {
        // If it's a networked object (Spawned via PhotonNetwork.Instantiate)
        if (photonView != null && photonView.ViewID != 0)
        {
            // We tell the Master Client to destroy it so it disappears for everyone
            photonView.RPC("RPC_MasterDestroyMagnet", RpcTarget.MasterClient);
        }
        else
        {
            // Just a local object in the scene
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void RPC_MasterDestroyMagnet()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}