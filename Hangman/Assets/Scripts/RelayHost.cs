using UnityEngine;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Diagnostics;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class RelayHost : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI joinCodeText;

    public string gameSceneName = "Main";

    private const int MAX_PLAYERS = 10;
    
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        System.Diagnostics.Debug.WriteLine("Signed in as: " + AuthenticationService.Instance.PlayerId);
    }
    public async void HostRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            System.Diagnostics.Debug.WriteLine("Join Code: " + joinCode);

            if (joinCodeText != null) joinCodeText.text = joinCode;

            var unityTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            if (unityTransport == null)
            {
                unityTransport = NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = unityTransport;
            }

            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
            System.Diagnostics.Debug.WriteLine("Hosting Relay server...");
        }
        catch (System.Exception e)
        {
            System.Diagnostics.Debug.WriteLine("Failed to host relay server: " +  e.Message);
        }
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
