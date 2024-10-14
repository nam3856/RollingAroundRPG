using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FireBall : Skill
{
    private Mage mage;
    public FireBall() : base("ȭ����", "ȭ������ �����ϴ�", 1, null, 0, 3f) { }

    private AudioClip SpellSoundClip = Resources.Load<AudioClip>("Sounds/fireballSpellSound");

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;

        character.audioSource.PlayOneShot(SpellSoundClip);
        character.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        UniTask.Void(async () =>
        {
            Vector2 attackDirection = character.GetLastMoveDirection();
            character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);

            await UniTask.Delay(500);

            int attackDamage = character.attackDamage;
            Vector3 bulletPosition = character.transform.position;

            // �߻� ���⿡ ���� �Ѿ� ��ġ ����
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

            //character.audioSource.PlayOneShot(shotSound);

            // PhotonNetwork.Instantiate�� ����Ͽ� �Ѿ� ����
            GameObject fireball = PhotonNetwork.Instantiate("Fireball", bulletPosition, Quaternion.identity);
            FireballScript fireballScript = fireball.GetComponent<FireballScript>();
            Collider2D shooterCollider = character.GetComponent<Collider2D>();
            fireballScript.SetDirectionAndDamage(new object[] { attackDirection, attackDamage, character.PV.OwnerActorNr, shooterCollider, true, character.PV.ViewID });


            // ��ų ��Ÿ�� ����
            SetLastUsedTime(Time.time);

            character.SetIsAttacking(false);
            character.RB.constraints = RigidbodyConstraints2D.FreezeRotation;
        });
    }

}
