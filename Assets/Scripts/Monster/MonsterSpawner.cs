using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviourPunCallbacks
{
    public GameObject monsterPrefab; // 스폰할 몬스터 프리팹
    public int maxMonsters = 5;      // 최대 몬스터 수
    public float spawnInterval = 5f; // 몬스터 스폰 간격

    private Collider2D spawnArea;    // 스폰 구역
    private List<GameObject> spawnedMonsters = new List<GameObject>(); // 현재 스폰된 몬스터 리스트

    private void Start()
    {
        spawnArea = GetComponent<Collider2D>();
        if (spawnArea == null)
        {
            Debug.LogError("스폰 구역을 위한 Collider2D가 필요합니다.");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating("TrySpawnMonster", 0f, spawnInterval);
        }
    }

    void TrySpawnMonster()
    {
        // 현재 몬스터 수 확인
        spawnedMonsters.RemoveAll(item => item == null); // null인 항목 제거

        if (spawnedMonsters.Count >= maxMonsters)
        {
            return; // 최대 수에 도달했으므로 스폰하지 않음
        }

        // 스폰 위치 계산
        Vector2 spawnPosition = GetRandomPositionInArea();

        // 몬스터 생성
        GameObject monster = PhotonNetwork.InstantiateRoomObject(monsterPrefab.name, spawnPosition, Quaternion.identity);

        // 리스트에 추가
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

        // 포인트가 콜라이더 내부에 있는지 확인
        if (spawnArea.OverlapPoint(randomPosition))
        {
            return randomPosition;
        }
        else
        {
            // 재귀 호출로 내부의 점을 찾음
            return GetRandomPositionInArea();
        }
    }
}
