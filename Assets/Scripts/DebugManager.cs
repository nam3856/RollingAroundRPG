using Photon.Pun;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    private string debugLog = ""; // 로그를 저장할 변수
    private Vector2 scrollPosition; // 스크롤 위치
    private MyNetworkManager myNetworkManager;

    private void Start()
    {
        myNetworkManager = FindObjectOfType<MyNetworkManager>();
        //PhotonNetwork.LogLevel = PunLogLevel.Full;
    }
    // 디버그 메시지를 업데이트하는 메서드
    public void AddDebugLog(string message)
    {
        debugLog += message + "\n"; // 로그를 추가하고 줄바꿈
    }

    void OnGUI()
    {
        // 박스의 크기를 설정
        //Rect boxRect = new Rect(10, 10, 500, 300); // (x, y, width, height)

        // 배경 박스를 먼저 그림 (검은색)
        //GUI.color = Color.black; // 박스 색상 설정
        //GUI.Box(boxRect, GUIContent.none); // 빈 박스 그리기

        // 박스 안에 흰색 텍스트를 출력
        //GUI.color = Color.white; // 텍스트 색상 설정
        //scrollPosition = GUI.BeginScrollView(new Rect(10, 10, 500, 300), scrollPosition, new Rect(0, 0, 480, 1000));
        //GUI.Label(new Rect(0, 0, 480, 1000), debugLog); // 텍스트를 그리는 위치와 크기
        //GUI.EndScrollView();
        //if (GUI.Button(new Rect(500, 0, 400, 200), "소환"))
            //myNetworkManager.MonsterSpawn();
    }
}
