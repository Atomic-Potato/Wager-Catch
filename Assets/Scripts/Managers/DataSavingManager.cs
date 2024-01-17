using System;
using System.IO;
using UnityEngine;

public class DataSavingManager : Singleton<DataSavingManager>
{
    static string _playerDataFileName = "PlayerData.json";
    public static string PlayerDataFileName => _playerDataFileName;

    new void Awake()
    {
        base.Awake();
        string path = Path.Combine(Application.persistentDataPath, _playerDataFileName);

        if (!File.Exists(path))
            CreateSaveFile();
    }

    public void CreateSaveFile()
    {
        PlayerData data = new PlayerData(GameManager.Instance.StartingBalance);
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, _playerDataFileName);
        File.WriteAllText(path, json);
    }

    public static void Save()
    {
        PlayerData data = new PlayerData(GameManager.Instance.Balance);
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, _playerDataFileName);
        File.WriteAllText(path, json);
    }   

    public static PlayerData LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, _playerDataFileName);
        string json = File.ReadAllText(path);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json); 
        return data;
    }
}

[Serializable]
public class PlayerData
{
    public int Balance;

    private PlayerData(){}
    public PlayerData(int balance)
    {
        Balance = balance;
    }
}
