using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : Singleton<UIController>
{
    [Header("UIҳ��")]
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

    [Header("��ǰ���������")]
    private int currentTaskIndex=0;

    [Header("��ȡ����Json")]
    public List<TaskData> tasksD = new List<TaskData>();

    [Header("�����������")]
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
            case 1://ƥ������
                NumberMatchManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 2://��������
                NumberCountManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 3://��������
                SimpleSequenceManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 4://��������
                ReverseOrderManager.Instance.StartGame();
                currentTaskIndex = index;
                break;
            case 5://�������
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

    private void SetTeachingButton()//��ѧѵ��
    {
        SetUIActiveState(false, false, false, false);
        RecordLevelInformation(1);
    }

    private void SetModelButton()//ģʽѡ��
    {
        SetUIActiveState(false, true, false, false);
    }

    private void SetCheckButton()//�鿴����
    {
        tasksD = TaskDataManager.Instance.LoadTaskDataToList();

        // �����������������ʾ
        foreach (var taskProgress in taskProgressList)
        {
            taskProgress.UpdateTextData();
        }
        SetUIActiveState(false, false, true, false);
    }

    private void SetRestartButton()//���汾��
    {
        //����
        if (currentTaskIndex!=0)
        {
            RecordLevelInformation(currentTaskIndex);
        }
    }

    private void SetReturnButton()//�������˵�
    {
        SetUIActiveState(true, false, false, false);
    }



    public void QuitGame()//�˳���Ϸ
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
