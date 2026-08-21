using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using LightningPoly.FootballEssentials3D;

public class GambleUIController : MonoBehaviour
{
    [Header("Death UI")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathText;

    public static GambleUIController Instance;

    [Header("Panels")]
    public GameObject gamblePanel;
    public GameObject imposterChoicePanel;
    public GameObject waitingPanel;

    [Header("UI Elements")]
    public Slider betSlider;
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI currentBetText;

    // =========================================================
    // --- NEW: THE TEXT BOX FOR EVERYONE'S MONEY ---
    // =========================================================
    public TextMeshProUGUI allPlayersBankText;

    [Header("Buttons")]
    public Button confirmButton;
    public Button acceptButton;
    public Button rejectButton;

    private PhotonNetworkPlayer localPlayerData;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmBet);
        acceptButton.onClick.AddListener(OnAcceptImposter);
        rejectButton.onClick.AddListener(OnRejectImposter);
        betSlider.onValueChanged.AddListener(delegate { UpdateBetText(); });

        StartCoroutine(WaitForPlayerAndShowUI());
    }

    IEnumerator WaitForPlayerAndShowUI()
    {
        while (localPlayerData == null)
        {
            FindLocalPlayer();
            yield return new WaitForSeconds(0.1f);
        }

        OpenGamblePanelForNewRound();
    }

    void FindLocalPlayer()
    {
        PhotonNetworkPlayer[] players = FindObjectsByType<PhotonNetworkPlayer>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.photonView.IsMine)
            {
                localPlayerData = p;
                break;
            }
        }
    }

    public void OpenGamblePanelForNewRound()
    {
        if (gamblePanel != null && localPlayerData != null)
        {
            if (waitingPanel) waitingPanel.SetActive(false);
            if (imposterChoicePanel) imposterChoicePanel.SetActive(false);

            gamblePanel.SetActive(true);

            balanceText.text = "Balance: " + localPlayerData.currentMoney;
            betSlider.maxValue = localPlayerData.currentMoney;
            betSlider.value = 0;
            UpdateBetText();

            // =========================================================
            // --- NEW: LOOP THROUGH EVERYONE AND CHECK THEIR POCKETS ---
            // =========================================================
            if (allPlayersBankText != null)
            {
                string bankList = "Players' Wealth:\n";

                // Look at every player connected to the Photon Room
                foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
                {
                    int playerMoney = 1000; // Default fallback

                    // Grab their actual money from the server
                    if (p.CustomProperties.ContainsKey("FinalBalance"))
                    {
                        playerMoney = (int)p.CustomProperties["FinalBalance"];
                    }

                    // Add their name and money to the text list
                    bankList += $"{p.NickName}: {playerMoney} Coins\n";
                }

                // Update the UI text box
                allPlayersBankText.text = bankList;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Player moveScript = localPlayerData.GetComponent<Player>();
            if (moveScript == null) moveScript = localPlayerData.GetComponentInChildren<Player>();

            if (moveScript != null)
            {
                moveScript.enabled = false;
                Rigidbody rb = moveScript.GetComponent<Rigidbody>();
                if (rb != null) rb.linearVelocity = Vector3.zero;
            }
        }
    }

    void UpdateBetText()
    {
        currentBetText.text = "Betting: " + (int)betSlider.value + " Coins";
    }

    public void OnConfirmBet()
    {
        if (localPlayerData != null)
        {
            localPlayerData.PlaceBet((int)betSlider.value);
            gamblePanel.SetActive(false);
            waitingPanel.SetActive(true);
        }
    }

    public void ShowImposterChoice()
    {
        if (waitingPanel) waitingPanel.SetActive(false);
        if (gamblePanel) gamblePanel.SetActive(false);

        if (imposterChoicePanel != null)
        {
            imposterChoicePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OnAcceptImposter()
    {
        if (localPlayerData != null)
        {
            localPlayerData.photonView.RPC("RPC_UpdateDecision", RpcTarget.AllBuffered, true);
            imposterChoicePanel.SetActive(false);
            waitingPanel.SetActive(true);
        }
    }

    public void OnRejectImposter()
    {
        if (localPlayerData != null)
        {
            localPlayerData.photonView.RPC("RPC_UpdateDecision", RpcTarget.AllBuffered, false);
            imposterChoicePanel.SetActive(false);
            waitingPanel.SetActive(true);
        }
    }

    public void HideAllPanels()
    {
        if (gamblePanel) gamblePanel.SetActive(false);
        if (imposterChoicePanel) imposterChoicePanel.SetActive(false);
        if (waitingPanel) waitingPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (localPlayerData != null)
        {
            Player moveScript = localPlayerData.GetComponent<Player>();
            if (moveScript == null) moveScript = localPlayerData.GetComponentInChildren<Player>();

            if (moveScript != null) moveScript.enabled = true;
        }

        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.ResumeTimer();
        }
    }
}