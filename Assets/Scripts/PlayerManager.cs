using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager instance;

    public PlayerScript localPlayerCharacter;

    private void Awake()
    {
        instance = this;
    }

    public void RespawnPlayer()
    {
        Vector3 respawnPosition = new Vector3(-59, 14.54f, 0);

        if (PlayerManager.instance != null && PlayerManager.instance.localPlayerCharacter != null)
        {
            PlayerManager.instance.localPlayerCharacter.RespawnCharacter(respawnPosition);
        }
        else
        {
            Debug.LogError("플레이어 캐릭터를 찾을 수 없습니다.");
        }
    }
}
