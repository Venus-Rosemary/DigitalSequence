using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[Serializable]
public class TaskData
{
    public string taskName;
    public float bestTime;
    public int highestScore;

    public TaskData(string name, float time, int score)
    {
        taskName = name;
        bestTime = time;
        highestScore = score;
    }
}

[Serializable]
public class TaskDataCollection
{
    public List<TaskData> tasks = new List<TaskData>();
}

public class TaskDataManager : Singleton<TaskDataManager>
{
    private string SavePath => Path.Combine(Application.persistentDataPath, "taskData.json");
    private TaskDataCollection allTaskData;

    private void LoadAllData()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            allTaskData = JsonUtility.FromJson<TaskDataCollection>(json);
        }
        
        if (allTaskData == null)
        {
            allTaskData = new TaskDataCollection();
        }
    }

    public void SaveTaskData(string taskName, float time, int score)
    {
        if (allTaskData == null) LoadAllData();

        TaskData currentData = allTaskData.tasks.Find(t => t.taskName == taskName);
        bool shouldUpdate = false;

        if (currentData == null)
        {
            currentData = new TaskData(taskName, time, score);
            allTaskData.tasks.Add(currentData);
            shouldUpdate = true;
        }
        else
        {
            //存储的是最高分数，和最长时间
            if (score > currentData.highestScore)
            {

                Debug.Log($"分值大");
                currentData.highestScore = score;
                shouldUpdate = true;
            }
            
            if (time > currentData.bestTime || currentData.bestTime == 0)
            {
                Debug.Log($"时间大");
                currentData.bestTime = time;
                shouldUpdate = true;
            }
        }

        if (shouldUpdate)
        {
            string json = JsonUtility.ToJson(allTaskData, true);
            Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
            File.WriteAllText(SavePath, json);
            Debug.Log($"所有任务数据已保存: {json}");
        }
    }

    public TaskData LoadTaskData(string taskName)
    {
        if (allTaskData == null) LoadAllData();
        return allTaskData.tasks.Find(t => t.taskName == taskName);
    }

    public List<TaskData> GetAllTaskData()
    {
        if (allTaskData == null) LoadAllData();
        return allTaskData.tasks;
    }

    //读取json，并存入列表
    public List<TaskData> LoadTaskDataToList()
    {
        if (allTaskData == null) LoadAllData();

        List<TaskData> dataList = new List<TaskData>();

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            TaskDataCollection loadedData = JsonUtility.FromJson<TaskDataCollection>(json);

            if (loadedData != null && loadedData.tasks != null)
            {
                foreach (var task in loadedData.tasks)
                {
                    dataList.Add(new TaskData(
                        task.taskName,
                        task.bestTime,
                        task.highestScore
                    ));
                }
            }
        }

        return dataList;
    }
}