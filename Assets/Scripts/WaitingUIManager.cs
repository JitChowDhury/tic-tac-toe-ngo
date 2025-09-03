using Unity.Netcode;
using UnityEngine;

public class WaitingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject waitingUI;

    private void Start()
    {
        // Show the UI initially
        waitingUI.SetActive(true);

        // Listen for new players joining
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Host waits until at least 2 players
        if (NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                waitingUI.SetActive(false);
            }
        }
        else
        {
            // For client, just hide once they are connected
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                waitingUI.SetActive(false);
            }
        }
    }
}
