using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

public class Slime : MonsterBase
{
    protected override void Start()
    {
        base.Start();

    }

    [PunRPC]
    void SetForBoss()
    {
        experiencePoints = 0;
        Vector2 dir = new Vector2(UnityEngine.Random.Range(-1f,1f), UnityEngine.Random.Range(-1f,1f)).normalized * UnityEngine.Random.Range(2f,3f);
        GetComponent<Rigidbody2D>().AddForce(dir,ForceMode2D.Impulse);
    }
    protected override void Attack()
    {
    }

    protected override void GiveExperienceToAttackers()
    {
        base.GiveExperienceToAttackers();

        UniTask.Void(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f));//죽는 애니메이션 대기
            PhotonNetwork.Destroy(gameObject);
        });
    }
}