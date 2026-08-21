using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    // 【新增】：声明 playerTransform 变量，解决 CS0103 错误
    public Transform playerTransform;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    void Start()
    {
        // 【修改 1】：左右两边都使用 PlayerControllerManager，解决 CS0246 错误
        // 【修改 2】：使用 FindFirstObjectByType 替换过时的 FindObjectOfType，消除黄色警告
        PlayerControllerManager pm = FindAnyObjectByType<PlayerControllerManager>();

        if (pm != null)
            playerTransform = pm.transform;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
        {
            // Every player who loads this scene will run this line LOCALLY
            // Because it's a networked instantiate, everyone will see everyone else.
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

            // Use ActorNumber to prevent spawning on the same spot
            int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;

            Vector3 pos = spawnPoints[index].transform.position;

            // Instantiate the character directly
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Character"), pos, Quaternion.identity);
        }
    }
}