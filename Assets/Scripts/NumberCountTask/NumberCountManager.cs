using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class NumberCountManager : Singleton<NumberCountManager>
{
    [Header("挑战任务一设置")]
    public GameObject taskTwoObject;
    public GameObject taskTwoUI;

    [Header("Json存储数据设置")]
    public string NumberCountTask = "challengeTaskOne";
    private float endTime;//用来记录结束时间，并用于josn

    [Header("球体设置")]
    public GameObject ballPrefab;//球的预制体
    public int minBalls = 1;
    public int maxBalls = 9;

    public float ballRadius = 0.5f; // 球体间距
    public int maxSpawnAttempts = 50; // 最大尝试生成次数

    [Header("难度设置")]
    public float improveTime = 60f;//每隔xx秒增加难度
    public Vector2 IncreaseTheNumber = new Vector2(3, 6);//每次难度增加范围
    private int GrowthCoefficient=0;//随时间的一个增长系数 minBalls * GrowthCoefficient  maxBalls * GrowthCoefficient
    private float difficultyTimer = 0f;//难度计时器

    [Header("范围限制")]
    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 yRange = new Vector2(0f, 9f);

    [Header("容器设置")]
    public ABCDTrigger[] abcdTriggers;

    [Header("UI设置")]
    public TMP_Text timerText;//游戏时间文本
    public TMP_Text topScoreText;//得分数文本
    public TMP_Text PromptText;//正确数量提示文本

    public Slider countdownSlider;//倒计时条
    public float countdownTime = 10f;//倒计时时间
    private float currentCountdown;//当前倒计时
    private bool chooseNumber = false;//选择分数后停止倒计时

    [Header("得分设置")]
    private int currentScore = 0;//分数
    private bool canSelectNumber;//点击限制器


    private List<GameObject> activeBalls = new List<GameObject>();//所有活跃的球体
    [SerializeField] private int currentBallCount;//当前生成球的数量
    private int clickedBallCount;//点击的球的数量
    private bool isRoundActive;
    
    [Header("鼠标跟随设置")]
    public GameObject mouseFollower; // 跟随鼠标的物体
    private Camera mainCamera;
    private Plane mousePlane; // 用于鼠标射线检测的平面

    [Header("游戏设置")]
    private float gameTimer = 0f; // 游戏计时器
    private int errorCount = 0; // 错误次数
    private bool isGameActive = false; // 游戏是否进行中
    
    private void Start()
    {
        mainCamera = Camera.main;
        mousePlane = new Plane(Vector3.forward, Vector3.forward * taskTwoObject.transform.position.z);
        mouseFollower.SetActive(false);
        taskTwoObject.SetActive(false);
        taskTwoUI.SetActive(false);
    }

    private void Update()
    {
        if (!isGameActive) return;

        // 更新计时器
        gameTimer += Time.deltaTime;

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
        

        //增加难度系数 每60秒增加一次难度
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= improveTime)
        {
            difficultyTimer = 0f;
            GrowthCoefficient += 1;
        }

        timerText.text = $"用时：{gameTimer:F2}秒";

        // 更新鼠标跟随物体位置
        UpdateMouseFollower();
    }

    #region 物体跟随鼠标移动
    private void UpdateMouseFollower()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (mousePlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            // 限制在生成范围内
            hitPoint.x = Mathf.Clamp(hitPoint.x, xRange.x, xRange.y);
            hitPoint.y = Mathf.Clamp(hitPoint.y, yRange.x, yRange.y);
            mouseFollower.transform.position = hitPoint;
        }
    }
    #endregion


    #region 倒计时功能
    private void TimeOut()
    {
        chooseNumber = true;
        canSelectNumber=false;
        PromptText.text= $"超时！正确答案是：{currentBallCount}";
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
    public void StartGame()
    {
        taskTwoObject.SetActive(true);
        taskTwoUI.SetActive(true);
        mouseFollower.SetActive(true);
        isGameActive = true;
        gameTimer = 0f;
        difficultyTimer = 0f;
        GrowthCoefficient = 0;
        currentScore = 0;
        errorCount = 0;
        SetupNewRound();
    }
    

    //新一轮开始
    private void SetupNewRound()
    {
        // 清理上一轮的球
        ClearBalls();
        SetabcdTriggersEnable(false);

        //重置文本
        PromptText.text = $"请选择正确的数量";

        int currentMinBalls = minBalls + (GrowthCoefficient * (int)IncreaseTheNumber.x);
        int currentMaxBalls = maxBalls + (GrowthCoefficient * (int)IncreaseTheNumber.y);
        // 生成新的球
        currentBallCount = Random.Range(currentMinBalls, currentMaxBalls + 1);
        SpawnBalls();
        
        // 重置计时器

        isRoundActive = true;
        canSelectNumber = false;
        clickedBallCount = 0;


        // 重置倒计时
        chooseNumber = false;
        currentCountdown = countdownTime;
        countdownSlider.value = 1f;
    }

    private void GameOver()
    {
        isGameActive = false;
        canSelectNumber = false;
        mouseFollower.SetActive(false);
        taskTwoObject.SetActive(false);

        endTime= gameTimer;

        Debug.Log($"游戏结束！\n用时：{gameTimer:F2}秒");

        // 保存任务数据
        TaskDataManager.Instance.SaveTaskData(NumberCountTask, endTime, currentScore);

        DOVirtual.DelayedCall(3f, () => FinishProcessing());
    }

    private void FinishProcessing()
    {
        taskTwoUI.SetActive(false);
        UIController.Instance.SetGameEndUI();
    }
    #endregion


    #region 容器范围内球的生成
    //球的生成
    private void SpawnBalls()
    {
        List<Vector3> occupiedPositions = new List<Vector3>();

        for (int i = 0; i < currentBallCount; i++)
        {
            Vector3 position = GetValidBallPosition(occupiedPositions);

            // 如果找不到有效位置，跳过当前球的生成
            if (position == Vector3.zero)
            {
                Debug.LogWarning("无法找到有效的生成位置，当前球体生成可能重叠");
                position = new Vector3(
                    Random.Range(xRange.x, xRange.y),
                    Random.Range(yRange.x, yRange.y),
                    taskTwoObject.transform.position.z);
            }

            GameObject ball = Instantiate(ballPrefab, position, Quaternion.identity, taskTwoObject.transform);
            ball.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);

            BallTrigger triggerComponent = ball.AddComponent<BallTrigger>();
            triggerComponent.OnBallTriggered += OnBallClicked;

            activeBalls.Add(ball);
            occupiedPositions.Add(position);
        }
    }

    private Vector3 GetValidBallPosition(List<Vector3> occupiedPositions)
    {
        float minDistance = ballRadius * 2; // 最小间距为球体直径

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 position = new Vector3(
                Random.Range(xRange.x, xRange.y),
                Random.Range(yRange.x, yRange.y),
                taskTwoObject.transform.position.z
            );

            bool isValidPosition = true;

            // 检查与已有球体的距离
            foreach (Vector3 occupiedPos in occupiedPositions)
            {
                if (Vector3.Distance(position, occupiedPos) < minDistance)
                {
                    isValidPosition = false;
                    break;
                }
            }

            if (isValidPosition)
            {
                return position;
            }
        }

        return Vector3.zero; // 如果找不到有效位置，返回零向量
    }
    #endregion


    #region 球的事件
    //点击球的事件
    private void OnBallClicked(GameObject ball)
    {
        if (!isRoundActive) return;
        
        clickedBallCount++;
        
        // 球体消失动画
        ball.transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                activeBalls.Remove(ball);
                Destroy(ball);
                
                // 检查是否点击完所有球
                if (clickedBallCount >= currentBallCount)
                {
                    ShowNumberSelection();
                    isRoundActive=false;
                }
            });
    }
    

    //点击完所有球体
    private void ShowNumberSelection()
    {
        canSelectNumber = true;

        // 生成4个不重复的数字选项
        List<int> numbers = new List<int>();
        numbers.Add(currentBallCount); // 添加正确答案
        
        // 生成3个干扰项
        while (numbers.Count < 4)
        {
            int offset = Random.Range(-3, 4); // 随机生成-3到3的偏移量
            if (offset == 0) continue; // 跳过0，避免重复
            
            int newNumber = currentBallCount + offset;
            // 确保数字在合理范围内且不重复
            if (newNumber > 0 && !numbers.Contains(newNumber))
            {
                numbers.Add(newNumber);
            }
        }
        
        // 打乱顺序
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }
        
        // 设置每个触发器的数字
        for (int i = 0; i < abcdTriggers.Length; i++)
        {
            abcdTriggers[i].SetCurrentTriggerNum(numbers[i]);//传递数量
        }

        DOVirtual.DelayedCall(0.3f, () => SetabcdTriggersEnable(true));//激活
    }

    //设置abcdTriggers的激活关闭
    private void SetabcdTriggersEnable(bool ActivateOrTurnOff)
    {
        for (int i = 0; i < abcdTriggers.Length; i++)
        {
            abcdTriggers[i].gameObject.SetActive(ActivateOrTurnOff);
        }
    }
    #endregion


    #region 判断对错
    //判断正误
    public void OnNumberSelected(int selectedNumber)
    {
        if (!canSelectNumber) return;
        canSelectNumber = false;

        Debug.Log($"传过来： {selectedNumber}，目标是： {currentBallCount}");
        if (selectedNumber == currentBallCount)
        {
            currentScore++;
            topScoreText.text = $"答对：{currentScore}";
            ShowScorePopupTool.Instance.ShowScorePopup(1);
        }
        else
        {
            PromptText.text = $"错误！正确答案是：{currentBallCount}";

            ShowScorePopupTool.Instance.ShowScorePopup(0);
            errorCount++;

            if (errorCount >= 3)
            {
                GameOver();
                return;
            }
        }
        chooseNumber = true;
        DOVirtual.DelayedCall(2f, SetupNewRound);
    }
    #endregion

    //清理掉生成的球体
    private void ClearBalls()
    {
        foreach (var ball in activeBalls)
        {
            Destroy(ball);
        }
        activeBalls.Clear();
    }
    
    private void OnDestroy()
    {

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 绘制移动边界
        Gizmos.color = Color.red;
        Vector3 center = new Vector3(
            taskTwoObject.transform.position.x,
            4.5f,
            taskTwoObject.transform.position.z
        );
        Vector3 size = new Vector3(
            xRange.y*2,
            yRange.y,
            3f
        );
        Gizmos.DrawWireCube(center, size);

    }
#endif
}