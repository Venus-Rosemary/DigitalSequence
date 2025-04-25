using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [Header("UI页面")]
    public GameObject StartUI;
    public GameObject ModelChooseUI;
    public GameObject CheckProgressUI;
    public GameObject EndUI;

    [Header("Button")]
    public Button TeachingButton;
    public Button StartPanelModelButton;
    public Button CheckButton;
    public Button StartPanelExitButton;
    public Button RestartButton;
    public Button ReturnButton;
    public Button EndPanelModelButton;
    public Button EndPanelExitButton;
    public Button CheckPanelReturnButton;

    [Header("当前进入的任务")]
    private int currentTaskIndex=0;

    [Header("读取到的Json")]
    public List<TaskData> tasksD = new List<TaskData>();

    [Header("进度面板设置")]
    public List<UpdataTaskProgress> taskProgressList = new List<UpdataTaskProgress>();

    void Start()
    {
        tasksD = TaskDataManager.Instance.LoadTaskDataToList();
        SetUIActiveState(true, false, false, false);
        TeachingButton.onClick.AddListener(SetTeachingButton);
        StartPanelModelButton.onClick.AddListener(SetModelButton);
        CheckButton.onClick.AddListener(SetCheckButton);
        StartPanelExitButton.onClick.AddListener(QuitGame);
        RestartButton.onClick.AddListener(SetRestartButton);
        ReturnButton.onClick.AddListener(SetReturnButton);
        EndPanelModelButton.onClick.AddListener(SetModelButton);
        EndPanelExitButton.onClick.AddListener(QuitGame);
        CheckPanelReturnButton.onClick.AddListener(SetReturnButton);
    }

    
    void Update()
    {
        
    }

    public void SetGameEndUI()
    {
        SetUIActiveState(false, false, false, true);
    }

    private void RecordLevelInformation(int index)
    {
        switch (index)
        {
            case 1://匹配任务
                NumberMatchManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 2://计数任务
                NumberCountManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 3://正序任务
                SimpleSequenceManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 4://倒序任务
                ReverseOrderManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 5://辨别任务
                SequenceDiscriminationManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 6://n-back
                NBackManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
        }
        SetUIActiveState(false, false, false, false);
    }

    private void SetUIActiveState(bool startUI,bool modelUI,bool checkUI,bool endUI)
    {
        StartUI.SetActive(startUI);
        ModelChooseUI.SetActive(modelUI);
        CheckProgressUI.SetActive(checkUI);
        EndUI.SetActive(endUI);
    }

    private void SetTeachingButton()//教学训练
    {
        SetUIActiveState(false, false, false, false);
        RecordLevelInformation(1);
    }

    private void SetModelButton()//模式选择
    {
        SetUIActiveState(false, true, false, false);
    }

    private void SetCheckButton()//查看进度
    {
        tasksD = TaskDataManager.Instance.LoadTaskDataToList();

        // 更新所有任务进度显示
        foreach (var taskProgress in taskProgressList)
        {
            taskProgress.UpdateTextData();
        }
        SetUIActiveState(false, false, true, false);
    }

    private void SetRestartButton()//重玩本关
    {
        //重玩
        if (currentTaskIndex!=0)
        {
            RecordLevelInformation(currentTaskIndex);
        }
    }

    private void SetReturnButton()//返回主菜单
    {
        SetUIActiveState(true, false, false, false);
    }



    public void QuitGame()//退出游戏
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
