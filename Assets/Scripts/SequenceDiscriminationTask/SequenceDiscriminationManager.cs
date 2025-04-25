using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SequenceDiscriminationManager : Singleton<SequenceDiscriminationManager>
{
    [Header("任务四基础设置")]
    public GameObject taskFourObject;
    public GameObject taskFourUI;

    [Header("Json存储相关设置")]
    public string TaskName = "challengeTaskFour";
    private float endTime;

    [Header("序列设置")]
    public int sequenceLength = 4; // 随机序列长度
    public float showTime = 2.5f; // 显示时间
    private List<int> numberSequence = new List<int>(); // 存储随机序列

    [System.Serializable]
    public class BallGroup
    {
        public List<GameObject> balls = new List<GameObject>();
    }

    [Header("球体列表设置")]
    public List<BallGroup> ballGroups = new List<BallGroup>(); // 4组球体列表

    [Header("按钮容器设置")]
    public List<OptionContainer> answerContainers; // 3个答案容器

    [Header("UI设置")]
    public TMP_Text timerText;
    public TMP_Text topScoreText;
    public TMP_Text PromptText;

    public GameObject countdownObject;
    public Slider countdownSlider;//倒计时条
    public float countdownTime = 10f;//倒计时时间
    private float initialCountdownTime; // 初始倒计时时间
    private const float MIN_COUNTDOWN_TIME = 3f; // 最小倒计时时间
    private float NextUpdatedTime;//难度更新后用来记录新时间


    public float difficultyT = 60f;
    private float difficultyTimer = 0f; // 难度计时器
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
        taskFourObject.SetActive(false);
        taskFourUI.SetActive(false);
        countdownObject.SetActive(false);
        chooseNumber = true;
        HideAllBalls();
        NextUpdatedTime = countdownTime;
        initialCountdownTime = countdownTime;
    }

    private void Update()
    {
        if (!isGameActive) return;
        gameTimer += Time.deltaTime;

        // 每60秒减少倒计时时间
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyT)
        {
            if (NextUpdatedTime > MIN_COUNTDOWN_TIME)
            {
                NextUpdatedTime = Mathf.Max(MIN_COUNTDOWN_TIME, NextUpdatedTime - 1f);
                PromptText.text = $"难度提升！选择时间减少到{NextUpdatedTime:F1}秒";
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

    #region 任务四基础设置
    //开始游戏
    public void StartGame()
    {
        taskFourObject.SetActive(true);
        taskFourUI.SetActive(true);
        isGameActive = true;
        canSelectNumber = false;
        gameTimer = 0f;
        currentScore = 0;
        errorCount = 0;
        difficultyTimer = 0f;
        NextUpdatedTime = initialCountdownTime;
        countdownTime = initialCountdownTime; // 重置倒计时时间
        SetupNewRound();
    }

    private void SetupNewRound()
    {
        if (showSequence != null)
        {
            showSequence.Kill();
            showSequence = null;
        }
        GenerateSequence();
        canSelectNumber = false;
        HideAllBalls(); // 确保先重置所有球体
        
        // 重置所有容器
        foreach (var container in answerContainers)
        {
            container.gameObject.SetActive(false);
        }

        // 重置倒计时
        countdownObject.SetActive(false);
        countdownTime = NextUpdatedTime;
        currentCountdown = countdownTime;
        countdownSlider.value = 1f;

        ShowSequenceWithDOTween();
        PromptText.text = "请记住显示顺序";
    }

    private void ShowSequenceWithDOTween()
    {
        showSequence = DOTween.Sequence();
        showSequence.AppendInterval(1f);

        // 同时显示所有组的球
        showSequence.AppendCallback(() => {
            for (int i = 0; i < numberSequence.Count; i++)
            {
                ShowBallsInGroup(i, numberSequence[i]);
            }
        });

        // 显示一段时间后隐藏
        showSequence.AppendInterval(showTime);
        showSequence.AppendCallback(() => HideAllBalls());

        // 显示答案容器
        showSequence.AppendCallback(() => {
            SetupAnswerContainers();
            canSelectNumber = true; // 设置可以选择
            PromptText.text = "请选择正确的序列";

            chooseNumber = false;
            countdownObject.SetActive(true);
        });
    }

    private void GameOver()
    {
        isGameActive = false;
        canSelectNumber = false;
        endTime = gameTimer;

        chooseNumber = true;
        if (showSequence != null)
        {
            showSequence.Kill();
            showSequence = null;
        }
        taskFourObject.SetActive(false);
        TaskDataManager.Instance.SaveTaskData(TaskName, endTime, currentScore);
        DOVirtual.DelayedCall(3f, () => FinishProcessing());
    }
    private void FinishProcessing()
    {
        taskFourUI.SetActive(false);
        UIController.Instance.SetGameEndUI();
    }
    #endregion

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

    //显示要展示个数的球
    private void ShowBallsInGroup(int groupIndex, int count)
    {
        if (groupIndex >= ballGroups.Count) return;
        var group = ballGroups[groupIndex].balls;

        for (int i = 0; i < group.Count; i++)
        {
            group[i].SetActive(i < count);
        }
    }

    private void GenerateSequence()
    {
        numberSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            numberSequence.Add(Random.Range(1, 10));
        }
    }

    private void SetupAnswerContainers()
    {
        // 生成正确答案和干扰项
        List<string> allAnswers = GenerateAnswers();
        
        // 随机选择一个容器显示正确答案
        int correctIndex = Random.Range(0, answerContainers.Count);
        
        // 设置每个容器的答案
        for (int i = 0; i < answerContainers.Count; i++)
        {
            answerContainers[i].SetCurrentTriggerNum(allAnswers[i]);
            answerContainers[i].gameObject.SetActive(true);
        }
    }

    private List<string> GenerateAnswers()
    {
        List<string> answers = new List<string>();
        HashSet<string> usedAnswers = new HashSet<string>();
        
        // 添加正确答案
        string correctAnswer = string.Join("-", numberSequence);
        answers.Add(correctAnswer);
        usedAnswers.Add(correctAnswer);
        
        // 生成两个干扰项
        int maxAttempts = 10; // 防止无限循环
        for (int i = 0; i < 2; i++)
        {
            string wrongAnswer = "";
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                List<int> tempAnswer = new List<int>(numberSequence);
                int disturbType = Random.Range(1, 3);
                
                if (disturbType == 1)
                {
                    // 随机选择1-2个数字进行+1或-1操作
                    int changeCount = Random.Range(1, 3);
                    for (int j = 0; j < changeCount; j++)
                    {
                        int indexToChange = Random.Range(0, tempAnswer.Count);
                        int variation = Random.Range(0, 2) == 0 ? -1 : 1;
                        int newNum = Mathf.Clamp(tempAnswer[indexToChange] + variation, 1, 9);
                        tempAnswer[indexToChange] = newNum;
                    }
                }
                else
                {
                    // 随机选择两个位置进行交换
                    int index1 = Random.Range(0, tempAnswer.Count);
                    int index2;
                    do
                    {
                        index2 = Random.Range(0, tempAnswer.Count);
                    } while (index2 == index1);
                    
                    int temp = tempAnswer[index1];
                    tempAnswer[index1] = tempAnswer[index2];
                    tempAnswer[index2] = temp;
                }
                
                wrongAnswer = string.Join("-", tempAnswer);
                
                // 检查是否生成了新的不重复答案
                if (!usedAnswers.Contains(wrongAnswer))
                {
                    answers.Add(wrongAnswer);
                    usedAnswers.Add(wrongAnswer);
                    break;
                }
                
                attempts++;
            }
            
            // 如果实在无法生成不重复的答案，使用简单的变化
            if (attempts >= maxAttempts)
            {
                List<int> fallbackAnswer = new List<int>(numberSequence);
                fallbackAnswer[Random.Range(0, fallbackAnswer.Count)] += 1;
                wrongAnswer = string.Join("-", fallbackAnswer);
                answers.Add(wrongAnswer);
            }
        }

        // 打乱答案顺序
        for (int i = answers.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            string temp = answers[i];
            answers[i] = answers[randomIndex];
            answers[randomIndex] = temp;
        }
        
        return answers;
    }

    public void OnContainerSelected(string container)
    {
        if (!isGameActive || !canSelectNumber) return;

        canSelectNumber = false;
        string correctSequence = string.Join("-", numberSequence);

        if (container == correctSequence)
        {
            currentScore++;
            topScoreText.text = $"答对：{currentScore}";
            ShowScorePopupTool.Instance.ShowScorePopup(1);
            DOVirtual.DelayedCall(2f, SetupNewRound);
        }
        else
        {
            PromptText.text = $"选择错误！正确序列：{correctSequence}";
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

        chooseNumber = true;
    }

    private void HideAllBalls()
    {
        foreach (var group in ballGroups)
        {
            foreach (var ball in group.balls)
            {
                ball.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (showSequence != null)
        {
            showSequence.Kill();
            showSequence = null;
        }
    }
}
