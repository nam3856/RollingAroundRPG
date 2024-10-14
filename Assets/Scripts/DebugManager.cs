using Photon.Pun;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    private string debugLog = ""; // �α׸� ������ ����
    private Vector2 scrollPosition; // ��ũ�� ��ġ
    private MyNetworkManager myNetworkManager;

    private void Start()
    {
        myNetworkManager = FindObjectOfType<MyNetworkManager>();
        //PhotonNetwork.LogLevel = PunLogLevel.Full;
    }
    // ����� �޽����� ������Ʈ�ϴ� �޼���
    public void AddDebugLog(string message)
    {
        debugLog += message + "\n"; // �α׸� �߰��ϰ� �ٹٲ�
    }

    void OnGUI()
    {
        // �ڽ��� ũ�⸦ ����
        //Rect boxRect = new Rect(10, 10, 500, 300); // (x, y, width, height)

        // ��� �ڽ��� ���� �׸� (������)
        //GUI.color = Color.black; // �ڽ� ���� ����
        //GUI.Box(boxRect, GUIContent.none); // �� �ڽ� �׸���

        // �ڽ� �ȿ� ��� �ؽ�Ʈ�� ���
        //GUI.color = Color.white; // �ؽ�Ʈ ���� ����
        //scrollPosition = GUI.BeginScrollView(new Rect(10, 10, 500, 300), scrollPosition, new Rect(0, 0, 480, 1000));
        //GUI.Label(new Rect(0, 0, 480, 1000), debugLog); // �ؽ�Ʈ�� �׸��� ��ġ�� ũ��
        //GUI.EndScrollView();
        //if (GUI.Button(new Rect(500, 0, 400, 200), "��ȯ"))
            //myNetworkManager.MonsterSpawn();
    }
}
