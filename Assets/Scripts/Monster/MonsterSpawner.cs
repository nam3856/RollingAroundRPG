using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviourPunCallbacks
{
    public GameObject monsterPrefab; // ������ ���� ������
    public int maxMonsters = 5;      // �ִ� ���� ��
    public float spawnInterval = 5f; // ���� ���� ����

    private Collider2D spawnArea;    // ���� ����
    private List<GameObject> spawnedMonsters = new List<GameObject>(); // ���� ������ ���� ����Ʈ

    private void Start()
    {
        spawnArea = GetComponent<Collider2D>();
        if (spawnArea == null)
        {
            Debug.LogError("���� ������ ���� Collider2D�� �ʿ��մϴ�.");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating("TrySpawnMonster", 0f, spawnInterval);
        }
    }

    void TrySpawnMonster()
    {
        // ���� ���� �� Ȯ��
        spawnedMonsters.RemoveAll(item => item == null); // null�� �׸� ����

        if (spawnedMonsters.Count >= maxMonsters)
        {
            return; // �ִ� ���� ���������Ƿ� �������� ����
        }

        // ���� ��ġ ���
        Vector2 spawnPosition = GetRandomPositionInArea();

        // ���� ����
        GameObject monster = PhotonNetwork.InstantiateRoomObject(monsterPrefab.name, spawnPosition, Quaternion.identity);

        // ����Ʈ�� �߰�
        spawnedMonsters.Add(monster);
    }

    Vector2 GetRandomPositionInArea()
    {
        Bounds bounds = spawnArea.bounds;

        float minX = bounds.min.x;
        float maxX = bounds.max.x;

        float minY = bounds.min.y;
        float maxY = bounds.max.y;

        Vector2 randomPosition = new Vector2(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY)
        );

        // ����Ʈ�� �ݶ��̴� ���ο� �ִ��� Ȯ��
        if (spawnArea.OverlapPoint(randomPosition))
        {
            return randomPosition;
        }
        else
        {
            // ��� ȣ��� ������ ���� ã��
            return GetRandomPositionInArea();
        }
    }
}
