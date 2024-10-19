using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    private BaseItem BaseItem;
    public Sprite itemSprite;
    public Rigidbody2D Rigidbody2D;
    public CircleCollider2D CircleCollider2D;
    public PhotonView PhotonView;
    private bool IsSet = false;
    private Vector2 dir;
    private int owner;

    private void Start()
    {
        if (PhotonView == null) PhotonView = GetComponent<PhotonView>();
        if (itemSprite == null) itemSprite = GetComponent<Sprite>();
        if (Rigidbody2D == null) Rigidbody2D = GetComponent<Rigidbody2D>();
        if (CircleCollider2D == null) CircleCollider2D = GetComponent<CircleCollider2D>();
    }
    private void OnEnable()
    {
        
    }

    private async UniTaskVoid DropItem()
    {
        while(IsSet) await UniTask.Yield();
        CircleCollider2D.enabled = true;
        await UniTask.Delay(TimeSpan.FromSeconds(30));
        owner = -1;
        await UniTask.Delay(TimeSpan.FromSeconds(30));
        
        //이 다음 풀로 반환
    }

    [PunRPC]
    private void SetItem(BaseItem baseItem, Vector2 vector2, int owner = -1)
    {
        itemSprite = baseItem.icon;
        BaseItem = baseItem;
        dir = vector2 * 0.1f;
        Rigidbody2D.AddForce(dir);
        IsSet = true;
    }

    //circleCollider2d stay인가 뭐시기로 들어와 있는 사람의 viewID가 owner면 아이템 먹어지게
    //owner가 -1이면 그냥 아무나 먹어지게

    //먹어지는로직 : 아이템 추가 시도, 성공시 오브젝트 풀로 반환

    //풀로 반환할때
}
