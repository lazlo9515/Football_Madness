using UnityEngine;
using System.Collections.Generic;

public class PlayerFinanceManager : MonoBehaviour
{
    // Dictionary to store Player ID and their Coin balance
    public Dictionary<int, int> playerBalances = new Dictionary<int, int>();
    public int startingCoins = 1000;
    public int coinsPerRound = 200;

    // Inside PlayerFinanceManager.cs
    void Awake() // Changed from Start to Awake
    {
        for (int i = 0; i < 4; i++)
        {
            if (!playerBalances.ContainsKey(i)) // Safety check
            {
                playerBalances.Add(i, startingCoins);
            }
        }
    }

    // Call this at the start of every round
    public void StartNewRound()
    {
        foreach (var id in new List<int>(playerBalances.Keys))
        {
            playerBalances[id] += coinsPerRound;
            Debug.Log($"Player {id} now has {playerBalances[id]} coins.");
        }
    }

    public void AdjustBalance(int playerID, int amount)
    {
        if (playerBalances.ContainsKey(playerID))
        {
            playerBalances[playerID] += amount;
        }
    }
}