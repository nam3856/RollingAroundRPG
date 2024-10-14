using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public static class DataHolder
{
    public static int selected;
    public static string nickName;
}
public class LoadingManager : MonoBehaviourPunCallbacks
{
    public TMP_Text loadingText;
    public TMP_InputField nickNameInput;
    public GameObject characterSelectPanel;
    public GameObject warrior;
    public GameObject gunner;
    public GameObject mage;
    public GameObject select;
    public GameObject error;
    public GameObject nicknameI;
    private int sel = 0;

    private void Start()
    {
        characterSelectPanel.SetActive(false);
        Screen.SetResolution(1920, 1080, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        loadingText.text = "�÷��̾� ������ ã����";
         //���̺� ������ Ȯ��
        if (SaveSystem.PlayerDataExists())
        {
            loadingText.text = "�÷��̾� �����͸� ã�ҽ��ϴ�";
            ConnectAndLoadMainScene();
        }
        else
        {

            loadingText.text = "�÷��̾� �����͸� ã�� ���߽��ϴ�";
        characterSelectPanel.SetActive(true);
        ShowCharacterSelectionPanel();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            select.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            sel--;
            Debug.Log(sel);
            if (sel < 0)
            {
                sel = 0;
                select.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 1000f, ForceMode2D.Impulse);
            }
            else if(sel == 1)
            {
                select.transform.localPosition = new Vector3(0, -40, 0);
            }
            else
            {
                select.transform.localPosition = new Vector3(-236, -34, 0);
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            select.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            sel++;

            Debug.Log(sel);
            if (sel > 2)
            {
                sel = 2;
                select.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 1000f, ForceMode2D.Impulse);
            }
            else if (sel == 1)
            {
                select.transform.localPosition = new Vector3(0, -40, 0);
            }
            else
            {
                select.transform.localPosition = new Vector3(240, -40, 0);
            }
        }
    }

    void ConnectAndLoadMainScene()
    {
        loadingText.text = "���̺� ������ �ҷ����� ��";
        PlayerData data = SaveSystem.LoadPlayerData();
        DataHolder.nickName = data.NickName;
        DataHolder.selected = data.CharacterClassIndex;
        loadingText.text = "������";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        if (string.IsNullOrEmpty(nickNameInput.text))
        {
            error.SetActive(true);
            nicknameI.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 1000f, ForceMode2D.Impulse);
            return;
        }
        characterSelectPanel.SetActive(false);
        DataHolder.selected = sel;
        DataHolder.nickName = nickNameInput.text;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        loadingText.text = "�����ִ� ���� ã�� ��";
        PhotonNetwork.JoinOrCreateRoom("Room", new Photon.Realtime.RoomOptions { MaxPlayers = 6 }, null);
    }

    public override void OnJoinedRoom()
    {
        loadingText.text = "���ӿ� ���� ��";
        PhotonNetwork.IsMessageQueueRunning = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("MainScene");
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        PhotonNetwork.IsMessageQueueRunning = true;
    }
    void ShowCharacterSelectionPanel()
    {
        loadingText.text = "";
        characterSelectPanel.SetActive(true);
        warrior.GetComponent<Animator>().SetTrigger("warrior init");
        gunner.GetComponent<Animator>().SetTrigger("gunner init");
        mage.GetComponent<Animator>().SetTrigger("mage init");

    }

    public void SelectWarrior()
    {
        sel = 0;
        select.SetActive(true);
        select.transform.localPosition= new Vector3(-236, -34, 0);
    }
    public void SelectGunner()
    {
        sel = 1;
        select.SetActive(true);
        select.transform.localPosition = new Vector3(0, -40, 0);
    }
    public void SelectMage()
    {
        sel = 2;
        Debug.Log(sel);
        select.SetActive(true);
        select.transform.localPosition = new Vector3(240, -40, 0);
    }
}
