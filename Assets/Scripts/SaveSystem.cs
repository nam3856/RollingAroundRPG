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
            return null; // ����� �����Ͱ� ���� ���
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
            Debug.Log("���̺� �����Ͱ� �����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("������ ���̺� �����Ͱ� �����ϴ�.");
        }
    }
}
