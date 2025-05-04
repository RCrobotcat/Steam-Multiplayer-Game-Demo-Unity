using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class LobbyListItem : MonoBehaviour
{
    public CSteamID lobbySteamID;
    public string lobbyName;
    public Text lobbyNameTxt;
    public Button joinButton;

    private void Awake()
    {
        joinButton.onClick.AddListener(JoinLobby);
    }

    public void SetLobbyData()
    {
        if (lobbyName.Equals(""))
            lobbyNameTxt.text = "Empty";
        else
            lobbyNameTxt.text = lobbyName;
    }

    public void JoinLobby()
    {
        SteamLobby.Instance.JoinLobby(lobbySteamID);
    }
}