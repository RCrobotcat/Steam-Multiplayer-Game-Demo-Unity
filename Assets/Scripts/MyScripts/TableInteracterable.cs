using Mirror;
using UnityEngine;

public class TableInteracterable : NetworkBehaviour
{
    private ItemsManager itemsManager;
    public string desiredItem = "";

    [SyncVar(hook = nameof(OnTableItemChanged))]
    private string tableItem;

    private void Start()
    {
        itemsManager = FindObjectOfType<ItemsManager>();
        if (isServer)
        {
            tableItem = desiredItem;
        }

        OnTableItemChanged(null, tableItem);
    }

    private void Update()
    {
        PlayerMovement[] allPlayers = FindObjectsOfType<PlayerMovement>();
        foreach (var player in allPlayers)
        {
            GameObject playerObj = player.gameObject;

            if (playerObj && player.isLocalPlayer && player.ObjPlayerIsNear == gameObject)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CmdInteractWithTable(playerObj);
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdInteractWithTable(GameObject player)
    {
        if (tableItem.Equals(desiredItem)
            && player.GetComponent<PlayerMovement>().currentEquippedItem == "")
        {
            player.GetComponent<PlayerMovement>().currentEquippedItem = tableItem;

            tableItem = "";
        }
        else if (tableItem == "" &&
                 player.GetComponent<PlayerMovement>().currentEquippedItem == desiredItem)
        {
            tableItem = player.GetComponent<PlayerMovement>().currentEquippedItem;

            player.GetComponent<PlayerMovement>().currentEquippedItem = "";
        }
    }

    private void OnTableItemChanged(string oldTableItem, string newTableItem)
    {
        if (itemsManager)
        {
            foreach (Transform item in transform.Find("InterestParent").transform)
            {
                Destroy(item.gameObject);
            }

            if (newTableItem != "")
            {
                Transform newObj = Instantiate(itemsManager.items.transform.Find(newTableItem),
                    transform.Find("InterestParent").transform);
                newObj.transform.name = newTableItem;
                newObj.transform.localScale = Vector3.one * 0.6f;
                newObj.gameObject.SetActive(true);
                // NetworkServer.Spawn(newObj.gameObject);
            }
        }
    }
}