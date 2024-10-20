using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviourPunCallbacks
{
    public string bulletPrefabName = "Bullet"; // Resources ������ �ִ� �Ѿ� �������� �̸�
    public List<GameObject> pooledPrefabs;
    void Awake()
    {
        PhotonNetwork.PrefabPool = new CustomPrefabPool(pooledPrefabs, initialSize: 30);
    }

}

// IPunPrefabPool �������̽� ����
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
            // �׻� ���� ����
            GameObject prefab = Resources.Load<GameObject>(prefabId);
            if (prefab == null)
            {
                Debug.LogError("Resources �������� �������� ã�� �� �����ϴ�: " + prefabId);
                return null;
            }
            obj = GameObject.Instantiate(prefab, position, rotation);
        }
        else
        {
            // Ǯ�� ����
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
                    Debug.LogError("Resources �������� �������� ã�� �� �����ϴ�: " + prefabId);
                    return null;
                }
                obj = GameObject.Instantiate(prefab, position, rotation);
            }
        }

        // ��ȯ�ϱ� ���� GameObject�� ��Ȱ��ȭ�մϴ�.
        obj.SetActive(false);

        return obj;
    }


    public void Destroy(GameObject gameObject)
    {
        string prefabId = gameObject.name.Replace("(Clone)", "").Trim();

        // Ǯ�� ���� ���������� Ȯ��
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
