using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class rapidFire : Skill
{
    public int bulletCount;
    public float spreadAngle = 30f;

    private AudioClip shotSound = Resources.Load<AudioClip>("Sounds/shot");
    private AudioClip reloadSound = Resources.Load<AudioClip>("Sounds/reload");

    public rapidFire(List<Skill> prerequisites) : base("난사", "남은 탄창 수만큼 총알을 발사합니다.", 1, prerequisites, 2, 4f)
    {
    }

    protected override void ExecuteSkill(Character character)
    {
        Gunner gunner = character as Gunner;
        gunner.SetIsAttacking(true);
        gunner.RB.velocity = Vector2.zero;
        float halfSpread = spreadAngle / 2f;
        Vector2 attackDirection = gunner.GetLastMoveDirection();
        int attackDamage = gunner.attackDamage * 2;
        Vector3 bulletPosition = gunner.transform.position;

        bulletCount = character.GetCurrentMP() + 1;

        character.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;


        // 캐릭터의 공격 모션 동기화
        gunner.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);

        for (int i = 0; i < bulletCount; i++)
        {
            float randomAngle = Random.Range(-halfSpread, halfSpread);
            Vector2 randomDirection = RotateVector(attackDirection, randomAngle);


            UniTask.Void(async () =>
            {
                await UniTask.Delay(50 * i);
                character.AdjustCurrentMP(1);
                
                gunner.audioSource.PlayOneShot(shotSound);
                GameObject bullet = PhotonNetwork.Instantiate("Bullet", bulletPosition, Quaternion.identity);
                BulletScript bulletScript = bullet.GetComponent<BulletScript>();
                Collider2D shooterCollider = gunner.GetComponent<Collider2D>();
                bulletScript.SetDirectionAndDamage(new object[] { randomDirection, attackDamage, gunner.PV.OwnerActorNr, shooterCollider, false, gunner.PV.ViewID });
            });
        }
        SetLastUsedTime(Time.time);

        UniTask.Void(async () =>
        {
            await UniTask.Delay(50 * bulletCount + 500);
            character.audioSource.PlayOneShot(reloadSound);
            character.SetIsAttacking(false);

            character.RB.constraints = RigidbodyConstraints2D.FreezeRotation;
            await UniTask.Delay(1000);
            character.AdjustCurrentMP(-character.GetMaxMP());
        });
    }
    private Vector2 RotateVector(Vector2 direction, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sin(radian);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );
    }

}
