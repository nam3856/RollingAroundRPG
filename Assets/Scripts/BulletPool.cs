using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviourPunCallbacks
{
    public string bulletPrefabName = "Bullet"; // Resources 폴더에 있는 총알 프리팹의 이름
    public List<GameObject> pooledPrefabs;
    void Awake()
    {
        PhotonNetwork.PrefabPool = new CustomPrefabPool(pooledPrefabs, initialSize: 30);
    }

}

// IPunPrefabPool 인터페이스 구현
public class CustomPrefabPool : IPunPrefabPool
{
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private HashSet<string> excludePoolingPrefabs = new HashSet<string>()
    {
        "Player",
        "Death",
        "FanBullet",
        "Slime"
    };

    public CustomPrefabPool(List<GameObject> prefabs, int initialSize = 10)
    {
        foreach (var prefab in prefabs)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = GameObject.Instantiate(prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(prefab.name, objectPool);
        }
    }
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (excludePoolingPrefabs.Contains(prefabId))
        {
            // 항상 새로 생성
            GameObject prefab = Resources.Load<GameObject>(prefabId);
            if (prefab == null)
            {
                Debug.LogError("Resources 폴더에서 프리팹을 찾을 수 없습니다: " + prefabId);
                return null;
            }
            obj = GameObject.Instantiate(prefab, position, rotation);
        }
        else
        {
            // 풀링 로직
            if (poolDictionary.ContainsKey(prefabId) && poolDictionary[prefabId].Count > 0)
            {
                obj = poolDictionary[prefabId].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            else
            {
                GameObject prefab = Resources.Load<GameObject>(prefabId);
                if (prefab == null)
                {
                    Debug.LogError("Resources 폴더에서 프리팹을 찾을 수 없습니다: " + prefabId);
                    return null;
                }
                obj = GameObject.Instantiate(prefab, position, rotation);
            }
        }

        // 반환하기 전에 GameObject를 비활성화합니다.
        obj.SetActive(false);

        return obj;
    }


    public void Destroy(GameObject gameObject)
    {
        string prefabId = gameObject.name.Replace("(Clone)", "").Trim();

        // 풀링 제외 프리팹인지 확인
        if (excludePoolingPrefabs.Contains(prefabId))
        {
            GameObject.Destroy(gameObject);
            return;
        }


        gameObject.SetActive(false);

        if (!poolDictionary.ContainsKey(prefabId))
        {
            poolDictionary[prefabId] = new Queue<GameObject>();
        }

        poolDictionary[prefabId].Enqueue(gameObject);
    }
}
