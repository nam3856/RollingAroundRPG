using Photon.Pun;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 캐릭터 클래스 타입을 정의합니다.
/// </summary>
public enum CharacterClassType
{
    Warrior = 0,
    Gunner = 1,
    Mage = 2
}

/// <summary>
/// 플레이어 데이터를 임시로 저장하는 클래스입니다.
/// </summary>
public static class DataHolder
{
    public static int SelectedCharacterIndex;
    public static string NickName;
}

/// <summary>
/// 로딩 및 캐릭터 선택을 관리하는 매니저 클래스입니다.
/// </summary>
public class LoadingManager : MonoBehaviourPunCallbacks
{
    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_InputField nickNameInput;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject warriorPrefab;
    [SerializeField] private GameObject gunnerPrefab;
    [SerializeField] private GameObject magePrefab;
    [SerializeField] private GameObject selectIndicator;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private GameObject nicknameErrorIndicator;
    #endregion

    #region Character Selection Settings
    [Header("Character Selection Settings")]
    [SerializeField] private float selectionForce = 1000f; // 선택 인디케이터에 가하는 힘
    [SerializeField]
    private Vector3[] selectionPositions = new Vector3[3]
    {
        new Vector3(-236, -34, 0), // 전사 위치
        new Vector3(0, -40, 0),     // 거너 위치
        new Vector3(240, -40, 0)    // 마법사 위치
    };
    #endregion

    #region Private Variables
    private int selectedIndex = 0;
    #endregion

    #region Unity Methods
    private void Start()
    {
        InitializeUI();
        ConfigurePhotonSettings();
        CheckPlayerData();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectIndicator.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            HandleMoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectIndicator.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            HandleMoveRight();
        }
    }
    #endregion



    #region Initialization Methods

    /// <summary>
    /// UI 초기화.
    /// </summary>
    private void InitializeUI()
    {
        characterSelectPanel.SetActive(false);
    }

    /// <summary>
    /// Photon 설정 초기화.
    /// </summary>
    private void ConfigurePhotonSettings()
    {
        // 화면 해상도 설정
        Screen.SetResolution(1920, 1080, false);

        // Photon 전송 설정
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }
    #endregion

    #region Data Handling
    /// <summary>
    /// 플레이어 데이터 존재 여부 확인 및 초기화.
    /// </summary>
    private void CheckPlayerData()
    {
        if (SaveSystem.PlayerDataExists())
        {
            loadingText.text = "플레이어 데이터를 찾았습니다";
            ConnectAndLoadMainScene();
        }
        else
        {
            loadingText.text = "플레이어 데이터를 찾지 못했습니다";
            ShowCharacterSelectionPanel();
        }
    }

    /// <summary>
    /// 저장된 플레이어 데이터를 로드하고 메인 씬으로 접속.
    /// </summary>
    private void ConnectAndLoadMainScene()
    {
        loadingText.text = "세이브 데이터 불러오는 중";
        PlayerData data = SaveSystem.LoadPlayerData();
        DataHolder.NickName = data.NickName;
        DataHolder.SelectedCharacterIndex = data.CharacterClassIndex;
        loadingText.text = "접속중";
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// 닉네임 입력 후 Photon 접속.
    /// </summary>
    public void Connect()
    {
        string nickname = nickNameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            ShowNicknameError();
            return;
        }

        DataHolder.SelectedCharacterIndex = selectedIndex;
        DataHolder.NickName = nickname;
        characterSelectPanel.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// 닉네임 입력 오류 표시.
    /// </summary>
    private void ShowNicknameError()
    {
        errorPanel.SetActive(true);
        nicknameErrorIndicator.GetComponent<Rigidbody2D>().AddForce(Vector2.right * selectionForce, ForceMode2D.Impulse);
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        loadingText.text = "열려있는 방을 찾는 중";
        PhotonNetwork.JoinOrCreateRoom("Room", new Photon.Realtime.RoomOptions { MaxPlayers = 6 }, null);
    }

    public override void OnJoinedRoom()
    {
        loadingText.text = "게임에 입장 중";
        PhotonNetwork.IsMessageQueueRunning = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("MainScene");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PhotonNetwork.IsMessageQueueRunning = true;
    }
    #endregion

    #region Character Selection
    /// <summary>
    /// 캐릭터 선택 패널 표시 및 초기 애니메이션 트리거.
    /// </summary>
    private void ShowCharacterSelectionPanel()
    {
        loadingText.text = "";
        characterSelectPanel.SetActive(true);
        warriorPrefab.GetComponent<Animator>().SetTrigger("warrior init");
        gunnerPrefab.GetComponent<Animator>().SetTrigger("gunner init");
        magePrefab.GetComponent<Animator>().SetTrigger("mage init");
    }

    /// <summary>
    /// 전사 캐릭터 선택.
    /// </summary>
    public void SelectWarrior()
    {
        selectedIndex = (int)CharacterClassType.Warrior;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// 거너 캐릭터 선택.
    /// </summary>
    public void SelectGunner()
    {
        selectedIndex = (int)CharacterClassType.Gunner;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// 마법사 캐릭터 선택.
    /// </summary>
    public void SelectMage()
    {
        selectedIndex = (int)CharacterClassType.Mage;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// 왼쪽으로 이동할 때 호출되는 핸들러.
    /// </summary>
    private void HandleMoveLeft()
    {
        if (selectedIndex <= 0)
        {
            selectIndicator.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 1000f, ForceMode2D.Impulse);
            return;
        }

        selectedIndex--;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// 오른쪽으로 이동할 때 호출되는 핸들러.
    /// </summary>
    private void HandleMoveRight()
    {
        if (selectedIndex >= selectionPositions.Length - 1) {
            selectIndicator.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 1000f, ForceMode2D.Impulse);
            return; 
        }

        selectedIndex++;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// 선택 인디케이터 위치 업데이트.
    /// </summary>
    private void UpdateSelectionIndicator()
    {
        selectIndicator.SetActive(true);
        Rigidbody2D rb = selectIndicator.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;

        Vector3 targetPosition = selectionPositions[selectedIndex];
        selectIndicator.transform.localPosition = targetPosition;
    }
    #endregion
}
