using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Cinemachine;

public class MyNetworkManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;
    public TMP_Dropdown dropdown;
    public ChatManager ChatManager;
    private int selected;

    private void Start()
    {
        RespawnPanel.SetActive(false);
        selected = DataHolder.SelectedCharacterIndex;
        PhotonNetwork.LocalPlayer.NickName = DataHolder.NickName;
        ChatManager.SetNicknameAndConnect(PhotonNetwork.LocalPlayer.NickName);
        /*
        if (SaveSystem.PlayerDataExists())
        {
            Connect();
        }
        else
        {
            DisconnectPanel.SetActive(true);
        }
        */
        if (PhotonNetwork.InRoom)
        {
            Spawn(selected);
        }
        else
        {
            // �濡 �������� ���� ���, OnJoinedRoom �ݹ鿡�� Spawn�� ȣ���ϵ��� ����
            PhotonNetwork.AddCallbackTarget(this);
        }
    }

    public override void OnJoinedRoom()
    {
        Spawn(selected);
    }
    private void Update()
    {
    }

    public void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("Title");
    }
    public void Spawn(int sel)
    {
        RespawnPanel.SetActive(false);
        Vector3 spawnPosition = Vector3.zero;

        if (SaveSystem.PlayerDataExists())
        {
            PlayerData data = SaveSystem.LoadPlayerData();
            spawnPosition = data.LastPosition.ToVector3();
            PolygonCollider2D confiner = PlayerScript.FindConfinerByID(data.currentCMRangeId);
            if (confiner != null)
            {
                CinemachineVirtualCamera mainCamera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
                CinemachineConfiner confinerComponent = mainCamera.GetComponent<CinemachineConfiner>();
                if (confinerComponent != null)
                {
                    confinerComponent.m_BoundingShape2D = confiner;
                }
                else
                {
                    Debug.LogError("CinemachineConfiner�� ã�� �� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError("�ش� confinerID�� ���� confiner�� ã�� �� �����ϴ�.");
            }
        }

        object[] instantiationData = new object[] { sel };
        PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity, 0, instantiationData);

    }

    public void Respawn()
    {
        RespawnPanel.SetActive(false);
    }

    public void MonsterSpawn()
    {
        PhotonNetwork.Instantiate("BossSlime", new Vector3(0, 0, 0), Quaternion.identity);
    }
}
