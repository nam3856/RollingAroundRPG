using System.Collections;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class SnipeShotSkill : MonoBehaviourPunCallbacks
{
    public CinemachineVirtualCamera sniperCamera;
    public CanvasGroup sniperOverlay;
    public float zoomSpeed = 10f;
    public float overlayFadeSpeed = 5f;
    public float zoomInOrthoSize = 3.5f;
    private bool isSniping = false;
    private Transform originalFollow;
    private Transform originalLookAt;
    private AudioClip snipeSound;
    private CinemachineConfiner confiner;
    private Collider2D originalBoundingShape;
    private Character character;
    private HashSet<int> attackedPlayers = new HashSet<int>();

    private AudioClip reloadSound;
    private void Start()
    {
        sniperCamera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
        sniperOverlay = GameObject.Find("UI/SnipeScope").GetComponent<CanvasGroup>();
        confiner = sniperCamera.GetComponent<CinemachineConfiner>();
        character = GetComponentInParent<Character>();
        originalFollow = sniperCamera.Follow;
        originalLookAt = sniperCamera.LookAt;
        sniperOverlay.alpha = 0;
        snipeSound = Resources.Load<AudioClip>("Sounds/snipeShot");
        
    }


    private void Update()
    {
        if (isSniping && photonView.IsMine)
        {
            HandleSniperMovement();
            if (Input.GetKeyDown(KeyCode.R))
            {
                character.audioSource.PlayOneShot(snipeSound);
                Shoot();
            }
        }
    }

    public IEnumerator ActivateSnipeShot()
    {
        originalBoundingShape = confiner.m_BoundingShape2D;
        // Stop following the player
        sniperCamera.Follow = null;
        sniperCamera.LookAt = null;

        // Disable bounding shape
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = null;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Zoom in the camera and fade in the overlay simultaneously
        float currentOrthoSize = sniperCamera.m_Lens.OrthographicSize;
        while (currentOrthoSize > zoomInOrthoSize || sniperOverlay.alpha < 1)
        {
            if (currentOrthoSize > zoomInOrthoSize)
            {
                currentOrthoSize -= zoomSpeed * Time.deltaTime;
                sniperCamera.m_Lens.OrthographicSize = currentOrthoSize;
            }

            if (sniperOverlay.alpha < 1)
            {
                sniperOverlay.alpha += overlayFadeSpeed * Time.deltaTime;
            }

            yield return null;
        }
        sniperCamera.m_Lens.OrthographicSize = zoomInOrthoSize;
        Debug.Log("�����غ� �Ϸ�");
        isSniping = true;
        sniperOverlay.alpha = 1;
    }

    private void HandleSniperMovement()
    {
        float moveSpeed = 10f;
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        sniperCamera.transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void Shoot()
    {
        isSniping = false;
        // ȭ�� �߾��� ��ũ�� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenCenter);

        // Ÿ�� ���� ������ ����
        float radius = 0.5f;

        // �ش� ���� ���� ��� �ݶ��̴� ����
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPoint, radius);

        foreach (var collider in hitColliders)
        {
            
            if (collider.CompareTag("Monster"))
            {
                PhotonView monsterPV = collider.GetComponent<PhotonView>();
                if (attackedPlayers.Contains(monsterPV.ViewID)) continue;
                attackedPlayers.Add(monsterPV.ViewID);
                if (monsterPV != null)
                {
                    monsterPV.RPC("TakeDamage", RpcTarget.All, 999, GetComponent<Character>().PV.ViewID);
                }
            }
        }
        DeactivateSnipeShot();
    }

    

    private void DeactivateSnipeShot()
    {

        character.RB.constraints = RigidbodyConstraints2D.FreezeRotation;
        UniTask.Void(async () =>
        {
            await UniTask.Delay(1000);
            character.AdjustCurrentMP(-character.GetMaxMP());
        });
        character.SetIsAttacking(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        float currentOrthoSize = sniperCamera.m_Lens.OrthographicSize;
        sniperCamera.m_Lens.OrthographicSize = 5f;
        sniperOverlay.alpha = 0;

        sniperCamera.Follow = originalFollow;
        sniperCamera.LookAt = originalLookAt;

        if (confiner != null)
        {
            confiner.m_BoundingShape2D = originalBoundingShape;
        }
    }
}