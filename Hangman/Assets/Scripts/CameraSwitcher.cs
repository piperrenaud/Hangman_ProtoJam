using Unity.Netcode;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera hostCamera;
    public Camera playerCamera;

    public void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            hostCamera.gameObject.SetActive(true);
            playerCamera.gameObject.SetActive(false);
        }
        else
        {
            hostCamera.gameObject.SetActive(false);
            playerCamera.gameObject.SetActive(true);
        }
    }
}
