using Cinemachine;
using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerData
{
    public string NickName;
    public int CharacterClassIndex;
    public int Level;
    public int Experience;
    public List<string> LearnedSkills;
    public List<string> EquippedItems;
    public List<string> InventoryItems;
    public Vector3 LastPosition;
    public int currentCMRangeId;
}
public class PlayerScript : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    public DebugLogger logger;
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public TMP_Text NickNameText;
    public Image HealthImage;
    public TMP_InputField inputField;
    public bool cantMove = false;
    public int maxHealth = 100;

    private Character character;

    public PlayerData playerData;

    public CharacterClass characterClass;
    private int characterIndex;
    private KeyCode[] skillKeys = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R };

    public static event Action<PlayerScript> OnPlayerDied;
    public static event Action<PlayerScript> OnPlayerRespawned;

    private CinemachineVirtualCamera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (PV.IsMine)
        {
            PlayerManager.instance.localPlayerCharacter = this;
            if (SaveSystem.PlayerDataExists())
            {
                playerData = SaveSystem.LoadPlayerData();
                characterIndex = playerData.CharacterClassIndex;
                
            }
            else
            {
                playerData = new PlayerData
                {
                    NickName = PhotonNetwork.LocalPlayer.NickName,
                    CharacterClassIndex = characterIndex,
                    Level = 1,
                    Experience = 0,
                    LearnedSkills = new List<string>(),
                    EquippedItems = new List<string>(),
                    InventoryItems = new List<string>(),
                    LastPosition = transform.position,
                    currentCMRangeId = 0
};
                SaveSystem.SavePlayerData(playerData);
            }
        }
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.blue;

        if (logger == null)
        {
            logger = FindObjectOfType<DebugLogger>();
        }

        inputField = FindObjectOfType<TMP_InputField>();

        SetCharacter(characterIndex);
    }
    void SetCharacter(int index)
    {
        UIManager uIManager = FindObjectOfType<UIManager>();
        SkillTreeManager skillTreeManager = FindObjectOfType<SkillTreeManager>();

        switch (index)
        {
            case 0:
                // 전사
                character = gameObject.AddComponent<Warrior>();
                if (PV.IsMine)
                {
                    uIManager.InitializeSkillsForClass("Warrior");

                    // 기본 스킬 획득
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[0].Skills[0], character);  // 전사 기본 공격
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[0].Skills[1], character);  // 전사 콤보 공격
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[0].Skills[2], character); // 전사 돌진
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[0].Skills[3], character);  // 전사 파워 스트라이크
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[0].Skills[4], character);  // 전사 방패 막기
                }


                AN.SetTrigger("warrior init");
                break;
            case 1:
                // 거너
                character = gameObject.AddComponent<Gunner>();
                if (PV.IsMine)
                {
                    uIManager.InitializeSkillsForClass("Gunner");
                    character.gameObject.AddComponent<SnipeShotSkill>();

                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[1].Skills[0], character);  // 거너 기본 공격
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[1].Skills[1], character);  // 거너 난사 스킬
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[1].Skills[2], character);  // 거너 수류탄
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[1].Skills[3], character);  // 거너 저격
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[1].Skills[4], character);  // 거너 구르기
                }
                AN.SetTrigger("gunner init");
                break;
            case 2:
                // 법사
                Debug.Log(2);
                character = gameObject.AddComponent<Mage>();
                if (PV.IsMine)
                {
                    uIManager.InitializeSkillsForClass("Mage");
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[2].Skills[0], character);  //법사 염구
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[2].Skills[1], character);  //법사 보호막
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[2].Skills[2], character);  //법사 치유
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[2].Skills[3], character);  //법사 메테오 ToDo
                    skillTreeManager.AcquireSkill(skillTreeManager.CharacterClasses[2].Skills[4], character);  //법사 텔포 ToDo
                }
                AN.SetTrigger("mage init");
                break;
        }
        AddToPhotonObservedComponents(character);
    }

    private void OnApplicationQuit()
    {
        if (PV.IsMine)
        {
            playerData.NickName = NickNameText.text;
            playerData.LastPosition = transform.position;
            SaveSystem.SavePlayerData(playerData);
        }
    }
    void AddToPhotonObservedComponents(MonoBehaviour component)
    {
        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView != null && !photonView.ObservedComponents.Contains(component))
        {
            photonView.ObservedComponents.Add(component);
            //Debug.Log($"{component.GetType().Name}이(가) PhotonView의 Observed Components에 추가되었습니다.");
        }
    }

    void Update()
    {
        if (PV.IsMine)
        {
            HandleInput();
        }

    }

    void HandleInput()
    {
        if (inputField.isFocused) return;
        if (character.isRushing || character.isRolling|| character.GetIsAttacking()|| cantMove) return;
        Vector2 moveDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection.y = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection.y = -1;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection.x = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection.x = -1;
        }

         character.Move(moveDirection);

        if (Input.GetKeyDown(KeyCode.F))
        {
            character.StartSpecialSkill(true);
        }


        if (Input.GetKeyUp(KeyCode.F))
        {
            character.StartSpecialSkill(false);
        }

        for (int i = 0; i < skillKeys.Length; i++)
        {
            if (Input.GetKeyDown(skillKeys[i]))
            {
                character.StartSkill(i);
            }
        }
    }

    [PunRPC]
    void Teleport(Vector3 destination, int confinerID)
    {
        // 플레이어 위치 이동
        transform.position = destination;

        // CinemachineConfiner 설정
        if (!PV.IsMine) return;
        PolygonCollider2D confiner = FindConfinerByID(confinerID);
        if (confiner != null)
        {
            CinemachineVirtualCamera mainCamera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CinemachineConfiner confinerComponent = mainCamera.GetComponent<CinemachineConfiner>();
            if (confinerComponent != null)
            {
                confinerComponent.m_BoundingShape2D = confiner;
                playerData.currentCMRangeId = confinerID;
                playerData.LastPosition = destination;
                SaveSystem.SavePlayerData(playerData);
            }
            else
            {
                Debug.LogError("CinemachineConfiner를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("해당 confinerID를 가진 confiner를 찾을 수 없습니다.");
        }
    }

    public static PolygonCollider2D FindConfinerByID(int id)
    {
        ConfinerID[] allConfiners = FindObjectsOfType<ConfinerID>();
        foreach (var confinerIDComponent in allConfiners)
        {
            if (confinerIDComponent.confinerID == id)
            {
                return confinerIDComponent.GetComponent<PolygonCollider2D>();
            }
        }
        return null;
    }


    [PunRPC]
    void DeactivateGameObject()
    {
        //Destroy(gameObject);
        OnPlayerDied?.Invoke(this);
        gameObject.SetActive(false);
    }
    public void RespawnCharacter(Vector3 respawnPosition)
    {
        PV.RPC("ReactivateGameObject", RpcTarget.AllBuffered, respawnPosition);
        PolygonCollider2D confiner = FindConfinerByID(1);
        CinemachineVirtualCamera mainCamera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
        CinemachineConfiner confinerComponent = mainCamera.GetComponent<CinemachineConfiner>();
        if (confinerComponent != null)
        {
            confinerComponent.m_BoundingShape2D = confiner;
        }
        else
        {
            Debug.LogError("CinemachineConfiner를 찾을 수 없습니다.");
        }
    }
    [PunRPC]
    public void ReactivateGameObject(Vector3 spawnPosition)
    {
        gameObject.SetActive(true);
        character.PV.RPC("Respawn", RpcTarget.All);
        OnPlayerRespawned?.Invoke(this);
        //Debug.Log($"{PV.Owner.NickName} 부활, 이벤트 호출됨");
        transform.position = spawnPosition;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        characterIndex = (int)instantiationData[0];
    }
}

