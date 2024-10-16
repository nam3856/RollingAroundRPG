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
    PhotonView PV;
    private void Start()
    {
        currentHealth = maxHealth;
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void TakeDamage(object[] data)
    {
        int damage = (int)data[0];
        int attackerViewID = (int)data[1];
        bool isCritical = (bool)data[2];
        GameObject damageTextInstance = PhotonNetwork.Instantiate("DamageText", canvasTransform.position, Quaternion.identity);
        //GameObject damageTextInstance = Instantiate(damageTextPrefab, canvasTransform);
        DamageText damageTextScript = damageTextInstance.GetComponent<DamageText>();
        PhotonView damageTextPV = damageTextInstance.GetComponent<PhotonView>();

        GameObject uiCanvas = GameObject.Find("Canvas");
        damageTextInstance.transform.SetParent(uiCanvas.transform, false);

        damageTextPV.RPC("SetDamageText", RpcTarget.All, damage.ToString(), isCritical);

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
        else
        {
            if (data.Length>3)
            {
                PV.RPC("ApplyKnockback", RpcTarget.All, (Vector3)data[3], (float)data[4]);
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
