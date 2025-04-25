using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TaskNameEnum
{
    challengeTaskOne,
    challengeTaskTwo,
    challengeTaskThree,
    challengeTaskFour,
    challengeTaskFive
}
public class UpdataTaskProgress : MonoBehaviour
{
    public TaskNameEnum TaskName;
    public TMP_Text topScore;
    public TMP_Text topTime;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTextData()
    {

        TaskData currentData = 
            UIController.Instance.tasksD.Find(t => t.taskName == TaskName.ToString());

        topScore.text = $"��ߵ÷֣�{currentData.highestScore}";
        topTime.text = $"�ʱ�䣺{currentData.bestTime:F2}s";

    }
}
