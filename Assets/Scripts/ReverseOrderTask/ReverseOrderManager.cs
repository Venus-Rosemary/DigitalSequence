using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReverseOrderManager : Singleton<ReverseOrderManager>
{
    [Header("任务三基础设置")]
    public GameObject taskThreeObject;
    public GameObject taskThreeUI;

    [Header("Json存储相关设置")]
    public string SimpleSequenceTask = "challengeTaskThree";
    private float endTime;

    [Header("序列设置")]
    public int sequenceLength = 3; // 随机序列长度
    private int initialLength;
    private const int MAX_SEQUENCE_LENGTH = 7; // 最大序列长度

    public float showTime = 2f; // 每个数字显示时间
    private float initialShowTime; // 初始显示时间
    private const float MIN_SHOW_TIME = 0.5f; // 最小显示时间

    public float intervalTime = 1f; // 显示间隔时间
    private List<int> numberSequence = new List<int>(); // 存储随机序列
    private int currentInputIndex = 0; // 当前输入序号
    private float difficultyTimer = 0f; // 难度计时器
    public float difficultyT = 60f;

    [Header("3D UI设置")]
    public TMP_Text numberDisplayText; // 用于显示数字的3D文本

    [Header("球体设置")]
    public List<GameObject> ballPrefab = new List<GameObject>();//所有球体存放列表

    [Header("按钮设置")]
    public List<SequenceButton> sequenceButtons = new List<SequenceButton>();//所有按钮存放列表

    [Header("UI设置")]
    public TMP_Text timerText;//时间文本
    public TMP_Text topScoreText;//得分情况文本
    public TMP_Text PromptText;//提示内容文本

    public GameObject countdownObject;
    public Slider countdownSlider;//倒计时条
    public float countdownTime = 10f;//倒计时时间
    private float currentCountdown;//当前倒计时
    private bool chooseNumber = false;//选择分数后停止倒计时

    [Header("得分设置")]
    private int currentScore = 0;//分数
    private bool canSelectNumber;//限制器

    [Header("游戏设置")]
    private float gameTimer = 0f;
    private int errorCount = 0;
    private bool isGameActive = false;
    private Sequence showSequence;

    private void Start()
    {
        taskThreeObject.SetActive(false);
        taskThreeUI.SetActive(false);
        countdownObject.SetActive(false);
        chooseNumber = true;
        HideAllBalls();
        SetButtonsActive(false);
        initialLength = sequenceLength;
        initialShowTime = showTime;
        if (numberDisplayText != null)
            numberDisplayText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isGameActive) return;
        gameTimer += Time.deltaTime;

        // 每60秒增加序列长度和减少显示时间
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyT)
        {
            if (sequenceLength < MAX_SEQUENCE_LENGTH)
            {
                sequenceLength++;
            }
            if (showTime > MIN_SHOW_TIME)
            {
                showTime = Mathf.Max(MIN_SHOW_TIME, showTime - 0.1f);
                PromptText.text = 
                    $"难度提升！序列长度增加到{sequenceLength}，" +
                    $"显示时间减少到{showTime:F1}秒";
            }
            difficultyTimer = 0f;
        }

        // 更新倒计时
        if (!chooseNumber)
        {
            currentCountdown -= Time.deltaTime;
            countdownSlider.value = currentCountdown / countdownTime;

            if (currentCountdown <= 0)
            {
                // 超时处理
                TimeOut();
            }
        }
        timerText.text = $"用时：{gameTimer:F2}秒";
    }

    #region 倒计时功能
    private void TimeOut()
    {
        chooseNumber = true;
        canSelectNumber = false;

        // 生成完整的正确序列字符串
        string correctSequence = string.Join("、", numberSequence);
        PromptText.text = $"顺序错误！正确顺序：{correctSequence}";

        ShowScorePopupTool.Instance.ShowScorePopup(0);
        errorCount++;

        if (errorCount >= 3)
        {
            GameOver();
            return;
        }

        DOVirtual.DelayedCall(2f, SetupNewRound);
    }
    #endregion

    #region 任务流程
    //开始任务
    public void StartGame()
    {
        taskThreeObject.SetActive(true);
        taskThreeUI.SetActive(true);
        isGameActive = true;
        canSelectNumber = false;
        gameTimer = 0f;
        currentScore = 0;
        errorCount = 0;
        difficultyTimer = 0f;
        sequenceLength = initialLength; // 重置序列长度
        showTime = initialShowTime;
        SetupNewRound();
    }

    //开始下一轮
    private void SetupNewRound()
    {
        KillAllTweens();
        GenerateSequence();
        canSelectNumber = false;
        ShowSequenceWithDOTween();
        currentInputIndex = 0;
        SetButtonsActive(false);
        PromptText.text = "请记住显示顺序";
        foreach (var button in sequenceButtons)
        {
            button.ResetMaterial();
        }

        // 重置倒计时
        countdownObject.SetActive(false);
        currentCountdown = countdownTime;
        countdownSlider.value = 1f;
    }

    //游戏结束
    private void GameOver()
    {
        isGameActive = false;
        canSelectNumber = false;
        endTime = gameTimer;
        chooseNumber = true;
        KillAllTweens();
        taskThreeObject.SetActive(false);
        TaskDataManager.Instance.SaveTaskData(SimpleSequenceTask, endTime, currentScore);
        DOVirtual.DelayedCall(3f, () => FinishProcessing());
    }
    private void FinishProcessing()
    {
        taskThreeUI.SetActive(false);
        UIController.Instance.SetGameEndUI();
    }
    #endregion


    //随机数列队
    private void GenerateSequence()
    {
        numberSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            numberSequence.Add(Random.Range(1, 10));
        }
    }

    //用DOTween显示序列
    private void ShowSequenceWithDOTween()
    {
        showSequence = DOTween.Sequence();

        // 初始等待
        showSequence.AppendInterval(1f);

        int Sequ = sequenceLength;

        // 依次显示每个数字
        for (int i = 0; i < numberSequence.Count; i++)
        {
            int index = i;
            showSequence.AppendCallback(() => {
                if (Sequ <= 4)
                {
                    ShowBalls(numberSequence[index]);
                    numberDisplayText.gameObject.SetActive(false);
                }
                else
                {
                    HideAllBalls();
                    numberDisplayText.gameObject.SetActive(true);
                    numberDisplayText.text = numberSequence[index].ToString();
                }
            });
            showSequence.AppendInterval(showTime);
            showSequence.AppendCallback(() => {
                HideAllBalls();
                numberDisplayText.gameObject.SetActive(false);
            });
            showSequence.AppendInterval(intervalTime);
        }


        canSelectNumber = true;
        // 序列展示完成
        showSequence.OnComplete(() => {
            PromptText.text = "请按倒序点击按钮";
            chooseNumber = false;
            countdownObject.SetActive(true);
            SetButtonsActive(true);
        });
    }

    //显示要展示个数的球
    private void ShowBalls(int count)
    {
        HideAllBalls();
        for (int i = 0; i < count && i < ballPrefab.Count; i++)
        {
            ballPrefab[i].SetActive(true);
        }
    }

    //关闭所有球
    private void HideAllBalls()
    {
        foreach (var ball in ballPrefab)
        {
            ball.SetActive(false);
        }
    }

    //关闭所有3d按钮
    private void SetButtonsActive(bool active)
    {
        foreach (var item in sequenceButtons)
        {
            item.gameObject.SetActive(active);
        }
    }


    //倒序按钮事件
    public void OnReverseButtonClicked(int number)
    {
        if (!isGameActive || currentInputIndex >= numberSequence.Count) return;

        if (!canSelectNumber) return;

        // 从末尾开始检查
        int reverseIndex = numberSequence.Count - 1 - currentInputIndex;
        if (number == numberSequence[reverseIndex])
        {
            // 正确选择，改变当前按钮材质
            sequenceButtons.Find(b => b.number == number).SetCorrectMaterial();
            currentInputIndex++;

            if (currentInputIndex >= numberSequence.Count)
            {
                currentScore++;
                topScoreText.text = $"答对：{currentScore}";
                ShowScorePopupTool.Instance.ShowScorePopup(1);

                chooseNumber = true;
                DOVirtual.DelayedCall(2f, SetupNewRound);
            }
        }
        else
        {
            // 生成倒序的正确序列字符串
            List<int> reverseSequence = new List<int>(numberSequence);
            reverseSequence.Reverse();
            string correctSequence = string.Join("、", reverseSequence);
            PromptText.text = $"顺序错误！正确倒序：{correctSequence}";

            canSelectNumber = false;
            ShowScorePopupTool.Instance.ShowScorePopup(0);
            errorCount++;

            chooseNumber = true;
            if (errorCount >= 3)
            {
                GameOver();
                return;
            }
            DOVirtual.DelayedCall(2f, SetupNewRound);
        }
    }


    private void KillAllTweens()
    {
        if (showSequence != null)
        {
            showSequence.Kill();
            showSequence = null;
        }
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

}
