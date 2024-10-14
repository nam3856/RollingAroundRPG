using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GrenadeToss : Skill
{
    private AudioClip grenadeThrowSound;
    private AudioClip grenadeExplodeSound;
    private int attackDamage = 0;

    public GrenadeToss(List<Skill> prerequisites)
        : base("수류탄 투척", "수류탄을 던져 광범위한 피해를 줍니다.", 0, prerequisites, 3, 16f)
    {
        grenadeThrowSound = Resources.Load<AudioClip>("Sounds/grenadeThrow");
        grenadeExplodeSound = Resources.Load<AudioClip>("Sounds/grenadeExplode");

        icon = Resources.Load<Sprite>("Icons/Gunner_Skill3");
    }

    protected override void ExecuteSkill(Character character)
    {
        attackDamage = character.attackDamage * 3;
    }

    public void ThrowGrenade(Character character, float throwForce)
    {
        // 캐릭터의 마지막 이동 방향을 기준으로 발사 방향 결정
        Vector2 attackDirection = character.GetLastMoveDirection();
        if (attackDirection == Vector2.zero)
            attackDirection = Vector2.left; // 기본 방향 설정

        Vector2 throwVelocity = attackDirection.normalized * throwForce;

        Vector3 spawnPosition = character.transform.position + (Vector3)(attackDirection.normalized * 0.1f);

        GameObject grenade = PhotonNetwork.Instantiate("grenade", spawnPosition, Quaternion.identity);
        GrenadeScript grenadeScript = grenade.GetComponent<GrenadeScript>();
        Collider2D shooterCollider = character.GetComponent<Collider2D>();
        grenadeScript.SetDirectionAndDamage(new object[] { attackDirection, attackDamage, character.PV.OwnerActorNr, shooterCollider, true, character.PV.ViewID, throwVelocity });

        StartCoolDown(character);

        character.audioSource.PlayOneShot(grenadeThrowSound);

        PlayExplodeSoundAsync(1000, character).Forget();
    }

    private async UniTask PlayExplodeSoundAsync(int delay, Character character)
    {
        await UniTask.Delay(delay);
        character.audioSource.PlayOneShot(grenadeExplodeSound);
    }
}
