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
        // ItemInstance를 ItemInstanceData로 변환
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

        // EquippedItems도 동일하게 처리
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

            // ItemInstanceData를 ItemInstance로 변환
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
                    Debug.LogWarning($"BaseItem을 찾을 수 없습니다. ID: {itemData.baseItemId}");
                }
            }

            // EquippedItems도 동일하게 처리
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
                    Debug.LogWarning($"BaseItem을 찾을 수 없습니다. ID: {itemData.baseItemId}");
                }
            }

            return data;
        }
        else
        {
            return null; // 저장된 데이터가 없을 경우
        }
    }


    private static int CURRENT_DATA_VERSION = 1;

    private static PlayerData MigratePlayerData(PlayerData oldData)
    {
        // 새로운 버전의 PlayerData 생성
        PlayerData newData = new PlayerData();

        // 기존 데이터 복사 및 변환
        newData.LearnedTraits = oldData.LearnedTraits ?? new List<string>();

        // 버전에 따른 마이그레이션 로직
        if (oldData.version < CURRENT_DATA_VERSION)
        {

        }

        // 버전 업데이트
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
            Debug.Log("세이브 데이터가 삭제되었습니다.");
        }
        else
        {
            Debug.LogWarning("삭제할 세이브 데이터가 없습니다.");
        }
    }
}
