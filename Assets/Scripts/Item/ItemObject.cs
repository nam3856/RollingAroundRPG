using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    private ItemInstance itemInstance;
    public SpriteRenderer itemSprite;
    public Rigidbody2D Rigidbody2D;
    public CircleCollider2D CircleCollider2D;
    public PhotonView PhotonView;
    private bool IsSet = false;
    private Vector2 dir;
    [SerializeField]
    private int owner;
    public PhotonView targetPhotonView;


    private async UniTaskVoid DropItem()
    {
        while(IsSet) await UniTask.Yield();
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        gameObject.SetActive(true);
        CircleCollider2D.enabled = true;
        await UniTask.Delay(TimeSpan.FromSeconds(30));
        owner = -1;
        await UniTask.Delay(TimeSpan.FromSeconds(30));

        PhotonView.RPC("DestroyItem", RpcTarget.AllBuffered);
    }

    public void SetItem(ItemInstance itemInstance, Vector2 vector2, int owner = -1)
    {
        if (PhotonView == null) PhotonView = GetComponent<PhotonView>();
        if (itemSprite == null) itemSprite = GetComponent<SpriteRenderer>();
        if (Rigidbody2D == null) Rigidbody2D = GetComponent<Rigidbody2D>();
        if (CircleCollider2D == null) CircleCollider2D = Rigidbody2D.GetComponent<CircleCollider2D>();
        Color color = itemSprite.color;
        color.a = 1f;
        itemSprite.color = color;

        itemSprite.sprite = itemInstance.baseItem.icon;
        this.itemInstance = itemInstance;

        dir = vector2 * 1f;
        Rigidbody2D.AddForce(dir);
        this.owner = owner;
        IsSet = true;
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        targetPhotonView = col.GetComponent<PhotonView>();
        if (col.CompareTag("Player"))
        {
            if (targetPhotonView != null)
            {
                if (IsSet && (owner == -1 || owner == targetPhotonView.OwnerActorNr))
                {
                    IsSet = false;
                    if (targetPhotonView.IsMine)
                    {
                        MoveTowardsPlayerAsync(targetPhotonView.transform).Forget();
                        Character character = targetPhotonView.GetComponent<Character>();
                        if (character != null && character.TryGetItem(itemInstance))
                        {
                            // 다른 플레이어들에게 아이템 획득을 알림
                            targetPhotonView.RPC("GetItem", RpcTarget.Others, itemInstance.baseItem.id, itemInstance.instanceId);
                        }
                        else
                        {
                            IsSet = true;
                            return;
                        }
                    }
                    dir = (targetPhotonView.transform.position - transform.position).normalized * 0.1f;
                    PhotonView.RPC("DestroyItem", RpcTarget.AllBuffered);
                }
            }
        }
    }


    private async UniTaskVoid MoveTowardsPlayerAsync(Transform playerTransform)
    {
        float moveDuration = 1.0f;  // 아이템이 이동할 시간 (초)
        float elapsedTime = 0f;

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = playerTransform.position;

        Vector2 dir = (targetPosition - startPosition).normalized * 2f;
        // 지정된 시간 동안 이동
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            Rigidbody2D.AddForce(dir * Time.deltaTime, ForceMode2D.Impulse);

            await UniTask.Yield();  // 다음 프레임으로 넘어가기
        }

    }

    [PunRPC]
    private async UniTask DestroyItem()
    {
        IsSet = false;
        CircleCollider2D.enabled = false;
        itemInstance = null;
        float elapsedTime = 0f;
        float fadedTime = 0.7f;
        while (elapsedTime < fadedTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadedTime);
            Color color = itemSprite.color;
            color.a = alpha;
            itemSprite.color = color;

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }
        dir = Vector2.zero;
        itemSprite = null;

        gameObject.SetActive(false);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
