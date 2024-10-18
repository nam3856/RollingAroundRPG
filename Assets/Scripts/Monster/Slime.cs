using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;

public class SlimeMonster : MonsterBase
{
    protected override void Start()
    {
        base.Start();

    }

    protected override void Attack()
    {
    }

    protected override void GiveExperienceToAttackers()
    {
        base.GiveExperienceToAttackers();

        UniTask.Void(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f));//�״� �ִϸ��̼� ���
            PhotonNetwork.Destroy(gameObject);
        });
    }
}