using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReverseOrderManager : Singleton<ReverseOrderManager>
{
    [Header("��������������")]
    public GameObject taskThreeObject;
    public GameObject taskThreeUI;

    [Header("Json�洢�������")]
    public string SimpleSequenceTask = "challengeTaskThree";
    private float endTime;

    [Header("��������")]
    public int sequenceLength = 3; // ������г���
    private int initialLength;
    private const int MAX_SEQUENCE_LENGTH = 7; // ������г���

    public float showTime = 2f; // ÿ��������ʾʱ��
    private float initialShowTime; // ��ʼ��ʾʱ��
    private const float MIN_SHOW_TIME = 0.5f; // ��С��ʾʱ��

    public float intervalTime = 1f; // ��ʾ���ʱ��
    private List<int> numberSequence = new List<int>(); // �洢�������
    private int currentInputIndex = 0; // ��ǰ�������
    private float difficultyTimer = 0f; // �Ѷȼ�ʱ��
    public float difficultyT = 60f;

    [Header("3D UI����")]
    public TMP_Text numberDisplayText; // ������ʾ���ֵ�3D�ı�

    [Header("��������")]
    public List<GameObject> ballPrefab = new List<GameObject>();//�����������б�

    [Header("��ť����")]
    public List<SequenceButton> sequenceButtons = new List<SequenceButton>();//���а�ť����б�

    [Header("UI����")]
    public TMP_Text timerText;//ʱ���ı�
    public TMP_Text topScoreText;//�÷�����ı�
    public TMP_Text PromptText;//��ʾ�����ı�

    public GameObject countdownObject;
    public Slider countdownSlider;//����ʱ��
    public float countdownTime = 10f;//����ʱʱ��
    private float currentCountdown;//��ǰ����ʱ
    private bool chooseNumber = false;//ѡ�������ֹͣ����ʱ

    [Header("�÷�����")]
    private int currentScore = 0;//����
    private bool canSelectNumber;//������

    [Header("��Ϸ����")]
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

        // ÿ60���������г��Ⱥͼ�����ʾʱ��
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
                    $"�Ѷ����������г������ӵ�{sequenceLength}��" +
                    $"��ʾʱ����ٵ�{showTime:F1}��";
            }
            difficultyTimer = 0f;
        }

        // ���µ���ʱ
        if (!chooseNumber)
        {
            currentCountdown -= Time.deltaTime;
            countdownSlider.value = currentCountdown / countdownTime;

            if (currentCountdown <= 0)
            {
                // ��ʱ����
                TimeOut();
            }
        }
        timerText.text = $"��ʱ��{gameTimer:F2}��";
    }

    #region ����ʱ����
    private void TimeOut()
    {
        chooseNumber = true;
        canSelectNumber = false;

        // ������������ȷ�����ַ���
        string correctSequence = string.Join("��", numberSequence);
        PromptText.text = $"˳�������ȷ˳��{correctSequence}";

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

    #region ��������
    //��ʼ����
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
        sequenceLength = initialLength; // �������г���
        showTime = initialShowTime;
        SetupNewRound();
    }

    //��ʼ��һ��
    private void SetupNewRound()
    {
        KillAllTweens();
        GenerateSequence();
        canSelectNumber = false;
        ShowSequenceWithDOTween();
        currentInputIndex = 0;
        SetButtonsActive(false);
        PromptText.text = "���ס��ʾ˳��";
        foreach (var button in sequenceButtons)
        {
            button.ResetMaterial();
        }

        // ���õ���ʱ
        countdownObject.SetActive(false);
        currentCountdown = countdownTime;
        countdownSlider.value = 1f;
    }

    //��Ϸ����
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


    //������ж�
    private void GenerateSequence()
    {
        numberSequence.Clear();
        for (int i = 0; i < sequenceLength; i++)
        {
            numberSequence.Add(Random.Range(1, 10));
        }
    }

    //��DOTween��ʾ����
    private void ShowSequenceWithDOTween()
    {
        showSequence = DOTween.Sequence();

        // ��ʼ�ȴ�
        showSequence.AppendInterval(1f);

        int Sequ = sequenceLength;

        // ������ʾÿ������
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
        // ����չʾ���
        showSequence.OnComplete(() => {
            PromptText.text = "�밴��������ť";
            chooseNumber = false;
            countdownObject.SetActive(true);
            SetButtonsActive(true);
        });
    }

    //��ʾҪչʾ��������
    private void ShowBalls(int count)
    {
        HideAllBalls();
        for (int i = 0; i < count && i < ballPrefab.Count; i++)
        {
            ballPrefab[i].SetActive(true);
        }
    }

    //�ر�������
    private void HideAllBalls()
    {
        foreach (var ball in ballPrefab)
        {
            ball.SetActive(false);
        }
    }

    //�ر�����3d��ť
    private void SetButtonsActive(bool active)
    {
        foreach (var item in sequenceButtons)
        {
            item.gameObject.SetActive(active);
        }
    }


    //����ť�¼�
    public void OnReverseButtonClicked(int number)
    {
        if (!isGameActive || currentInputIndex >= numberSequence.Count) return;

        if (!canSelectNumber) return;

        // ��ĩβ��ʼ���
        int reverseIndex = numberSequence.Count - 1 - currentInputIndex;
        if (number == numberSequence[reverseIndex])
        {
            // ��ȷѡ�񣬸ı䵱ǰ��ť����
            sequenceButtons.Find(b => b.number == number).SetCorrectMaterial();
            currentInputIndex++;

            if (currentInputIndex >= numberSequence.Count)
            {
                currentScore++;
                topScoreText.text = $"��ԣ�{currentScore}";
                ShowScorePopupTool.Instance.ShowScorePopup(1);

                chooseNumber = true;
                DOVirtual.DelayedCall(2f, SetupNewRound);
            }
        }
        else
        {
            // ���ɵ������ȷ�����ַ���
            List<int> reverseSequence = new List<int>(numberSequence);
            reverseSequence.Reverse();
            string correctSequence = string.Join("��", reverseSequence);
            PromptText.text = $"˳�������ȷ����{correctSequence}";

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
