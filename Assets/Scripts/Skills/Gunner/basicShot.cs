using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basicShot : Skill
{
    private AudioClip shotSound = Resources.Load<AudioClip>("Sounds/shot");
    private AudioClip reloadSound = Resources.Load<AudioClip>("Sounds/reload");

    public basicShot() : base("기본 사격", "총을 써서 전방의 적 1명에게 데미지를 줍니다.", 1, null, 0, 0.3f)
    {

        icon = Resources.Load<Sprite>("Icons/Gunner_Skill1");
    }

    protected override void ExecuteSkill(Character character)
    {
        character.ResetAttackState(0.3f, true).Forget();

        Vector2 attackDirection = character.GetLastMoveDirection();
        int attackDamage = character.attackDamage;
        Vector3 bulletPosition = character.transform.position;

        // 발사 방향에 따라 총알 위치 조정
        if (Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y))
        {
            attackDirection = new Vector2(Mathf.Sign(attackDirection.x), 0);
            bulletPosition += new Vector3(attackDirection.x * 0.17f, -0.03f, 0);
        }
        else
        {
            attackDirection = new Vector2(0, Mathf.Sign(attackDirection.y));
            bulletPosition += new Vector3(attackDirection.y > 0 ? 0.15f : -0.1f, attackDirection.y * 0.2f, 0);
        }

        character.audioSource.PlayOneShot(shotSound);

        // PhotonNetwork.Instantiate를 사용하여 총알 생성
        GameObject bullet = PhotonNetwork.Instantiate("Bullet", bulletPosition, Quaternion.identity);
        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        Collider2D shooterCollider = character.GetComponent<Collider2D>();
        bulletScript.SetDirectionAndDamage(new object[] { attackDirection, attackDamage, character.PV.OwnerActorNr, shooterCollider, true, character.PV.ViewID });

        // 캐릭터의 공격 모션 동기화
        character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);

        // 스킬 쿨타임 설정
        SetLastUsedTime(Time.time);
        if (character.GetCurrentMP() == 0)
        {
            UniTask.Void(async () =>
            {
            await UniTask.Delay(290);
            character.audioSource.PlayOneShot(reloadSound);
            await UniTask.Delay(1000);
            character.AdjustCurrentMP(-character.GetMaxMP());
            });
        }
    }
}
