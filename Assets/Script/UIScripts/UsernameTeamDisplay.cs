using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UsernameTeamDisplay : MonoBehaviour
{
    public Text usernameText;
    public Text teamText;

    public PhotonView view;

    void Start()
    {
        if(view.IsMine)
        {
            //don't want to display username
            gameObject.SetActive(false);
        }

        usernameText.text = view.Owner.NickName;

        //show team number
        if(view.Owner.CustomProperties.ContainsKey("Team"))
        {
            int team = (int)view.Owner.CustomProperties["Team"];
            teamText.text = "Team " + team;
        }

    }
}
