using Mono.Cecil.Cil;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class RelayJoin : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField codeInput;
    public TextMeshProUGUI statusText;
    public TMP_InputField nameInput;

    public string gameSceneName = "Multi";

    private void Start()
    {
        if (codeInput != null) codeInput.onEndEdit.AddListener(OnInputSubmitted);
    }

    void OnInputSubmitted(string code)
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            JoinRelay(code, nameInput.text.Trim());
        }
        else
        {
            statusText.text = "Please enter your name!";
        }
    }


    public async void JoinRelay(string joinCode, string playerName)
    {
        if (string.IsNullOrEmpty(joinCode)) return;

        try
        {
            JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var unityTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            if (unityTransport == null)
            {
                unityTransport = NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
            }

            unityTransport.SetRelayServerData(
            alloc.RelayServer.IpV4,
            (ushort)alloc.RelayServer.Port,
            alloc.AllocationIdBytes,
            alloc.Key,
            alloc.ConnectionData,
            alloc.HostConnectionData);

            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                if (clientId == NetworkManager.Singleton.LocalClientId)
                {
                    var playerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var pd = playerObj.GetComponent<PlayerData>();
                    pd.SetPlayerNameServerRpc(nameInput.text.Trim());

                    NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
            };

            NetworkManager.Singleton.StartClient();
            if (statusText != null) statusText.text = "Joining game...";

            Debug.Log("Joining Relay Server");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed To join relay server: " + ex.Message);
            if (statusText != null) statusText.text = "Failed to join: " + ex.Message;
        }
    }
}
