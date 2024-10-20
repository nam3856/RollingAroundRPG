using Cinemachine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerData
{
    public int version = 1;

    public string NickName;
    public int CharacterClassIndex;
    public int Level;
    public int Experience;
    public List<string> LearnedSkills;

    public List<ItemInstanceData> InventoryItemsData = new List<ItemInstanceData>();
    public List<ItemInstanceData> EquippedItemsData = new List<ItemInstanceData>();
    [JsonIgnore]
    public List<ItemInstance> InventoryItems =new List<ItemInstance>();
    [JsonIgnore]
    public List<ItemInstance> EquippedItems = new List<ItemInstance>();

    public List<string> LearnedTraits;
    public SerializableVector3 LastPosition;
    public int currentCMRangeId;
    public int SkillPoint;
    public int TraitPoint;
}
[System.Serializable]
public struct SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
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
    UIManager uIManager;
    SkillTreeManager skillTreeManager;


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
                    EquippedItems = new List<ItemInstance>(),
                    InventoryItems = new List<ItemInstance>(),
                    LearnedTraits = new List<string>(),
                    LastPosition = new SerializableVector3(transform.position),
                    currentCMRangeId = 0,
                    SkillPoint = 0,
                    TraitPoint = 0
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
        uIManager = FindObjectOfType<UIManager>();
        skillTreeManager = FindObjectOfType<SkillTreeManager>();
        RuntimeAnimatorController controller;
        switch (index)
        {
            case 0:
                // 전사
                character = gameObject.AddComponent<Warrior>();
                if (PV.IsMine)
                {
                    uIManager.InitializeSkillsForClass("Warrior");
                    skillTreeManager.SetPlayerClass(skillTreeManager.CharacterClasses[0]);
                }

                controller = Resources.Load<RuntimeAnimatorController>("Animation/Warrior");
                AN.runtimeAnimatorController = controller;
                break;
            case 1:
                // 거너
                character = gameObject.AddComponent<Gunner>();
                if (PV.IsMine)
                {
                    uIManager.InitializeSkillsForClass("Gunner");
                    character.gameObject.AddComponent<SnipeShotSkill>();
                    skillTreeManager.SetPlayerClass(skillTreeManager.CharacterClasses[1]);

                }
                controller = Resources.Load<RuntimeAnimatorController>("Animation/Gunner");
                AN.runtimeAnimatorController = controller;
                break;
            case 2:
                // 법사
                character = gameObject.AddComponent<Mage>();
                if (PV.IsMine)
                {
                    uIManager.InitializeSkillsForClass("Mage");
                    skillTreeManager.SetPlayerClass(skillTreeManager.CharacterClasses[2]);

                }

                controller = Resources.Load<RuntimeAnimatorController>("Animation/Mage");
                AN.runtimeAnimatorController = controller;
                break;
        }

        SynchronizeAllParameters(GetComponent<PhotonAnimatorView>(), AN);
        character.OnPhotonViewInitialized += HandlePhotonViewInitialized;
        AddToPhotonObservedComponents(character);
    }

    void SynchronizeAllParameters(PhotonAnimatorView view, Animator animator)
    {
        // 기존의 동기화 파라미터를 초기화
        view.GetSynchronizedParameters().Clear();

        // Animator의 모든 파라미터 가져오기
        var parameters = animator.parameters;

        foreach (var param in parameters)
        {
            PhotonAnimatorView.ParameterType paramType;
            switch (param.type)
            {
                case AnimatorControllerParameterType.Bool:
                    paramType = PhotonAnimatorView.ParameterType.Bool;
                    break;
                case AnimatorControllerParameterType.Float:
                    paramType = PhotonAnimatorView.ParameterType.Float;
                    break;
                case AnimatorControllerParameterType.Int:
                    paramType = PhotonAnimatorView.ParameterType.Int;
                    break;
                case AnimatorControllerParameterType.Trigger:
                    paramType = PhotonAnimatorView.ParameterType.Trigger;
                    break;
                default:
                    continue;
            }

            view.SetParameterSynchronized(param.name, paramType, PhotonAnimatorView.SynchronizeType.Discrete);
        }
    }


    private void HandlePhotonViewInitialized(Character character)
    {
        uIManager.character = character;
        skillTreeManager.AcquireSkill(skillTreeManager.PlayerClass.Skills[0], character);
    }

    private void OnApplicationQuit()
    {
        if (PV.IsMine)
        {
            playerData.NickName = NickNameText.text;
            playerData.LastPosition = new SerializableVector3(transform.position);
            playerData.Experience = character.experience;
            SaveSystem.SavePlayerData(playerData);
        }
    }
    void AddToPhotonObservedComponents(MonoBehaviour component)
    {
        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView != null && !photonView.ObservedComponents.Contains(component))
        {
            photonView.ObservedComponents.Add(component);
        }
    }

    void Update()
    {
        if (PV.IsMine && character.PV != null)
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
                playerData.LastPosition = new SerializableVector3(destination);
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
            playerData.currentCMRangeId = 1;
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
        transform.position = spawnPosition;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        characterIndex = (int)instantiationData[0];
    }
    private void OnDestroy()
    {
        if (character != null)
        {
            character.OnPhotonViewInitialized -= HandlePhotonViewInitialized;
        }
    }
}

