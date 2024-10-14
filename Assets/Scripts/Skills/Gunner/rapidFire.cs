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

        icon = Resources.Load<Sprite>("Icons/Gunner_Skill2");
    }

    protected override void ExecuteSkill(Character character)
    {
        Gunner gunner = character as Gunner;
        float halfSpread = spreadAngle / 2f;
        Vector2 attackDirection = gunner.GetLastMoveDirection();
        int attackDamage = gunner.attackDamage * 2;
        Vector3 bulletPosition = gunner.transform.position;
        bulletCount = character.GetCurrentMP() + 1;

        character.ResetAttackState(0.05f * bulletCount + 0.5f, true).Forget();

        for (int i = 0; i < bulletCount; i++)
        {
            float randomAngle = Random.Range(-halfSpread, halfSpread);
            Vector2 randomDirection = RotateVector(attackDirection, randomAngle);

            UniTask.Void(async () =>
            {
                await UniTask.Delay(50 * i);

                gunner.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);
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
