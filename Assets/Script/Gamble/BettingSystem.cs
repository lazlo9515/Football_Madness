using UnityEngine;

public class BettingSystem : MonoBehaviour
{
    // Stores the current bet for each player for the active round
    public int[] currentBets = new int[4];
    private PlayerFinanceManager finance;

    void Awake()
    {
        finance = GetComponent<PlayerFinanceManager>();
    }

    // Call this from your UI Button (e.g., a "Bet 100" button)
    public bool PlaceBet(int playerID, int amount)
    {
        if (finance.playerBalances[playerID] >= amount)
        {
            currentBets[playerID] = amount;
            finance.AdjustBalance(playerID, -amount); // Take money now
            Debug.Log($"Player {playerID} bet {amount} on themselves!");
            return true;
        }
        return false; // Not enough money
    }

    // Call this when the match ends
    public void ResolveBets(int winningTeam)
    {
        // Winning Team 0 = Players 0 & 1
        // Winning Team 1 = Players 2 & 3
        int startIdx = (winningTeam == 0) ? 0 : 2;
        int endIdx = (winningTeam == 0) ? 1 : 3;

        for (int i = 0; i < 4; i++)
        {
            if (i >= startIdx && i <= endIdx)
            {
                // Winner: Get back double their bet
                int winnings = currentBets[i] * 2;
                finance.AdjustBalance(i, winnings);
                Debug.Log($"Player {i} won {winnings} coins!");
            }
            else
            {
                // Loser: The money is already gone from PlaceBet
                Debug.Log($"Player {i} lost their bet.");
            }

            // Reset bet for next round
            currentBets[i] = 0;
        }
    }
}