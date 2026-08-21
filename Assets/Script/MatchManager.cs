using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks
{
    public static MatchManager Instance;

    public enum Team { Red, Blue }

    [Header("Match Settings")]
    public float matchTime = 180f; // 3 minutes per round
    public int totalRounds = 3;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundPopupText;

    [Header("Audio")]
    public AudioClip gameStartSound;

    private int currentRound = 1;
    private float currentTime;
    private bool matchActive = false;
    private bool isTimerRunning = false;
    private int lastSyncedSecond = -1;

    private int redTeamScore = 0;
    private int blueTeamScore = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Application.runInBackground = true;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartMatch();
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !matchActive || !isTimerRunning) return;

        currentTime -= Time.deltaTime;

        int currentSecond = Mathf.CeilToInt(currentTime);
        if (currentSecond != lastSyncedSecond)
        {
            photonView.RPC("UpdateTimerUI", RpcTarget.All, currentSecond);
            lastSyncedSecond = currentSecond;
        }

        if (currentTime <= 0)
        {
            isTimerRunning = false;
            currentTime = 0;
            photonView.RPC("UpdateTimerUI", RpcTarget.All, 0);
            EvaluateRoundWinner();
        }
    }

    void StartMatch()
    {
        currentRound = 1;
        currentTime = matchTime;
        matchActive = true;
        isTimerRunning = false;

        photonView.RPC("UpdateScoreUI", RpcTarget.All, redTeamScore, blueTeamScore);
        photonView.RPC("UpdateTimerUI", RpcTarget.All, Mathf.CeilToInt(currentTime));
    }

    public void TeamScored(Team scoringTeam)
    {
        if (PhotonNetwork.IsMasterClient && matchActive && isTimerRunning)
        {
            if (scoringTeam == Team.Red) redTeamScore++;
            else if (scoringTeam == Team.Blue) blueTeamScore++;

            photonView.RPC("UpdateScoreUI", RpcTarget.All, redTeamScore, blueTeamScore);
        }
    }

    void EvaluateRoundWinner()
    {
        string roundWinner = "TIE";
        if (redTeamScore > blueTeamScore) roundWinner = "RED TEAM";
        else if (blueTeamScore > redTeamScore) roundWinner = "BLUE TEAM";

        photonView.RPC("RoundOver", RpcTarget.All, roundWinner, currentRound);
    }

    [PunRPC]
    void RoundOver(string roundWinnerName, int roundFinished)
    {
        matchActive = false;
        isTimerRunning = false;

        // ========================================================
        // THE FIX: CHECK FOR A TIE FIRST!
        // ========================================================
        bool gameTied = (roundWinnerName == "TIE");

        // Setup the winner (If it's a tie, this enum gets ignored anyway)
        Team winningTeamEnum = Team.Red;
        if (roundWinnerName == "BLUE TEAM")
        {
            winningTeamEnum = Team.Blue;
        }

        PhotonNetworkPlayer[] allPlayers = FindObjectsByType<PhotonNetworkPlayer>(FindObjectsSortMode.None);

        foreach (PhotonNetworkPlayer player in allPlayers)
        {
            if (player.photonView.IsMine)
            {
                // Pass BOTH the team and the tie-check into the gambling math!
                player.ResolveMatch(winningTeamEnum, gameTied);
                break;
            }
        }

        StartCoroutine(RoundTransitionRoutine(roundWinnerName, roundFinished));
    }

    IEnumerator RoundTransitionRoutine(string roundWinnerName, int roundFinished)
    {
        if (roundPopupText != null)
        {
            roundPopupText.text = $"{roundWinnerName} WINS ROUND {roundFinished}!";
            roundPopupText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(4f);

        if (roundPopupText != null) roundPopupText.gameObject.SetActive(false);

        if (roundFinished < totalRounds)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("StartNewRoundRPC", RpcTarget.All, roundFinished + 1);
            }
        }
        else
        {
            PlayerPrefs.SetString("WinningTeam", roundWinnerName);
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("WinScene");
            }
        }
    }

    [PunRPC]
    void StartNewRoundRPC(int nextRound)
    {
        currentRound = nextRound;
        redTeamScore = 0;
        blueTeamScore = 0;
        currentTime = matchTime;

        UpdateScoreUI(redTeamScore, blueTeamScore);
        UpdateTimerUI(Mathf.CeilToInt(currentTime));

        if (GambleUIController.Instance != null)
        {
            GambleUIController.Instance.OpenGamblePanelForNewRound();
        }

        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.ResetForNewRound();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            matchActive = true;
            isTimerRunning = false;
        }
    }

    public void ResumeTimer()
    {
        if (PhotonNetwork.IsMasterClient && matchActive)
        {
            // 2. ADDED: Tell all clients to play the start sound when the timer resumes
            if (!isTimerRunning) 
            {
                photonView.RPC("PlayStartSoundRPC", RpcTarget.All);
            }
            
            isTimerRunning = true;
        }
    }

    [PunRPC]
    void PlayStartSoundRPC()
    {
        if (GlobalAudioManager.instance != null && gameStartSound != null)
        {
            GlobalAudioManager.instance.PlaySFX(gameStartSound);
        }
    }

    [PunRPC]
    void UpdateTimerUI(int timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60F);
        int seconds = Mathf.FloorToInt(timeRemaining - minutes * 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [PunRPC]
    void UpdateScoreUI(int red, int blue)
    {
        scoreText.text = $"<color=red>{red}</color> - <color=blue>{blue}</color>";
    }
}