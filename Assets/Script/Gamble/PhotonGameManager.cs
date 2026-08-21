using UnityEngine;
using Photon.Pun;

public class PhotonGameManager : MonoBehaviourPunCallbacks
{
    // ==========================================
    // --- NEW: THE SINGLETON ---
    // ==========================================
    public static PhotonGameManager Instance;

    public enum GameState { Gamble, Soccer, Results }
    public GameState currentGameState = GameState.Gamble;
    private bool rolesAssigned = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ==========================================
    // --- NEW: WAKES THE MANAGER BACK UP! ---
    // ==========================================
    public void ResetForNewRound()
    {
        currentGameState = GameState.Gamble;
        rolesAssigned = false;
        Debug.Log("[MANAGER] Game Manager reset! Ready to roll the 80% Imposter chance again.");
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (currentGameState == GameState.Gamble)
        {
            // 1. Wait for ALL players to click "Confirm" (even if bet is 0)
            if (!rolesAssigned && CheckIfAllBetsPlaced())
            {
                rolesAssigned = true;
                AssignExorcistRole();
            }

            // 2. Start match only when roles are set and decisions are made
            if (rolesAssigned && AllPlayersReadyToPlay())
            {
                photonView.RPC("RPC_StartSoccerMatch", RpcTarget.AllBuffered);
            }
        }
    }

    bool CheckIfAllBetsPlaced()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return false;

        foreach (GameObject p in players)
        {
            var data = p.GetComponent<PhotonNetworkPlayer>();

            // If a player hasn't placed a bet, tell us WHO it is!
            if (!data.hasPlacedBet)
            {
                Debug.Log($"Still waiting for {p.GetComponent<PhotonView>().Owner.NickName} (or a dummy object) to bet...");
                return false;
            }
        }
        return true;
    }

    void AssignExorcistRole()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (Random.Range(0, 100) <= 80 && players.Length > 0)
        {
            int randomIndex = Random.Range(0, players.Length);
            string chosenName = players[randomIndex].GetComponent<PhotonView>().Owner.NickName;

            Debug.Log($"[MANAGER] The 80% chance hit! Imposter choice sent to: {chosenName}");

            players[randomIndex].GetComponent<PhotonView>().RPC("RPC_SetAsPotentialImposter", RpcTarget.AllBuffered);
        }
        else
        {
            Debug.Log("[MANAGER] The 20% chance hit. No imposter this round. Proceeding to match.");
        }
    }

    [PunRPC]
    void RPC_StartSoccerMatch()
    {
        currentGameState = GameState.Soccer;
        Debug.Log("Soccer Match Started!");

        if (GambleUIController.Instance != null)
        {
            GambleUIController.Instance.HideAllPanels();
        }
        else
        {
            Debug.LogError("[UI ERROR] Manager could not find GambleUIController.Instance!");
        }
    }

    bool AllPlayersReadyToPlay()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            var data = p.GetComponent<PhotonNetworkPlayer>();
            if (data.isImposter && !data.decisionMade) return false;
        }
        return true;
    }
}