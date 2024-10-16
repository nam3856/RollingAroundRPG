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

        // 1�� ���� �׳� ���� �̵� (���̵� �ƿ� ����)
        while (elapsedTime < fadeDelay)
        {
            elapsedTime += Time.deltaTime;

            // �ؽ�Ʈ�� ���� �̵�
            transform.localPosition = startPos + new Vector3(0, moveSpeed * elapsedTime, 0);

            yield return null;
        }

        // ���̵� �ƿ� ����
        elapsedTime = 0f; // �ð��� �ٽ� �ʱ�ȭ
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // �ؽ�Ʈ�� ��� ���� �̵�
            transform.localPosition = startPos + new Vector3(0, moveSpeed * (fadeDelay + elapsedTime), 0);

            // �ؽ�Ʈ�� ������ ��������
            Color color = damageText.color;
            color.a = Mathf.Lerp(originalColor.a, 0, elapsedTime / duration);
            damageText.color = color;

            yield return null;
        }

        // �ִϸ��̼��� ������ ������Ʈ ����
        PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void DestroyRPC()
    {
        Destroy(gameObject);
    }
}
