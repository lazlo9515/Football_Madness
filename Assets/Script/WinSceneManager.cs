using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class WinSceneManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI finalBalanceText;
    public TextMeshProUGUI leaderboardText;

    // A small data container to help us sort the players
    private struct PlayerData
    {
        public string playerName;
        public int finalCoins;
        public bool isLocalPlayer;
    }

    void Start()
    {
        // Display the Final Round Winner (just for flavor)
        string winner = PlayerPrefs.GetString("WinningTeam", "NOBODY");
        winnerText.text = winner + " WON ROUND 3!";

        if (winner == "RED TEAM") winnerText.color = Color.red;
        else if (winner == "BLUE TEAM") winnerText.color = Color.blue;
        else winnerText.color = Color.white;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (leaderboardText != null) leaderboardText.text = "Calculating Final Wealth...";
        if (finalBalanceText != null) finalBalanceText.text = "Loading...";

        StartCoroutine(FetchAndSortServerBalances());
    }

    private IEnumerator FetchAndSortServerBalances()
    {
        // Wait 0.5 seconds for the server to finish sending the final math
        yield return new WaitForSeconds(0.5f);

        List<PlayerData> allPlayersData = new List<PlayerData>();

        // Loop through everyone on the server and pack their data into our list
        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            int playerBalance = 1000; // Fallback

            if (p.CustomProperties.ContainsKey("FinalBalance"))
            {
                playerBalance = (int)p.CustomProperties["FinalBalance"];
            }

            allPlayersData.Add(new PlayerData
            {
                playerName = p.NickName,
                finalCoins = playerBalance,
                isLocalPlayer = p.IsLocal
            });
        }

        // --- THE MAGIC: Sort the list from highest coins to lowest coins ---
        allPlayersData.Sort((playerA, playerB) => playerB.finalCoins.CompareTo(playerA.finalCoins));

        if (leaderboardText != null) leaderboardText.text = "FINAL LEADERBOARD:\n\n";

        // Build the Leaderboard text
        for (int i = 0; i < allPlayersData.Count; i++)
        {
            // If this player is YOU, update your personal text box
            if (allPlayersData[i].isLocalPlayer && finalBalanceText != null)
            {
                finalBalanceText.text = "Your Final Balance: " + allPlayersData[i].finalCoins + " Coins";
            }

            // Add them to the leaderboard string (e.g., "1. Jooven: 4500 Coins")
            if (leaderboardText != null)
            {
                leaderboardText.text += $"{i + 1}. {allPlayersData[i].playerName}: {allPlayersData[i].finalCoins} Coins\n";
            }
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }
}