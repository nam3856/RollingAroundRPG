using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Meteor : Skill
{
    private HashSet<int> effectedMonsters = new HashSet<int>();
    public Meteor(List<Skill> prerequisites) : base("메테오", "주위 모든 적에게 운석을 떨어뜨립니다", 50, prerequisites, 0, 30f) {

        icon = Resources.Load<Sprite>("Icons/Mage_Skill4");
    }

    protected override void ExecuteSkill(Character character)
    {
        character.ResetAttackState(1f, true).Forget();

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
                meteorMovement.Initialize(collider.transform, character.PV.ViewID, critical);
            }

            effectedMonsters.Add(monsterPV.ViewID);
        }
        effectedMonsters.Clear();
    }


}