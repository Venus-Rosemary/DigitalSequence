using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class NBackManager : Singleton<NBackManager>
{
    [Header("任务五基础设置")]
    public GameObject taskFiveObject;
    public GameObject taskFiveUI;

    [Header("Json存储相关设置")]
    public string FiveTaskName = "challengeTaskFive";
    private float endTime;

    [Header("序列设置")]
    public int n_BackLength = 2; // n-back的长度
    public float showTime = 2f; // 每个球体显示时间
    public float intervalTime = 1f; // 显示间隔时间

    [Header("球体设置")]
    public List<GameObject> ballPrefab = new List<GameObject>();//球体预制体列表

    [Header("按钮设置")]
    //public GameObject ButtonPrefab;

    [Header("UI设置")]
    public TMP_Text timerText;//时间文本
    public TMP_Text topScoreText;//得分面板文本
    public TMP_Text PromptText;//提示面板文本

    [Header("得分设置")]
    private int currentScore = 0;//分数
    private bool canSelectNumber;//限制器

    [Header("游戏设置")]
    private float gameTimer = 0f;//游戏时间
    private int errorCount = 0;//错误次数
    private bool isGameActive = false;

    [Header("N-back设置")]
    private Queue<int> nBackQueue = new Queue<int>(); // 存储最近n+1个数
    private Sequence showSequence;
    //private bool isShowingSequence = false;

    void Start()
    {
        taskFiveObject.SetActive(false);
        taskFiveUI.SetActive(false);
        HideAllBalls();
    }
    private void Update()
    {
        if (!isGameActive) return;
        gameTimer += Time.deltaTime;
        timerText.text = $"用时：{gameTimer:F2}秒";
    }
    public void StartGame()
    {
        taskFiveObject.SetActive(true);
        taskFiveUI.SetActive(true);
        isGameActive = true;
        canSelectNumber = false;
        gameTimer = 0f;
        currentScore = 0;
        errorCount = 0;
        nBackQueue.Clear();
        SetupNewRound();
    }

    private void SetupNewRound()
    {
        if (showSequence != null)
        {
            showSequence.Kill();
            showSequence = null;
        }
        
        //isShowingSequence = true;
        ShowNextNumber();
    }

    private void ShowNextNumber()
    {
        if (!isGameActive) return;

        int nextNumber = 0;
        if (nBackQueue!=null)
        {
            //当有前n_BackLength波时，最新的一波有概率=n_BackLength前的数
            if (nBackQueue.Count>= n_BackLength)
            {

                Debug.Log($"有概率一样");
                nextNumber = Random.Range(0, 3) ==
                    0 ? nBackQueue.ElementAt(1) : Random.Range(1, 10);
            }
            else
            {
                nextNumber = Random.Range(1, 10);
            }
        }
        else
        {
            nextNumber = Random.Range(1, 10);
        }
        
        Debug.Log($"当前数是：{nextNumber}");
        // 将新数字加入队列
        nBackQueue.Enqueue(nextNumber);
        
        // 如果队列超过n+1个数，移除最早的数
        if (nBackQueue.Count > n_BackLength + 1)
        {
            nBackQueue.Dequeue();
        }

        // 显示当前数字
        HideAllBalls();
        ShowBalls(nextNumber);

        PromptText.text = $"这个数字与{n_BackLength}步之前的数字是否相同？";
        canSelectNumber = true;//可以去得分
        // 设置下一次显示
        showSequence = DOTween.Sequence();
        showSequence.AppendInterval(showTime);//显示x秒时间
        showSequence.AppendCallback(() => {
            HideAllBalls(); 
            canSelectNumber = false;
        });//关闭显示
        showSequence.AppendInterval(intervalTime);//等待x秒显示下一波
        showSequence.AppendCallback(() => ShowNextNumber());
    }

    //显示x个数的球
    private void ShowBalls(int count)
    {
        for (int i = 0; i < ballPrefab.Count; i++)
        {
            ballPrefab[i].SetActive(i < count);
        }
    }

    //关闭显示
    private void HideAllBalls()
    {
        foreach (var ball in ballPrefab)
        {
            ball.SetActive(false);
        }
    }

    public void OnButtonClicked(bool isMatch)
    {
        if (!isGameActive || !canSelectNumber) return;

        canSelectNumber = false;
        
        // 只有当队列中的数量达到n+1时才能进行判断
        if (nBackQueue.Count <= n_BackLength)
        {
            return;
        }

        // 获取当前数字和n步之前的数字
        int currentNumber = nBackQueue.Last();
        int nBackNumber = nBackQueue.ElementAt(0);
        
        bool actualMatch = (currentNumber == nBackNumber);
        
        if (isMatch == actualMatch)
        {
            // 答对了
            currentScore++;
            topScoreText.text = $"答对：{currentScore}";
            ShowScorePopupTool.Instance.ShowScorePopup(1);
        }
        else
        {
            // 答错了
            PromptText.text = $"回答错误！当前数字：{currentNumber}，{n_BackLength}步之前的数字：{nBackNumber}";
            ShowScorePopupTool.Instance.ShowScorePopup(0);
            errorCount++;

            if (errorCount >= 3)
            {
                GameOver();
                return;
            }
        }
    }

    private void GameOver()
    {
        isGameActive = false;
        canSelectNumber = false;
        //isShowingSequence = false;
        endTime = gameTimer;
        if (showSequence != null)
        {
            showSequence.Kill();
            showSequence = null;
        }
        taskFiveObject.SetActive(false);
        TaskDataManager.Instance.SaveTaskData(FiveTaskName, endTime, currentScore);
        DOVirtual.DelayedCall(3f, () => FinishProcessing());
    }
    private void FinishProcessing()
    {
        taskFiveUI.SetActive(false);
        UIController.Instance.SetGameEndUI();
    }
}
