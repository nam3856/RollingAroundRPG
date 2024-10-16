using UnityEngine;
using TMPro;
using System.Collections;
using Photon.Pun;

public class DamageText : MonoBehaviourPun
{
    public TMP_Text damageText;
    public float moveSpeed = 0.5f;
    public float fadeSpeed = 0.5f;
    public float fadeDelay = 0.5f;
    public float duration = 0.5f;
    private PhotonView PV;

    private Color originalColor;

    void Start()
    {
        originalColor = damageText.color;
        PV = GetComponent<PhotonView>();
        PV.RPC("StartMoveUp", RpcTarget.All);
    }

    [PunRPC]
    public void SetDamageText(string damage, bool isCritical)
    {
        damageText.text = damage;
        if (isCritical)
        {
            damageText.text += "!";
            originalColor = Color.red;
            damageText.color = originalColor;
        }
    }

    [PunRPC]
    public void StartMoveUp()
    {
        StartCoroutine(MoveUpThenFadeOut());
    }
    IEnumerator MoveUpThenFadeOut()
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.localPosition;

        // 1초 동안 그냥 위로 이동 (페이드 아웃 없이)
        while (elapsedTime < fadeDelay)
        {
            elapsedTime += Time.deltaTime;

            // 텍스트가 위로 이동
            transform.localPosition = startPos + new Vector3(0, moveSpeed * elapsedTime, 0);

            yield return null;
        }

        // 페이드 아웃 시작
        elapsedTime = 0f; // 시간을 다시 초기화
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 텍스트가 계속 위로 이동
            transform.localPosition = startPos + new Vector3(0, moveSpeed * (fadeDelay + elapsedTime), 0);

            // 텍스트가 서서히 투명해짐
            Color color = damageText.color;
            color.a = Mathf.Lerp(originalColor.a, 0, elapsedTime / duration);
            damageText.color = color;

            yield return null;
        }

        // 애니메이션이 끝나면 오브젝트 삭제
        PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}
