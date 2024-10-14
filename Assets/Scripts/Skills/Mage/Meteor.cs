using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Meteor : Skill
{
    private HashSet<int> effectedMonsters = new HashSet<int>();
    public Meteor(List<Skill> prerequisites) : base("���׿�", "���� ��� ������ ��� ����߸��ϴ�", 50, prerequisites, 0, 30f) { }

    protected override void ExecuteSkill(Character character)
    {
        character.SetIsAttacking(true);
        character.RB.velocity = Vector2.zero;
        Vector2 attackDirection = character.GetLastMoveDirection();
        character.PV.RPC("StartAttackingMotion", RpcTarget.All, attackDirection, 0);
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(character.transform.position, 3f);

        foreach (var collider in hitColliders)
        {
            if (!collider.CompareTag("Monster")) continue;
            PhotonView monsterPV = collider.GetComponent<PhotonView>();
            if (effectedMonsters.Contains(monsterPV.ViewID)) continue;

            Vector3 meteorPosition = new Vector3(collider.transform.position.x, collider.transform.position.y + 3f, collider.transform.position.z);
            GameObject meteor = PhotonNetwork.Instantiate("Meteor", meteorPosition, Quaternion.identity);

            MeteorMovement meteorMovement = meteor.GetComponent<MeteorMovement>();
            if (meteorMovement != null)
            {
                meteorMovement.Initialize(collider.transform, character.PV.ViewID);
            }

            effectedMonsters.Add(monsterPV.ViewID);
        }
        UniTask.Void(async () =>
        {

            await UniTask.Delay(500);
            character.SetIsAttacking(false);
            character.RB.constraints = RigidbodyConstraints2D.FreezeRotation;
            effectedMonsters.Clear();
        });
    }


}