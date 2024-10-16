using System.Collections;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;
using Photon.Pun.Demo.PunBasics;

public class SnipeShotSkill : MonoBehaviourPunCallbacks
{
    public CinemachineVirtualCamera sniperCamera;
    public CanvasGroup sniperOverlay;
    public float zoomSpeed = 10f;
    public float overlayFadeSpeed = 5f;
    public float zoomInOrthoSize = 3.5f;
    public bool IsSniping = false;
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

    private SnipeShot snipeShotSkill;

    public void SetSnipeShotSkill(SnipeShot skill)
    {
        snipeShotSkill = skill;
    }

    private void Update()
    {
        if (IsSniping && photonView.IsMine && !character.isDead)
        {
            HandleSniperMovement();
            if (Input.GetKeyDown(KeyCode.R))
            {
                character.audioSource.PlayOneShot(snipeSound);
                Shoot();
            }
        }
    }

    public async UniTask ActiveSnipeScopeAsync()
    {
        originalBoundingShape = confiner.m_BoundingShape2D;
        sniperCamera.Follow = null;
        sniperCamera.LookAt = null;
        confiner.m_BoundingShape2D = null;

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

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate);
        }
        sniperCamera.m_Lens.OrthographicSize = zoomInOrthoSize;
        Debug.Log("저격준비 완료");
        IsSniping = true;
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
        IsSniping = false;
        // 화면 중앙의 스크린 좌표를 월드 좌표로 변환
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenCenter);

        // 타격 범위 반지름 설정
        float radius = 0.5f;

        // 해당 범위 내의 모든 콜라이더 감지
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
                    monsterPV.RPC("TakeDamage", RpcTarget.All, new object[] { snipeShotSkill.damage, GetComponent<Character>().PV.ViewID, true});
                }
            }
        }
        snipeShotSkill?.StartCoolDown(character);

        DeactivateSnipeShot();
    }

    public void DeactivateSnipeShot()
    {
        IsSniping = false;
        character.RB.constraints = RigidbodyConstraints2D.FreezeRotation;

        UniTask.Void(async () =>
        {
            await UniTask.Delay(1000);
            character.AdjustCurrentMP(-character.GetMaxMP());
        });
        character.SetIsAttacking(false);

        float currentOrthoSize = sniperCamera.m_Lens.OrthographicSize;
        sniperCamera.m_Lens.OrthographicSize = 5f;
        sniperOverlay.alpha = 0;

        sniperCamera.Follow = originalFollow;
        sniperCamera.LookAt = originalLookAt;

        confiner.m_BoundingShape2D = originalBoundingShape;

        attackedPlayers.Clear();
    }
}