using UnityEngine;

public class MatchResolver : MonoBehaviour
{
    private BettingSystem bettingSystem;
    private PlayerFinanceManager finance;

    // These would be set during the Gamble UI phase
    public int imposterPlayerID = -1;
    public bool imposterAccepted = false;

    void Start()
    {
        bettingSystem = GetComponent<BettingSystem>();
        finance = GetComponent<PlayerFinanceManager>();
    }

    public void ResolveMatchResults(int winningTeam)
    {
        // Winning Team 0 = Players 0, 1 | Winning Team 1 = Players 2, 3
        int imposterTeam = (imposterPlayerID < 2) ? 0 : 1;

        for (int i = 0; i < 4; i++)
        {
            int playerBet = bettingSystem.currentBets[i];
            bool isOnWinningTeam = (winningTeam == 0 && i < 2) || (winningTeam == 1 && i >= 2);

            // --- IMPOSTER SPECIAL LOGIC ---
            if (i == imposterPlayerID)
            {
                if (imposterAccepted)
                {
                    // If he accepted and his team LOST = He Wins!
                    if (!isOnWinningTeam)
                    {
                        finance.AdjustBalance(i, playerBet * 3); // 200% profit + original bet
                        Debug.Log("Imposter Betrayal Success!");
                    }
                }
                else
                {
                    // If he declined and his team LOST = Double Loss Penalty
                    if (!isOnWinningTeam)
                    {
                        finance.AdjustBalance(i, -(playerBet * 2)); // Lost 200% of the bet
                        Debug.Log("Loyalty Penalty: Lost 200%!");
                    }
                    else
                    {
                        // If he declined and WON, he just gets normal winnings
                        finance.AdjustBalance(i, playerBet * 2);
                    }
                }
            }
            // --- NORMAL PLAYER LOGIC ---
            else if (isOnWinningTeam)
            {
                finance.AdjustBalance(i, playerBet * 2);
            }
        }
    }
}