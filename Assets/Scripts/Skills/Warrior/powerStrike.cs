using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerStrike : Skill
{
    private Warrior warrior;
    private AudioClip chargingSound = Resources.Load<AudioClip>("Sounds/PowerStrikeCharging");
    private AudioClip impactSound = Resources.Load<AudioClip>("Sounds/PowerStrikeImpact");
    public powerStrike(List<Skill> prerequisites) : base("필살 일격", "모든 힘을 실어 전방의 적들에게 매우 큰 데미지를 줍니다.", 2, prerequisites, 3,5f) { }
    protected override void ExecuteSkill(Character character)
    {
        if (character == null) return;
        if (character is Warrior)
        {
            warrior = character as Warrior;
        }
        warrior.SetIsAttacking(true);
        warrior.SetLastAttackTime(Time.time);
        warrior.SetAttackTime(Time.time + 0.8f);
        warrior.RB.velocity = Vector2.zero;
        warrior.audioSource.PlayOneShot(chargingSound);
        warrior.StartCoroutine(StartPowerStrike(warrior));
    }
    private IEnumerator StartPowerStrike(Warrior warrior)
    {
        Vector2 attackDirection = warrior.GetLastMoveDirection();
        warrior.PV.RPC("StartPowerStrikeMotion", RpcTarget.All, attackDirection);
        yield return new WaitForSeconds(0.5f); // 0.5초 선딜


        warrior.audioSource.PlayOneShot(impactSound);
        PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity)
            .GetComponent<PhotonView>().RPC("SetPowerStrike", RpcTarget.All, attackDirection, warrior.attackDamage * 5, warrior.PV.ViewID);
        SetLastUsedTime(Time.time); // 쿨타임 갱신
        yield return new WaitForSeconds(0.06f);
        warrior.SetIsAttacking(false);
        
    }
}
