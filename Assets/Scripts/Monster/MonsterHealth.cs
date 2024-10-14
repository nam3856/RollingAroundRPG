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
                // �������� Character ������Ʈ�� ã�Ƽ� ����ġ ����
                Character attackerCharacter = attackerPV.GetComponent<Character>();
                if (attackerCharacter != null)
                {
                    attackerPV.RPC("AddExperience", attackerPV.Owner, experiencePoints);
                }
                else
                {
                    Debug.LogError("Character ������Ʈ�� ã�� �� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError("�������� PhotonView�� ã�� �� �����ϴ�. attackerViewID: " + attackerViewID);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }

}
