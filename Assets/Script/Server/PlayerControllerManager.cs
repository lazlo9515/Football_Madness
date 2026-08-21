using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerControllerManager : MonoBehaviourPunCallbacks
{
    PhotonView view;

    GameObject controller;

    public int playerTeam;

    private Dictionary<int, int> playerTeams = new Dictionary<int, int>();

    void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    void Start()
    {
        if(view.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
        {
            playerTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            Debug.Log("Player's Team: " + playerTeam);
        }

        // This works now because we added the 'int team' parameter below
        AssignTeamsToaSpawnArea(playerTeam);
    }

    // FIX 1: Added "int team" so it can receive the playerTeam variable
    void AssignTeamsToaSpawnArea(int team)
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Character"), Vector3.zero, Quaternion.identity);
    }

    void AssignTeamsToAllPlayers()
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Team"))
            {
                int team = (int)player.CustomProperties["Team"];
                playerTeams[player.ActorNumber] = team;
                Debug.Log(player.NickName + "'s Team: " + team);
            }
        }
    }

    // FIX 2: Added the required (Photon.Realtime.Player newPlayer) parameters
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        // FIX 3: Added the 's' to match the method name exactly
        AssignTeamsToAllPlayers();
    }
}

    //void CreateController()
    //{
    //    if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team"))
    //    {
    //        playerTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
    //        Debug.Log("Player's Team: " + playerTeam);
    //    }

    //    AssignTeamsToaSpawnArea(playerTeam);

    //}

    //void AssignTeamsToaSpawnArea()
    //{
    //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Character"), Vector3.zero, Quaternion.identity);

    //}

    //void AssignTeamsToAllPlayers()
    //{
    //    foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList)
    //    {
    //        if(player.CustomProperties.ContainsKey("Team"))
    //        {
    //            int team = (int)player.CustomProperties["Team"];
    //            playerTeams[player.ActorNumber] = team;
    //            Debug.Log(player.NickName + "'s Team: " + team);
    //        }
    //    }
    //}

    //public override void OnPlayerEnteredRoom
    //{
    //    AssignTeamsToAllPlayer();
    //}

