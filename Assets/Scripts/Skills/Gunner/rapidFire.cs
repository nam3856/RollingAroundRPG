using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class rapidFire : Skill
{
    public int bulletCount;
    public float spreadAngle = 30f;

    private AudioClip shotSound = Resources.Load<AudioClip>("Sounds/shot");
    private AudioClip reloadSound = Resources.Load<AudioClip>("Sounds/reload");

    public rapidFire(List<Skill> prerequisites) : base("����", "���� źâ ����ŭ �Ѿ��� �߻��մϴ�.", 1, prerequisites, 2, 4f)
    {

        icon = Resources.Load<Sprite>("Icons/Gunner_Skill2");
    }

    protected override void ExecuteSkill(Character character)
    {
        Gunner gunner = character as Gunner;
        float halfSpread = spreadAngle / 2f;
        Vector2 attackDirection = gunner.GetLastMoveDirection();
        int attackDamage = (int)Math.Ceiling(gunner.attackDamage * 1.5);
        Vector3 bulletPosition = gunner.transform.position;
        bulletCount = character.GetCurrentMP() + 1;
        character.ResetAttackState(0.05f * bulletCount + 0.5f, true).Forget();

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

        for (int i = 0; i < bulletCount; i++)
        {
            float randomAngle = UnityEngine.Random.Range(-halfSpread, halfSpread);
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
                bulletScript.SetDirectionAndDamage(new object[] { randomDirection, attackDamage, gunner.PV.OwnerActorNr, shooterCollider, false, gunner.PV.ViewID, critical });
            });
        }

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
