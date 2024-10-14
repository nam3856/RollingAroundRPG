using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GrenadeToss : Skill
{
    private AudioClip grenadeSound = Resources.Load<AudioClip>("Sounds/grenadegrab");

    private AudioClip grenadeSound2 = Resources.Load<AudioClip>("Sounds/grenadethrow");
    private AudioClip grenadeExplodeSound = Resources.Load<AudioClip>("Sounds/grenadeExplode");

    public GrenadeToss(List<Skill> prerequisites) : base("����ź ��ô", "����ź�� ���� �������� ���ظ� �ݴϴ�.", 0, prerequisites, 3, 16f)
    {
    }

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;

        character.audioSource.PlayOneShot(grenadeSound);
        character.ResetAttackState(0.2f, true).Forget();
        character.StartCoroutine(ThrowGrenade(character, 0.2f));
    }

    private IEnumerator ThrowGrenade(Character character, float sec)
    {
        yield return new WaitForSeconds(sec);

        character.audioSource.PlayOneShot(grenadeSound2);
        Vector2 attackDirection = character.GetLastMoveDirection();
        int attackDamage = character.attackDamage * 3;
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


        // PhotonNetwork.Instantiate�� ����Ͽ� �Ѿ� ����
        GameObject grenade = PhotonNetwork.Instantiate("grenade", bulletPosition, Quaternion.identity);
        GrenadeScript grenadeScript = grenade.GetComponent<GrenadeScript>();
        Collider2D shooterCollider = character.GetComponent<Collider2D>();
        grenadeScript.SetDirectionAndDamage(new object[] { attackDirection, attackDamage, character.PV.OwnerActorNr, shooterCollider, true, character.PV.ViewID });

        // ĳ������ ���� ��� ����ȭ
        character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);

        // ��ų ��Ÿ�� ����
        SetLastUsedTime(Time.time);
        character.StartCoroutine(playExplodeSound(1.0f, character));
        character.SetIsAttacking(false);
    }
    private IEnumerator playExplodeSound(float sec, Character character)
    {
        yield return new WaitForSeconds(sec);
        character.audioSource.PlayOneShot(grenadeExplodeSound);
    }
}
