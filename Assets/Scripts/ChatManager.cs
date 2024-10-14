using UnityEngine;
using UnityEngine.UI;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using UnityEngine.Events;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public TMP_InputField inputField;
    public TextMeshProUGUI chatDisplay;
    private ChatClient chatClient;
    private string userName;
    public ScrollRect scrollRect;
    public TextMeshProUGUI chatText;
    public GameObject inp;
    public GameObject scr;

    public void SetNicknameAndConnect(string nickName)
    {
        userName = nickName;
        ConnectToChat();
        inp.SetActive(true);
        scr.SetActive(true);
    }
    void ConnectToChat()
    {
        chatClient = new ChatClient(this);
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
                           "1.0", new Photon.Chat.AuthenticationValues(userName));
    }

    void Update()
    {
        if (chatClient != null)
        {
            chatClient.Service(); // ä�� ���� ������Ʈ
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (inputField.isActiveAndEnabled && !string.IsNullOrEmpty(inputField.text))
            {
                SendChatMessage(inputField.text);
            }
            else
            {
                if (!inputField.isActiveAndEnabled) 
                { 
                    Debug.Log("inputField is not focused");
                    inputField.ActivateInputField();
                }
                else if (string.IsNullOrEmpty(inputField.text)) 
                {
                    Debug.Log("inputField is empty");
                    if(inputField.isActiveAndEnabled)
                        inputField.DeactivateInputField();
                    else inputField.ActivateInputField();
                }
            }
        }
    }

    public void SendChatMessage(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log(message);
            chatClient.PublishMessage("GlobalChat", message);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    // IChatClientListener �������̽� ����
    public void OnConnected()
    {
        Debug.Log("Chat Connected");
        chatClient.Subscribe(new string[] { "GlobalChat" });
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat Disconnected");
    }

    public void OnChatStateChange(ChatState state)
    {
        // ���� ���� �� ȣ��˴ϴ�.
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            string sender = senders[i];
            string message = messages[i].ToString();
            string formattedMessage = $"\n<color=yellow>{sender}</color>: {message}";
            chatText.text += formattedMessage;
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        // ���� �޽��� ���� ó��
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to channel(s): " + string.Join(", ", channels));
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log("Unsubscribed from channel(s): " + string.Join(", ", channels));
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        // ���� ������Ʈ ó��
    }

    public void OnUserSubscribed(string channel, string user)
    {
        // ����ڰ� ä�ο� �������� �� ȣ��˴ϴ�.
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        // ����ڰ� ä�ο��� ������ �� ȣ��˴ϴ�.
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }
}
