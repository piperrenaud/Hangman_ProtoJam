using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerData : NetworkBehaviour
{
    [Header("Player Info")]
    public NetworkVariable<int> Lives = new NetworkVariable<int>(5);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    public string playerName;

    public event Action<int> OnLivesChanged;
    public event Action<int> OnScoreChanged;

    void Update()
    {
        if (IsLocalPlayer)
        {
            OnLivesChanged?.Invoke(Lives.Value);
            OnScoreChanged?.Invoke(Score.Value);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer && string.IsNullOrEmpty(playerName))
        {
            playerName = "Player" + OwnerClientId;
        }
    }

    public void LoseLife()
    {
        if (!IsServer) return;
        Lives.Value--;
    }

    public void AddPoint()
    {
        if (!IsServer) return;
        Score.Value++;
    }
}
