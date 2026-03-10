using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerListManager : NetworkBehaviour
{
    [Header("UI")]
    public Transform contentParent;
    public GameObject playerNamePrefab;

    private Dictionary<ulong, GameObject> playerNameEntries = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var clientPair in NetworkManager.Singleton.ConnectedClients)
            {
                AddPlayer(clientPair.Key);
            }

            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayer;
        }
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayer;
        }
    }

    private void AddPlayer(ulong clientId)
    {
        if (!IsServer) return;

        PlayerData pd = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).GetComponent<PlayerData>();
        if (pd != null)
        {
            pd.playerName.OnValueChanged += (oldName, newName) =>
            {
                AddPlayerClientRpc(clientId, newName.ToString());
            };

            if (!string.IsNullOrEmpty(pd.playerName.Value.ToString()))
            {
                AddPlayerClientRpc(clientId, pd.playerName.Value.ToString());
            }
        }
    }

    [ClientRpc]
    private void AddPlayerClientRpc(ulong clientId, string playerName)
    {
        if (playerNameEntries.ContainsKey(clientId)) return;

        GameObject entry = Instantiate(playerNamePrefab, contentParent);
        TMP_Text tmpText = entry.GetComponent<TMP_Text>();
        if (tmpText != null) tmpText.text = playerName;

        playerNameEntries.Add(clientId, entry);
    }
}