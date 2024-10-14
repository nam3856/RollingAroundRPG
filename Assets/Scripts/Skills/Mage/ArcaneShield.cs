using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ArcaneShield : Skill
{
    private HashSet<int> effectedPlayers = new HashSet<int>();
    
    private AudioClip SpellSoundClip = Resources.Load<AudioClip>("Sounds/ArcaneShieldSpellSound");
    public ArcaneShield(List<Skill> prerequisites) : base("비전 보호막", "나와 근처의 동료들에게 보호막을 걸어 피해량을 줄여줍니다.", 1, prerequisites, 0, 3f) {
    }

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;
        Vector2 attackDirection = character.GetLastMoveDirection();
        character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);
        character.audioSource.PlayOneShot(SpellSoundClip);
        character.RB.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        UniTask.Void(async () =>
        {

            await UniTask.Delay(500);
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(character.transform.position, 2f);
            foreach (var collider in hitColliders)
            {

                if (collider.CompareTag("Player"))
                {
                    PhotonView playerPV = collider.GetComponent<PhotonView>();
                    if (effectedPlayers.Contains(playerPV.ViewID)) continue;
                    effectedPlayers.Add(playerPV.ViewID);
                    if (playerPV != null)
                    {
                        playerPV.RPC("SetArcaneShield", RpcTarget.All);
                    }
                }
            }
            character.SetIsAttacking(false);
            character.RB.constraints = RigidbodyConstraints2D.FreezeRotation;

            effectedPlayers.Clear();
        });
    }

    
}
