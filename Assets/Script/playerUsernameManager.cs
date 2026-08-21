using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class playerUsernameManager : MonoBehaviour
{
    [SerializeField] private InputField usernameInput;
    [SerializeField] private Text errorMessageText;

    public void OnSubmitUsername() 
    {
        string username = usernameInput.text;

        if(!string.IsNullOrEmpty(username) && username.Length <= 20)
        {
            PhotonNetwork.NickName = username;
            
            errorMessageText.text = "";
            MenuManager.instance.OpenMenu("TitleMenu");
        }
        else
        {
            errorMessageText.text = "Username must not be empty and should be 20 characters or less.";
        }
    }
}