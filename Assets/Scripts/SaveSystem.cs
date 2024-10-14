using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string saveFilePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

    public static void SavePlayerData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            return data;
        }
        else
        {
            return null; // 저장된 데이터가 없을 경우
        }
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
