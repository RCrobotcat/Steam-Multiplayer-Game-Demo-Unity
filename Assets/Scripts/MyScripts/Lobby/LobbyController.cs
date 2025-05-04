using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    public Text lobbyNameText;
    public GameObject playerListViewContent;
    public GameObject playerListItemPrefab;
    public GameObject localPlayerObject;

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    public List<PlayerListItem> PlayerListItems = new();
    public PlayerObjectController LocalPlayerObjectController;

    MyNetworkManager _myNetworkManager;

    public Button readyBtn;
    public Text readyBtnText;
    public Button startGameBtn;

    public GameObject lobbyCanvas;

    private MyNetworkManager MyNetworkManager
    {
        get
        {
            if (_myNetworkManager != null)
            {
                return _myNetworkManager;
            }

            return _myNetworkManager = MyNetworkManager.singleton as MyNetworkManager;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        readyBtn.onClick.AddListener(ReadyPlayer);
        startGameBtn.onClick.AddListener(StartGame);

        SteamLobby steamLobby = FindObjectOfType<SteamLobby>();
        if (steamLobby != null)
        {
            if (steamLobby.lobbySceneType == LobbySceneType.GameLobby)
            {
                lobbyCanvas.SetActive(true);
            }
            else
            {
                lobbyCanvas.SetActive(false);
            }
        }
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = MyNetworkManager.GetComponent<SteamLobby>().currentLobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "LobbyName");
    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated)
            CreateHostPlayerItem(); // Host

        if (PlayerListItems.Count < MyNetworkManager.GamePlayers.Count)
            CreateClientPlayerItem();

        if (PlayerListItems.Count > MyNetworkManager.GamePlayers.Count)
            RemovePlayerItem();

        if (PlayerListItems.Count == MyNetworkManager.GamePlayers.Count)
            UpdatePlayerItem();
    }

    public void FindLocalPlayer()
    {
        localPlayerObject = GameObject.Find("LocalPlayer");
        LocalPlayerObjectController = localPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in MyNetworkManager.GamePlayers)
        {
            GameObject newPlayerItem = Instantiate(playerListItemPrefab);
            PlayerListItem newPlayerListItem = newPlayerItem.GetComponent<PlayerListItem>();

            newPlayerListItem.playerName = player.playerName;
            newPlayerListItem.playerSteamID = player.playerSteamID;
            newPlayerListItem.connectionID = player.connectionID;
            newPlayerListItem.isReady = player.isReady;
            newPlayerListItem.SetPlayerValues();

            newPlayerListItem.transform.SetParent(playerListViewContent.transform);
            newPlayerListItem.transform.localScale = Vector3.one;
            newPlayerListItem.transform.localPosition = Vector3.zero;

            PlayerListItems.Add(newPlayerListItem);
        }

        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in MyNetworkManager.GamePlayers)
        {
            if (PlayerListItems.All(b => b.connectionID != player.connectionID))
            {
                GameObject newPlayerItem = Instantiate(playerListItemPrefab);
                PlayerListItem newPlayerListItem = newPlayerItem.GetComponent<PlayerListItem>();

                newPlayerListItem.playerName = player.playerName;
                newPlayerListItem.playerSteamID = player.playerSteamID;
                newPlayerListItem.connectionID = player.connectionID;
                newPlayerListItem.isReady = player.isReady;
                newPlayerListItem.SetPlayerValues();

                newPlayerListItem.transform.SetParent(playerListViewContent.transform);
                newPlayerListItem.transform.localScale = Vector3.one;
                newPlayerListItem.transform.localPosition = Vector3.zero;

                PlayerListItems.Add(newPlayerListItem);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in MyNetworkManager.GamePlayers)
        {
            foreach (PlayerListItem item in PlayerListItems)
            {
                if (item.connectionID == player.connectionID)
                {
                    item.playerName = player.playerName;
                    item.isReady = player.isReady;
                    item.SetPlayerValues();
                    if (player == LocalPlayerObjectController)
                        UpdateReadyBtn();
                }
            }
        }

        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new();
        foreach (PlayerListItem item in PlayerListItems)
        {
            if (MyNetworkManager.GamePlayers.All(b => b.connectionID != item.connectionID))
            {
                playerListItemToRemove.Add(item);
            }
        }

        if (playerListItemToRemove.Count > 0)
        {
            foreach (PlayerListItem removeItem in playerListItemToRemove)
            {
                GameObject objectToRemove = removeItem.gameObject;
                PlayerListItems.Remove(removeItem);
                Destroy(objectToRemove);
            }
        }
    }

    public void ReadyPlayer()
    {
        LocalPlayerObjectController.ChangeReadyStatus();
    }

    public void StartGame()
    {
        string scenePath = SceneManager.GetSceneByName("Scene_1").name;
        lobbyCanvas.SetActive(false);
        MyNetworkManager.HandleSendPlayerToNewScene(scenePath, "SpawnPos");
    }

    public void UpdateReadyBtn()
    {
        if (LocalPlayerObjectController.isReady)
        {
            readyBtnText.text = "Unready";
            readyBtnText.color = Color.red;
        }
        else
        {
            readyBtnText.text = "Ready";
            readyBtnText.color = Color.green;
        }
    }

    public void CheckIfAllReady()
    {
        bool AllReady = false;
        foreach (PlayerObjectController player in MyNetworkManager.GamePlayers)
        {
            if (player.isReady)
            {
                AllReady = true;
            }
            else
            {
                AllReady = false;
                break;
            }
        }

        if (AllReady)
        {
            if (LocalPlayerObjectController.playerID == 1) // Host
            {
                startGameBtn.interactable = true;
            }
            else
            {
                startGameBtn.interactable = false;
            }
        }
        else
        {
            startGameBtn.interactable = false;
        }
    }
}