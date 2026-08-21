using LightningPoly.FootballEssentials3D;
using Photon.Pun;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class PhotonNetworkPlayer : MonoBehaviourPunCallbacks
{
    [Header("Player Data")]
    public int currentMoney = 1000;
    public int myCurrentBet = 0;
    public bool hasPlacedBet = false;
    public bool isImposter = false;
    public bool decisionMade = false;

    [Header("Match Info")]
    public MatchManager.Team myTeam;

    [Header("Respawn Settings")]
    public Behaviour[] scriptsToDisable;
    public GameObject[] visualsToHide;

    void Start()
    {
        if (photonView.IsMine)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
            {
                int teamNumber = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];

                if (teamNumber == 1) myTeam = MatchManager.Team.Red;
                else if (teamNumber == 2) myTeam = MatchManager.Team.Blue;
            }
            else
            {
                if (PhotonNetwork.IsMasterClient) myTeam = MatchManager.Team.Red;
                else myTeam = MatchManager.Team.Blue;
            }

            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash["FinalBalance"] = currentMoney;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            SetupLocalCamera();
        }
    }

    void SetupLocalCamera()
    {
        GameObject redCamObj = GameObject.Find("RedVCam");
        GameObject blueCamObj = GameObject.Find("BlueVCam");

        if (redCamObj == null || blueCamObj == null) return;

        CinemachineCamera redVCam = redCamObj.GetComponent<CinemachineCamera>();
        CinemachineCamera blueVCam = blueCamObj.GetComponent<CinemachineCamera>();

        if (myTeam == MatchManager.Team.Red)
        {
            blueVCam.enabled = false;
            redVCam.enabled = true;
            redVCam.Priority = 20;
            redVCam.Follow = this.transform;
            redVCam.LookAt = this.transform;
        }
        else if (myTeam == MatchManager.Team.Blue)
        {
            redVCam.enabled = false;
            blueVCam.enabled = true;
            blueVCam.Priority = 20;
            blueVCam.Follow = this.transform;
            blueVCam.LookAt = this.transform;
        }
    }

    public void PlaceBet(int amount)
    {
        photonView.RPC("RPC_SetBet", RpcTarget.AllBuffered, amount);
    }

    [PunRPC]
    void RPC_SetBet(int amount)
    {
        myCurrentBet = amount;
        hasPlacedBet = true;

        if (photonView.IsMine)
        {
            currentMoney -= amount;
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash["FinalBalance"] = currentMoney;
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    [PunRPC]
    public void RPC_SetAsPotentialImposter()
    {
        isImposter = true;
        decisionMade = false;

        if (photonView.IsMine)
        {
            if (GambleUIController.Instance != null)
            {
                GambleUIController.Instance.ShowImposterChoice();
            }
        }
    }

    [PunRPC]
    public void RPC_UpdateDecision(bool accepted)
    {
        isImposter = accepted;
        decisionMade = true;
    }

    // NEW: We added ", bool isTie = false" to the parameters!
    public void ResolveMatch(MatchManager.Team winningTeam, bool isTie = false)
    {
        if (!photonView.IsMine) return;

        // ==========================================
        // NEW: THE TIE BREAKER (CASINO PUSH)
        // ==========================================
        if (isTie)
        {
            // Just give them their original bet back. No winners, no losers.
            currentMoney += myCurrentBet;
            Debug.Log($"[GAMBLE] Match was a TIE! Refunded your {myCurrentBet} coins.");
        }
        else
        {
            // ==========================================
            // THE NORMAL GAMBLING FORMULA (No Tie)
            // ==========================================
            bool myTeamWon = (myTeam == winningTeam);

            // ROLE 1: ACCEPTED IMPOSTER
            if (isImposter && decisionMade)
            {
                if (!myTeamWon)
                {
                    int winnings = myCurrentBet * 3;
                    currentMoney += winnings;
                    Debug.Log($"[GAMBLE] Sabotage successful! Payout: {winnings}");
                }
                else
                {
                    Debug.Log($"[GAMBLE] Sabotage failed. You lost your {myCurrentBet} coin bet.");
                }
            }
            // ROLE 2: DECLINED IMPOSTER 
            else if (!isImposter && decisionMade)
            {
                if (myTeamWon)
                {
                    int winnings = Mathf.RoundToInt(myCurrentBet * 2.5f);
                    currentMoney += winnings;
                    Debug.Log($"[GAMBLE] Match won (Declined)! Payout: {winnings}");
                }
                else
                {
                    currentMoney -= myCurrentBet;
                    Debug.Log($"[GAMBLE] Match lost! Double Penalty applied. Lost an extra {myCurrentBet} coins.");
                }
            }
            // ROLE 3: NORMAL PLAYER
            else
            {
                if (myTeamWon)
                {
                    int winnings = myCurrentBet * 2;
                    currentMoney += winnings;
                    Debug.Log($"[GAMBLE] Match won! Payout: {winnings}");
                }
                else
                {
                    Debug.Log($"[GAMBLE] Match lost. You lost your {myCurrentBet} coin bet.");
                }
            }
        }

        // Bankruptcy protection
        if (currentMoney < 0) currentMoney = 0;

        // Push to server
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash["FinalBalance"] = currentMoney;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

        photonView.RPC("RPC_ResetRoundData", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_ResetRoundData()
    {
        myCurrentBet = 0;
        hasPlacedBet = false;
        isImposter = false;
        decisionMade = false;
    }

    [PunRPC]
    public void RPC_DieAndRespawn()
    {
        PlayerBallController ballController = GetComponent<PlayerBallController>();
        if (ballController != null) ballController.ForceReleaseBall();

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        GetComponent<Collider>().enabled = false;

        foreach (var visual in visualsToHide)
        {
            if (visual != null && visual != this.gameObject)
                visual.SetActive(false);
        }

        if (photonView.IsMine)
        {
            foreach (var script in scriptsToDisable)
                if (script != null) script.enabled = false;

            if (GambleUIController.Instance != null && GambleUIController.Instance.deathPanel != null)
                GambleUIController.Instance.deathPanel.SetActive(true);
        }

        for (int i = 3; i > 0; i--)
        {
            if (photonView.IsMine && GambleUIController.Instance != null)
                GambleUIController.Instance.deathText.text = "YOU DIED!\nRespawning in " + i + "...";

            yield return new WaitForSeconds(1f);
        }

        Transform targetSpawn = null;
        if (myTeam == MatchManager.Team.Red)
            targetSpawn = GameObject.Find("RedSpawn")?.transform;
        else
            targetSpawn = GameObject.Find("BlueSpawn")?.transform;

        if (targetSpawn != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;

            transform.position = targetSpawn.position;
        }

        if (photonView.IsMine && GambleUIController.Instance != null && GambleUIController.Instance.deathPanel != null)
        {
            GambleUIController.Instance.deathPanel.SetActive(false);
        }

        GetComponent<Collider>().enabled = true;

        foreach (var visual in visualsToHide)
            if (visual != null && visual != this.gameObject) visual.SetActive(true);

        if (photonView.IsMine)
        {
            foreach (var script in scriptsToDisable)
                if (script != null) script.enabled = true;
        }
    }
}