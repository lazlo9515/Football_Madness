using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [SerializeField] InputField roomNameInputField;

    [SerializeField] TMP_Text roomNameText;

    [SerializeField] Text errorText;

    [SerializeField] Transform roomListContent;

    [SerializeField] GameObject roomListItemPrefab;

    [SerializeField] Transform playerListContent;

    [SerializeField] GameObject playerListItemPrefab;

    public GameObject startButton;

    int nextTeamNumber = 1;

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        Debug.Log("Connecting to Master...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;

        // Start the 3-second delay
        StartCoroutine(WaitAndShowMenu());
    }

    // 2. Add this new Coroutine function
    IEnumerator WaitAndShowMenu()
    {
        // Ensure the LoadingMenu is visible first
        MenuManager.instance.OpenMenu("LoadingMenu");

        // Wait for 3 seconds
        yield return new WaitForSeconds(0f);

        // After 3 seconds, open the UserNameMenu
        MenuManager.instance.OpenMenu("UserNameMenu");
        Debug.Log("3 seconds passed, showing UserNameMenu");
    }

    //public override void OnConnectedToMaster()
    //{
    //    Debug.Log("Connected to Master");
    //    PhotonNetwork.JoinLobby();
    //    PhotonNetwork.AutomaticallySyncScene = true;
    //}

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("UserNameMenu");
        Debug.Log("Joined Lobby successfully!");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }

        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.instance.OpenMenu("LoadingMenu");
    }
    
    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("RoomMenu");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for(int i = 0; i < players.Count(); i++)
        {
            int teamNumber = GetNextTeamNumber();

            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i], teamNumber);

        }

        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string errorMessage)
    {
        errorText.text = "Room Generation Unsuccessful" + errorMessage;
        MenuManager.instance.OpenMenu("ErrorMenu");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("LoadingMenu");

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("LoadingMenu");
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("TitleMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for(int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        int teamNumber = GetNextTeamNumber();

        GameObject playerItem = Instantiate(playerListItemPrefab, playerListContent);
        playerItem.GetComponent<PlayerListItem>().SetUp(newPlayer, teamNumber);
    }

    private int GetNextTeamNumber()
    {
        int teamNumber = nextTeamNumber;
        nextTeamNumber = 3 - nextTeamNumber;
        return teamNumber;
    }
}

