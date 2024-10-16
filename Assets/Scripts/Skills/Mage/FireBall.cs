using Cysharp.Threading.Tasks;
using Photon.Pun;
using System;
using UnityEngine;

public class FireBall : Skill
{
    private Mage mage;
    public FireBall() : base("화염구", "화염구를 던집니다", 1, null, 0, 3f) {

        icon = Resources.Load<Sprite>("Icons/Mage_Skill1");
    }

    private AudioClip SpellSoundClip = Resources.Load<AudioClip>("Sounds/fireballSpellSound");

    protected override void ExecuteSkill(Character character)
    {
        character.ResetAttackState(0.5f, true).Forget();
        
        character.audioSource.PlayOneShot(SpellSoundClip);
        UniTask.Void(async () =>
        {
            Vector2 attackDirection = character.GetLastMoveDirection();
            character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);

            await UniTask.Delay(500);

            int attackDamage = (int)Math.Ceiling(character.attackDamage);
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

            GameObject fireball = PhotonNetwork.Instantiate("Fireball", bulletPosition, Quaternion.identity);
            FireballScript fireballScript = fireball.GetComponent<FireballScript>();
            Collider2D shooterCollider = character.GetComponent<Collider2D>();
            fireballScript.SetDirectionAndDamage(new object[] { attackDirection, attackDamage, character.PV.OwnerActorNr, shooterCollider, true, character.PV.ViewID, critical });


            // 스킬 쿨타임 설정
            SetLastUsedTime(Time.time);
        });
    }

}
