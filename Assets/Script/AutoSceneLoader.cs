using System.Collections;
using UnityEngine;
using Photon.Pun;

public class AutoSceneLoader : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(2f);

        PhotonNetwork.LoadLevel("MainMenu"); // Scene 2 name
    }
}