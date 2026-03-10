using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Core.Environments;

public class RelayJoin : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField codeInput;
    public TextMeshProUGUI statusText;
    public TMP_InputField nameInput;

    public string gameSceneName = "Multi";

    private async void Start()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync(
                new InitializationOptions().SetEnvironmentName("production"));
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);
    }

    public void JoinButtonPressed() 
    { 
        string code = codeInput.text.Trim(); 
        string playerName = nameInput.text.Trim(); 
        if (string.IsNullOrEmpty(code)) 
        { 
            statusText.text = "Enter join code!"; 
            return; 
        } 
        
        if (string.IsNullOrEmpty(playerName)) 
        { 
            statusText.text = "Enter your name!"; 
            return; 
        } 
        
        JoinRelay(code, playerName); 
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

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

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
            if (statusText != null) statusText.text = "Failed to join: " + ex;
        }
    }
}