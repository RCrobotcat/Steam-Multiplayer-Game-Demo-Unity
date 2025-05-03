using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListItem : MonoBehaviour
{
    public string playerName;
    public ulong playerSteamID;
    public int connectionID;

    bool avatarReceived = false;

    public Text playerNameTxt;
    public RawImage playerIcon;

    public Text readyTxt;
    public bool isReady = false;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    private void Start()
    {
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == playerSteamID)
        {
            playerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else // Another Player
        {
            return;
        }
    }

    void GetPlayerIcon()
    {
        int imageID = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamID);
        if (imageID == -1) return;
        playerIcon.texture = GetSteamImageAsTexture(imageID);
    }

    public void SetPlayerValues()
    {
        playerNameTxt.text = playerName;
        ChangeReadyStatus();
        if (!avatarReceived)
        {
            GetPlayerIcon();
        }
    }

    public void ChangeReadyStatus()
    {
        if (isReady)
        {
            readyTxt.text = "Ready";
            readyTxt.color = Color.green;
        }
        else
        {
            readyTxt.text = "Unready";
            readyTxt.color = Color.red;
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        avatarReceived = true;
        return texture;
    }
}