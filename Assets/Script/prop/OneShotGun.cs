using System.Collections;
using UnityEngine;
using Photon.Pun;
using LightningPoly.FootballEssentials3D;

public class OneShotGun : MonoBehaviour
{
    public float range = 100f;
    public LayerMask playerLayer;
    public LineRenderer laserLine;

    public void Fire(Player shooter)
    {
        PlayerBallController ballCtrl = shooter.GetComponent<PlayerBallController>();
        if (ballCtrl != null) ballCtrl.ForceReleaseBall();

        Vector3 startPoint = transform.position;
        Vector3 shootDirection = shooter.transform.forward;
        Vector3 endPoint = startPoint + (shootDirection * range);

        RaycastHit hit;
        if (Physics.Raycast(startPoint, shootDirection, out hit, range, playerLayer))
        {
            endPoint = hit.point;
            PhotonView targetPv = hit.collider.GetComponent<PhotonView>();

            if (targetPv != null && targetPv.ViewID != shooter.GetComponent<PhotonView>().ViewID)
            {
                targetPv.RPC("RPC_DieAndRespawn", RpcTarget.All);
            }
        }

        if (laserLine != null) StartCoroutine(FlashLaser(startPoint, endPoint, shooter));
    }

    IEnumerator FlashLaser(Vector3 start, Vector3 end, Player shooter)
    {
        laserLine.enabled = true;
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);

        yield return new WaitForSeconds(0.05f);
        laserLine.enabled = false;

        if (shooter != null)
        {
            PlayerPowerupState playerState = shooter.GetComponent<PlayerPowerupState>();
            if (playerState != null) playerState.hasActivePowerup = false;

            PhotonView shooterPv = shooter.GetComponent<PhotonView>();
            if (shooterPv != null && shooterPv.IsMine)
            {
                // Use the PLAYER'S network to tell everyone else to destroy the gun!
                shooterPv.RPC("RPC_SyncGunDestroy", RpcTarget.Others);
            }
        }

        // Destroy locally for the shooter
        Destroy(gameObject);
    }
}