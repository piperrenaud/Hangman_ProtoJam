using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

public class PlayerData : NetworkBehaviour
{
    [Header("Player Info")]
    public NetworkVariable<int> Lives = new NetworkVariable<int>(5);
    public NetworkVariable<int> Score = new NetworkVariable<int>(0);
    
    public NetworkVariable<FixedString128Bytes> playerName = new NetworkVariable<FixedString128Bytes>("");

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

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerNameServerRpc(string name)
    {
        if (!IsServer) return;
        playerName.Value = name;
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
