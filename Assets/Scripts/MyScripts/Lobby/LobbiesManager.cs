using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class LobbiesManager : MonoBehaviour
{
    public static LobbiesManager Instance;

    public GameObject lobbiesMenu;
    public GameObject lobbyListItemPrefab;
    public Transform lobbyListContent;
    
    public Button backButton;

    public List<LobbyListItem> lobbiesList = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        
        backButton.onClick.AddListener(BackToMainMenu);
    }

    private void BackToMainMenu()
    {
        SteamLobby.Instance.hostButton.gameObject.SetActive(true);
        SteamLobby.Instance.lobbiesButton.gameObject.SetActive(true);
        lobbiesMenu.SetActive(false);
    }

    public void DisplayLobbiesList(List<CSteamID> lobbiesIDs, LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbiesIDs.Count; i++)
        {
            if (lobbiesIDs[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject lobbyItem = Instantiate(lobbyListItemPrefab, lobbyListContent);
                lobbyItem.transform.localScale = Vector3.one;

                LobbyListItem lobbyEntry = lobbyItem.GetComponent<LobbyListItem>();
                lobbyEntry.lobbySteamID = (CSteamID)lobbiesIDs[i].m_SteamID;
                lobbyEntry.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbiesIDs[i].m_SteamID, "LobbyName");
                lobbyEntry.SetLobbyData();

                lobbiesList.Add(lobbyEntry);
            }
        }
    }

    public void DestroyAllLobbies()
    {
        foreach (var item in lobbiesList)
        {
            Destroy(item.gameObject);
        }

        lobbiesList.Clear();
    }
}