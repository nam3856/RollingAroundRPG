using UnityEngine;
using Photon.Pun;
using Pathfinding;
using System.Collections.Generic;

public class MonsterHealth : MonoBehaviourPunCallbacks
{
    public int maxHealth = 20;
    public int experiencePoints = 1;
    private int currentHealth;
    private HashSet<int> attackers = new HashSet<int>();

    public GameObject damageTextPrefab;
    public Transform canvasTransform;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    [PunRPC]
    public void TakeDamage(int damage, int attackerViewID)
    {
        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", canvasTransform.position, Quaternion.identity);
        //GameObject damageTextInstance = Instantiate(damageTextPrefab, canvasTransform);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();

        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);

        damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString());

        if (!attackers.Contains(attackerViewID))
        {
            attackers.Add(attackerViewID);
        }
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GiveExperienceToAttackers();

            }
        }
    }

    private void GiveExperienceToAttackers()
    {
        foreach (int attackerViewID in attackers)
        {
            PhotonView attackerPV = PhotonView.Find(attackerViewID);
            if (attackerPV != null)
            {
                // 공격자의 Character 컴포넌트를 찾아서 경험치 지급
                Character attackerCharacter = attackerPV.GetComponent<Character>();
                if (attackerCharacter != null)
                {
                    attackerPV.RPC("AddExperience", attackerPV.Owner, experiencePoints);
                }
                else
                {
                    Debug.LogError("Character 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("공격자의 PhotonView를 찾을 수 없습니다. attackerViewID: " + attackerViewID);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }

}
