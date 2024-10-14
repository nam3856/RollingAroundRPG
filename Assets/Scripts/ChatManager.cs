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
            chatClient.Service(); // 채팅 서비스 업데이트
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

    // IChatClientListener 인터페이스 구현
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
        // 상태 변경 시 호출됩니다.
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
        // 개인 메시지 수신 처리
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
        // 상태 업데이트 처리
    }

    public void OnUserSubscribed(string channel, string user)
    {
        // 사용자가 채널에 가입했을 때 호출됩니다.
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        // 사용자가 채널에서 나갔을 때 호출됩니다.
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log(message);
    }
}
