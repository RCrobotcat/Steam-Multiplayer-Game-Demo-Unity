using System;
using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerObjectController : NetworkBehaviour
{
    // Player Data
    [SyncVar] public int connectionID;
    [SyncVar] public int playerID;
    [SyncVar] public ulong playerSteamID;

    [SyncVar(hook = nameof(OnPlayerNameUpdated))]
    public string playerName;

    [SyncVar(hook = nameof(OnReadyStatusChanged))]
    public bool isReady;

    private MyNetworkManager _myNetworkManager;

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

    public override void OnStartAuthority()
    {
        SetPlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalPlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        MyNetworkManager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        MyNetworkManager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    void SetPlayerName(string playerName)
    {
        OnPlayerNameUpdated(this.playerName, playerName);
    }

    void OnPlayerNameUpdated(string oldName, string newName)
    {
        if (isServer) // Host
        {
            playerName = newName;
        }

        if (isClient) // Client
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    void OnReadyStatusChanged(bool oldVal, bool newVal)
    {
        if (isServer) // Host
        {
            isReady = newVal;
        }

        if (isClient) // Client
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    public void CmdSetReadyStatus()
    {
        OnReadyStatusChanged(isReady, !isReady);
    }

    public void ChangeReadyStatus()
    {
        if (isOwned)
        {
            CmdSetReadyStatus();
        }
    }
}