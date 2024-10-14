using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class HealingWave : Skill
{
    private HashSet<int> effectedPlayers = new HashSet<int>();

    private AudioClip SpellSoundClip = Resources.Load<AudioClip>("Sounds/Healing");
    public HealingWave(List<Skill> prerequisites) : base("ġ���� �ĵ�", "�� �ֺ��� ������ 3�ʵ��� ġ�����ݴϴ�. ������ ȸ���մϴ�.", -30, prerequisites, 0, 10f) { }

    protected override void ExecuteSkill(Character character)
    {
        character.ResetAttackState(0.6f, true).Forget();
        Vector2 attackDirection = character.GetLastMoveDirection();

        character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);

        character.audioSource.PlayOneShot(SpellSoundClip);
        UniTask.Void(async () =>
        {
            await UniTask.Delay(600);

            for (int i = 0; i < 3; i++)
            {
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
                            playerPV.RPC("GetHealing", RpcTarget.All);
                        }
                    }
                }
                effectedPlayers.Clear();
                await UniTask.Delay(1000);
            }
        });
    }

    
}
