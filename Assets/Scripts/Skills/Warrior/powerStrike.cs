using Cysharp.Threading.Tasks;
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
    public powerStrike(List<Skill> prerequisites) : base("필살 일격", "모든 힘을 실어 전방의 적들에게 매우 큰 데미지를 줍니다.", 2, prerequisites, 3,5f) {
        icon = Resources.Load<Sprite>("Icons/Warrior_Skill4");
    }
    protected override void ExecuteSkill(Character character)
    {
        if (character == null) return;
        if (character is Warrior)
        {
            warrior = character as Warrior;
        }
        
        warrior.ResetAttackState(0.6f).Forget();

        PowerStrikeAsync(warrior).Forget();
    }
    private async UniTask PowerStrikeAsync(Warrior warrior)
    {
        warrior.audioSource.PlayOneShot(chargingSound);

        Vector2 attackDirection = warrior.GetLastMoveDirection();
        warrior.PV.RPC("StartPowerStrikeMotion", RpcTarget.All, attackDirection);

        await UniTask.Delay(500);

        warrior.audioSource.PlayOneShot(impactSound);
        PhotonNetwork.Instantiate("FanBullet", warrior.transform.position, Quaternion.identity)
            .GetComponent<PhotonView>().RPC("SetPowerStrike", RpcTarget.All, attackDirection, warrior.attackDamage * 5, warrior.PV.ViewID);
    }
}
