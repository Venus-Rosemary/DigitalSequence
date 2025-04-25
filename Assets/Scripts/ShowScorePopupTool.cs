using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowScorePopupTool : Singleton<ShowScorePopupTool>
{
    [Header("得分UI设置")]
    public GameObject scorePopupPrefab;//得分3dUI预制体
    public Transform playerTransform;//玩家位置
    public float uiMoveSpeed = 1f;
    public float uiFadeTime = 1f;

    [Header("3dUI对象池设置")]
    public int poolSize = 5; // 每种特效的对象池大小
    public Transform poolParent;//对象存放位置
    private Queue<GameObject> uiPool = new Queue<GameObject>();

    private void Start()
    {
        InitializeUIPool();
    }

    private void InitializeUIPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ui = Instantiate(scorePopupPrefab, poolParent);
            ui.SetActive(false);
            uiPool.Enqueue(ui);
        }
    }
    public void ShowScorePopup(int addScore)
    {
        if (scorePopupPrefab != null && playerTransform != null)
        {

            if (uiPool.Count == 0) return;

            // 在玩家位置上方生成得分UI
            Vector3 spawnPosition = playerTransform.position + Vector3.up * 2f;
            GameObject popup = uiPool.Dequeue();
            popup.transform.position = spawnPosition;
            popup.SetActive(true);

            // 设置文本
            TMP_Text scoreText = popup.GetComponentInChildren<TMP_Text>();

            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (scoreText != null)
            {
                scoreText.text = $"+{addScore}";
                canvasGroup.alpha = 1f;

                // 创建动画序列
                Sequence sequence = DOTween.Sequence();

                // 向上移动
                sequence.Join(popup.transform.DOMoveY(popup.transform.position.y + 1f, uiMoveSpeed));
                // 渐隐
                sequence.Join(canvasGroup.DOFade(0, uiFadeTime));

                // 动画完成后回收到对象池
                sequence.OnComplete(() =>
                {
                    popup.SetActive(false);
                    uiPool.Enqueue(popup);
                });
            }
        }
    }
}
