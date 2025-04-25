using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.UIElements;

public class NumberMatchManager : Singleton<NumberMatchManager>
{


    [Header("任务一设置")]
    public GameObject taskOneObject;
    public GameObject taskOneIU;

    [Header("得分设置")]
    private int currentScore = 0;//分数
    private bool isScoreLimit = false;//得分限制

    [Header("文本UI")]
    public TMP_Text targetNumberText;//顶部目标数量UI
    public TMP_Text topScoreText;//顶部得分显示

    [Header("容器设置")]
    public Container[] containers;
    public int minBalls = 1;
    public int maxBalls = 9;
    
    [Header("音效")]
    //public AudioClip correctSound;//正确
    //public AudioClip wrongSound;//错误
    
    //私有变量
    private int targetNumber;//目标数量
    private int correctContainerIndex;//正确容器索引
    private AudioSource audioSource;
    private List<int> usedNumbers = new List<int>();//使用过的数字
    private int totalRounds = 9;//总轮次
    private int currentRound = 0;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 初始时禁用所有任务一物体
        taskOneObject.SetActive(false);
        taskOneIU.SetActive(false);
    }
    
    // 新增开始游戏方法
    public void StartGame()
    {
        currentRound = 0;
        usedNumbers.Clear();

        taskOneObject.SetActive(true); 
        taskOneIU.SetActive(true);

        SetupNewRound();
    }
    
    public void SetupNewRound()
    {
        if (currentRound >= totalRounds)
        {
            GameOver();
            return;
        }
        
        currentRound++;

        //关闭所有高亮
        foreach (var item in containers)
        {
            item.GetComponentInChildren<Outline>().enabled = false;
        }

        // 从未使用的数字中随机选择
        List<int> availableNumbers = new List<int>();
        for (int i = minBalls; i <= maxBalls; i++)
        {
            if (!usedNumbers.Contains(i))
            {
                availableNumbers.Add(i);
            }
        }
        
        if (availableNumbers.Count == 0)
        {
            GameOver();
            return;
        }
        
        // 随机选择一个未使用的数字
        int randomIndex = Random.Range(0, availableNumbers.Count);
        targetNumber = availableNumbers[randomIndex];
        usedNumbers.Add(targetNumber);
        
        targetNumberText.text = $"第{currentRound}轮：请选择数量为 {targetNumber} 的区域";
        
        // 确保至少有一个容器包含正确数量的球
        correctContainerIndex = Random.Range(0, containers.Length);
        
        // 为每个容器设置球的数量
        for (int i = 0; i < containers.Length; i++)
        {
            int ballCount;
            if (i == correctContainerIndex)
            {
                ballCount = targetNumber;
            }
            else
            {
                do
                {
                    ballCount = Random.Range(minBalls, maxBalls + 1);
                } while (ballCount == targetNumber);//再次为正确数量，重新随机
            }
            containers[i].GenerateBalls(ballCount);


            isScoreLimit = true;
        }
    }
    
    public void OnContainerSelected(int containerIndex)
    {
        if (!isScoreLimit) return;

        Debug.Log($"传过来： {containerIndex}，目标是： {targetNumber}");
        if (containerIndex == targetNumber)
        {
            isScoreLimit = false;
            // 正确选择
            Debug.Log("正确");
            //audioSource.PlayOneShot(correctSound);

            currentScore++;
            topScoreText.text = $"答对：{currentScore}";
            ShowScorePopupTool.Instance.ShowScorePopup(1);
            Invoke("SetupNewRound", 1.5f);
        }
        else
        {
            // 错误选择
            Debug.Log("错误");
            ShowScorePopupTool.Instance.ShowScorePopup(0);

            //audioSource.PlayOneShot(wrongSound);

            // 激活正确容器的outline提示
            Outline correctOutline = containers[correctContainerIndex].GetComponentInChildren<Outline>();
            if (correctOutline != null)
            {
                correctOutline.enabled = true;
                // 1秒后自动关闭outline
                DOVirtual.DelayedCall(1f, () => correctOutline.enabled = false);
            }
        }
    }

    
    private void GameOver()
    {
        Debug.Log("游戏结束！");

        taskOneObject.SetActive(false);

        targetNumberText.text = "游戏结束！";

        DOVirtual.DelayedCall(3f, () => FinishProcessing());

    }

    private void FinishProcessing()
    {
        taskOneIU.SetActive(false);
        UIController.Instance.SetGameEndUI();
    }
}