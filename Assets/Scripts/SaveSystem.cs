using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static ItemDatabase itemDatabase = Resources.Load<ItemDatabase>("Item/ItemDatabase");
    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

    private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.Indented
    };

    public static void SavePlayerData(PlayerData data)
    {
        // ItemInstance�� ItemInstanceData�� ��ȯ
        data.InventoryItemsData = new List<ItemInstanceData>();
        foreach (var itemInstance in data.InventoryItems)
        {
            ItemInstanceData itemData = new ItemInstanceData
            {
                instanceId = itemInstance.instanceId,
                baseItemId = itemInstance.baseItem.id,
                quantity = itemInstance.quantity
            };
            data.InventoryItemsData.Add(itemData);
        }

        // EquippedItems�� �����ϰ� ó��
        data.EquippedItemsData = new List<ItemInstanceData>();
        foreach (var itemInstance in data.EquippedItems)
        {
            ItemInstanceData itemData = new ItemInstanceData
            {
                instanceId = itemInstance.instanceId,
                baseItemId = itemInstance.baseItem.id,
                quantity = itemInstance.quantity
            };
            data.EquippedItemsData.Add(itemData);
        }


        string json = JsonConvert.SerializeObject(data, jsonSettings);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Save Complete");
    }


    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonConvert.DeserializeObject<PlayerData>(json, jsonSettings);

            // ItemInstanceData�� ItemInstance�� ��ȯ
            data.InventoryItems = new List<ItemInstance>();
            foreach (var itemData in data.InventoryItemsData)
            {
                BaseItem baseItem = itemDatabase.items.Find(item => item.id == itemData.baseItemId);
                if (baseItem != null)
                {
                    ItemInstance itemInstance = new ItemInstance(baseItem, itemData.quantity)
                    {
                        instanceId = itemData.instanceId
                    };
                    data.InventoryItems.Add(itemInstance);
                }
                else
                {
                    Debug.LogWarning($"BaseItem�� ã�� �� �����ϴ�. ID: {itemData.baseItemId}");
                }
            }

            // EquippedItems�� �����ϰ� ó��
            data.EquippedItems = new List<ItemInstance>();
            foreach (var itemData in data.EquippedItemsData)
            {
                BaseItem baseItem = itemDatabase.items.Find(item => item.id == itemData.baseItemId);
                if (baseItem != null)
                {
                    ItemInstance itemInstance = new ItemInstance(baseItem, itemData.quantity)
                    {
                        instanceId = itemData.instanceId
                    };
                    data.EquippedItems.Add(itemInstance);
                }
                else
                {
                    Debug.LogWarning($"BaseItem�� ã�� �� �����ϴ�. ID: {itemData.baseItemId}");
                }
            }

            return data;
        }
        else
        {
            return null; // ����� �����Ͱ� ���� ���
        }
    }


    private static int CURRENT_DATA_VERSION = 1;

    private static PlayerData MigratePlayerData(PlayerData oldData)
    {
        // ���ο� ������ PlayerData ����
        PlayerData newData = new PlayerData();

        // ���� ������ ���� �� ��ȯ
        newData.LearnedTraits = oldData.LearnedTraits ?? new List<string>();

        // ������ ���� ���̱׷��̼� ����
        if (oldData.version < CURRENT_DATA_VERSION)
        {

        }

        // ���� ������Ʈ
        newData.version = CURRENT_DATA_VERSION;

        return newData;
    }



    public static bool PlayerDataExists()
    {
        return File.Exists(saveFilePath);
    }

    public static void DeletePlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("���̺� �����Ͱ� �����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("������ ���̺� �����Ͱ� �����ϴ�.");
        }
    }
}
