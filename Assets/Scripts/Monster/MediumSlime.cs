using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

public class BossSlimeMedium : MonsterBase
{
    private SlimeBoss parentBoss;

    public void Initialize(int health, SlimeBoss boss)
    {
        maxHealth = health;
        currentHealth = health;
        parentBoss = boss;
        Recombine().Forget();
    }


    private async UniTask Recombine()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(parentBoss.recombineDelay));
        if(parentBoss.isSplit)
            parentBoss.RecombineFromMediumSlimes();
    }
    public int GetCurrentHealth()
    {
        Debug.Log(currentHealth);
        return currentHealth;
    }

    protected override void Attack()
    {
        // Implement medium slime attack behavior
    }

    [PunRPC]
    public override void TakeDamage(object[] data)
    {
        base.TakeDamage(data);
    }

    protected override void GiveExperienceToAttackers()
    {
        parentBoss.mediumSlimes.Remove(gameObject);
        if(parentBoss.mediumSlimes.Count == 0 && parentBoss.isSplit)
        {
            parentBoss.gameObject.SetActive(true);
            if (PhotonNetwork.IsMasterClient)
            {
                parentBoss.GetComponent<PhotonView>().RPC("DieInstant", RpcTarget.All);
            }
        }
        UniTask.Void(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f));
            PhotonNetwork.Destroy(gameObject);
        });
    }
}