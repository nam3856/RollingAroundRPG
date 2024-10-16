using Photon.Pun;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ĳ���� Ŭ���� Ÿ���� �����մϴ�.
/// </summary>
public enum CharacterClassType
{
    Warrior = 0,
    Gunner = 1,
    Mage = 2
}

/// <summary>
/// �÷��̾� �����͸� �ӽ÷� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public static class DataHolder
{
    public static int SelectedCharacterIndex;
    public static string NickName;
}

/// <summary>
/// �ε� �� ĳ���� ������ �����ϴ� �Ŵ��� Ŭ�����Դϴ�.
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
    [SerializeField] private float selectionForce = 1000f; // ���� �ε������Ϳ� ���ϴ� ��
    [SerializeField]
    private Vector3[] selectionPositions = new Vector3[3]
    {
        new Vector3(-236, -34, 0), // ���� ��ġ
        new Vector3(0, -40, 0),     // �ų� ��ġ
        new Vector3(240, -40, 0)    // ������ ��ġ
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
    /// UI �ʱ�ȭ.
    /// </summary>
    private void InitializeUI()
    {
        characterSelectPanel.SetActive(false);
    }

    /// <summary>
    /// Photon ���� �ʱ�ȭ.
    /// </summary>
    private void ConfigurePhotonSettings()
    {
        // ȭ�� �ػ� ����
        Screen.SetResolution(1920, 1080, false);

        // Photon ���� ����
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }
    #endregion

    #region Data Handling
    /// <summary>
    /// �÷��̾� ������ ���� ���� Ȯ�� �� �ʱ�ȭ.
    /// </summary>
    private void CheckPlayerData()
    {
        if (SaveSystem.PlayerDataExists())
        {
            loadingText.text = "�÷��̾� �����͸� ã�ҽ��ϴ�";
            ConnectAndLoadMainScene();
        }
        else
        {
            loadingText.text = "�÷��̾� �����͸� ã�� ���߽��ϴ�";
            ShowCharacterSelectionPanel();
        }
    }

    /// <summary>
    /// ����� �÷��̾� �����͸� �ε��ϰ� ���� ������ ����.
    /// </summary>
    private void ConnectAndLoadMainScene()
    {
        loadingText.text = "���̺� ������ �ҷ����� ��";
        PlayerData data = SaveSystem.LoadPlayerData();
        DataHolder.NickName = data.NickName;
        DataHolder.SelectedCharacterIndex = data.CharacterClassIndex;
        loadingText.text = "������";
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// �г��� �Է� �� Photon ����.
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
    /// �г��� �Է� ���� ǥ��.
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
    #endregion

    #region Character Selection
    /// <summary>
    /// ĳ���� ���� �г� ǥ�� �� �ʱ� �ִϸ��̼� Ʈ����.
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
    /// ���� ĳ���� ����.
    /// </summary>
    public void SelectWarrior()
    {
        selectedIndex = (int)CharacterClassType.Warrior;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// �ų� ĳ���� ����.
    /// </summary>
    public void SelectGunner()
    {
        selectedIndex = (int)CharacterClassType.Gunner;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// ������ ĳ���� ����.
    /// </summary>
    public void SelectMage()
    {
        selectedIndex = (int)CharacterClassType.Mage;
        UpdateSelectionIndicator();
    }

    /// <summary>
    /// �������� �̵��� �� ȣ��Ǵ� �ڵ鷯.
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
    /// ���������� �̵��� �� ȣ��Ǵ� �ڵ鷯.
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
    /// ���� �ε������� ��ġ ������Ʈ.
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
