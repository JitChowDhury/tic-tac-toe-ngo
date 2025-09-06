using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentRoot;        // ScrollView/Viewport/Content
    [SerializeField] private GameObject messageItemPrefab; // the MessageItem prefab

    [Header("Settings")]
    [SerializeField] private int maxMessages = 50;

    private readonly Queue<GameObject> _items = new Queue<GameObject>();

    private void Awake()
    {
        if (sendButton) sendButton.onClick.AddListener(OnClickSend);
    }

    public override void OnNetworkSpawn()
    {
        bool canType = IsClient; // clients & host can type
        if (inputField) inputField.interactable = canType;
        if (sendButton) sendButton.interactable = canType;
    }

    private void Update()
    {
        if (!inputField) return;

        // Press Enter to send (Shift+Enter to add newline is disabled by Single Line)
        if (inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
        {
            OnClickSend();
        }
    }

    private void OnClickSend()
    {
        if (!inputField) return;

        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        inputField.text = string.Empty;
        inputField.ActivateInputField();

        // Send to server; allow clients without ownership
        SendMessageServerRpc(msg);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServerRpc(string message, ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;

        // Basic safety/sanitization
        if (message.Length > 256) message = message.Substring(0, 256);

        // Broadcast to everyone
        ReceiveMessageClientRpc(message, sender);
    }

    [ClientRpc]
    private void ReceiveMessageClientRpc(string message, ulong senderId)
    {
        AddMessageToUI(senderId, message);
    }

    private void AddMessageToUI(ulong senderId, string message)
    {
        if (!messageItemPrefab || !contentRoot) return;

        var go = Instantiate(messageItemPrefab, contentRoot);
        var label = go.GetComponent<TMP_Text>();

        if (label)
        {
            string who = (NetworkManager.Singleton != null &&
                          senderId == NetworkManager.Singleton.LocalClientId)
                         ? "You"
                         : $"Player {senderId}";
            label.text = $"<b>{who}:</b> {message}";
        }

        _items.Enqueue(go);
        if (_items.Count > maxMessages)
        {
            var oldest = _items.Dequeue();
            Destroy(oldest);
        }

        // Scroll to bottom
        if (scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}
